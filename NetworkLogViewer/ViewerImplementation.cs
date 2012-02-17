using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Interop;
using Kamilla;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.Network.Viewing.Plugins;
using Kamilla.WPF;

namespace NetworkLogViewer
{
    internal sealed class ViewerImplementation : NetworkLogViewerBase
    {
        bool m_autoParse;
        bool m_deallocQueueEnabled;

        volatile Protocol m_currentProtocol;
        NetworkLog m_currentLog;
        internal readonly ViewerItemCollection m_items;

        readonly MainWindow m_window;
        readonly PacketAddedEventHandler m_packetAddedHandler;
        readonly WindowInteropHelper m_interopHelper;
        readonly BackgroundWorker m_parsingWorker;
        INetworkLogViewerPlugin[] m_plugins;
        readonly List<PluginCommand> m_pluginCommands;
        readonly Queue<ViewerItem> m_parsingQueue = new Queue<ViewerItem>(64);

        const int s_maxAllocations = 1024;
        readonly Queue<ViewerItem> m_dataItems = new Queue<ViewerItem>(s_maxAllocations);
        readonly Queue<ViewerItem> m_parserItems = new Queue<ViewerItem>(s_maxAllocations);

        internal ViewerImplementation(MainWindow window)
        {
            m_window = window;
            m_interopHelper = new WindowInteropHelper(window);

            m_items = new ViewerItemCollection(this);
            m_items.ItemQueried += new ViewerItemEventHandler(m_items_ItemQueried);
            m_packetAddedHandler = new PacketAddedEventHandler(m_currentLog_PacketAdded);

            m_parsingWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            m_parsingWorker.DoWork += new DoWorkEventHandler(m_parsingWorker_DoWork);
            m_pluginCommands = new List<PluginCommand>();
        }

        internal void LoadSettings()
        {
            m_autoParse = Configuration.GetValue("AutoParse", true);
            m_deallocQueueEnabled = Configuration.GetValue("DeallocQueue", true);
        }

        internal void SaveSettings()
        {
            Configuration.SetValue("AutoParse", m_autoParse);
            Configuration.SetValue("DeallocQueue", m_deallocQueueEnabled);
        }

        internal bool AutoParse
        {
            get { return m_autoParse; }
            set
            {
                m_autoParse = value;

                if (value)
                    m_items.Update();
            }
        }

        internal bool EnableDeallocQueue
        {
            get { return m_deallocQueueEnabled; }
            set
            {
                m_deallocQueueEnabled = value;

                if (value)
                    this.DropCache();
                else
                {
                    lock (m_parserItems)
                        m_parserItems.Clear();

                    lock (m_dataItems)
                        m_dataItems.Clear();
                }
            }
        }

        void m_currentLog_PacketAdded(object sender, PacketAddedEventArgs e)
        {
            var item = new ViewerItem(this, (NetworkLog)sender, e.Packet, m_items.Count);
            m_items.Add(item);

            if (this.ItemAdded != null)
                this.ItemAdded(this, new ViewerItemEventArgs(item));
        }

        void m_items_ItemQueried(object sender, ViewerItemEventArgs e)
        {
            if (m_autoParse)
            {
                var item = e.Item;
                var parser = item.Parser;
                if (parser == null || !parser.IsParsed)
                    this.EnqueueParsing(item);
            }

            if (this.ItemQueried != null)
            {
                m_window.ThreadSafeBegin(_ =>
                {
                    if (this.ItemQueried != null)
                        this.ItemQueried(sender, e);
                });
            }
        }

        internal void DropCache()
        {
            lock (m_parserItems)
                m_parserItems.Clear();

            lock (m_dataItems)
                m_dataItems.Clear();

            foreach (var item in m_items)
            {
                item.Parser = null;
                item.VisualData = null;
            }
        }

        internal void SetProtocol(Protocol value)
        {
            var old = m_currentProtocol;
            if (old == value)
                return;

            if (old != null && value != null &&
                old.Wrapper == value.Wrapper)
            {
                Console.WriteLine("Error: Got same protocol {0}", value.Name);
                return;
            }

            this.StopParsing();

            // We should allow the protocol to integrate with viewer in viewer's thread.
            m_window.ThreadSafe(_ =>
            {
                if (old != null)
                    old.Unload();

                m_currentProtocol = value;

                if (value != null)
                    value.Load(this);

                this.DropCache();
            });

            Console.WriteLine("Debug: Switching Protocol:{0}       Old: {1}{2}       New: {3}",
                Environment.NewLine, old != null ? old.Name : "null",
                Environment.NewLine, value != null ? value.Name : "null");

            if (this.ProtocolChanged != null)
                this.ProtocolChanged(this, EventArgs.Empty);
        }

        internal void HookLog(NetworkLog value)
        {
            if (value != null)
                value.PacketAdded += m_packetAddedHandler;
        }

        internal void SetLog(NetworkLog value)
        {
            if (m_currentLog == value)
                return;

            this.StopParsing();

            var old = m_currentLog;
            if (old != null)
                old.PacketAdded -= m_packetAddedHandler;

            m_currentLog = value;

            if (this.NetworkLogChanged != null)
                this.NetworkLogChanged(this, EventArgs.Empty);

            if (m_currentLog != null)
                this.StartParsing();
        }

        internal void CloseFile()
        {
            m_items.Clear();

            lock (m_dataItems)
                m_dataItems.Clear();

            lock (m_parserItems)
                m_parserItems.Clear();

            this.SetLog(null);

            // We've got a lot of unreferenced object instances
            // at this point, and we can collect them instantly.
            GC.Collect();
        }

        protected override void OnItemVisualDataChanged(ViewerItem item, object oldData, object newData)
        {
            base.OnItemVisualDataChanged(item, oldData, newData);

            if (oldData == null && newData != null)
            {
                lock (m_dataItems)
                {
                    if (m_dataItems.Count >= s_maxAllocations)
                        m_dataItems.Dequeue().VisualData = null;

                    m_dataItems.Enqueue(item);
                }
            }
        }

        protected override void OnItemParserChanged(ViewerItem item, PacketParser oldParser, PacketParser newParser)
        {
            base.OnItemParserChanged(item, oldParser, newParser);

            if (oldParser == null && newParser != null)
            {
                lock (m_parserItems)
                {
                    if (m_parserItems.Count >= s_maxAllocations)
                        m_parserItems.Dequeue().Parser = null;

                    m_parserItems.Enqueue(item);
                }
            }
        }

        #region Plugins
        internal void InitializePlugins()
        {
            m_plugins = PluginManager.CreatePluginSet();

            m_window.ThreadSafeBegin(_ =>
            {
                var impl = _.Implementation;
                foreach (var plugin in impl.m_plugins)
                    plugin.Initialize(impl);
            });
        }

        /// <summary>
        /// Registers a <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/>.
        /// </summary>
        /// <param name="command">
        /// The <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> that should be registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <c>command</c> is null.
        /// </exception>
        public override void RegisterPluginCommand(PluginCommand command)
        {
            if (m_pluginCommands.Contains(command))
                throw new ArgumentException("Such command is already registered.", "command");

            m_pluginCommands.Add(command);

            m_window.ThreadSafeBegin(_ =>
            {
                var item = new MenuItem();

                item.Header = command.Title;
                item.InputGestureText = command.Gesture.GetDisplayString();
                item.Tag = command;
                item.Click += (o, e) =>
                {
                    e.Handled = true;
                    command.Callback();
                };

                _.ui_miPlugins.Items.Add(item);
                _.ui_miPlugins.IsEnabled = true;
            });
        }

        /// <summary>
        /// Unregisters a <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/>.
        /// </summary>
        /// <param name="command">
        /// The <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> that should be unregistered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <c>command</c> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The provided <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> is not
        /// registered with the current <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
        /// </exception>
        public override void UnregisterPluginCommand(PluginCommand command)
        {
            if (!m_pluginCommands.Contains(command))
                throw new ArgumentException("Such command is not registered.", "command");

            m_pluginCommands.Remove(command);

            m_window.ThreadSafeBegin(_ =>
            {
                _.ui_miPlugins.Items.Remove(
                    _.ui_miPlugins.Items
                        .Cast<MenuItem>()
                        .Single(item => (PluginCommand)((MenuItem)item).Tag == command)
                );

                if (!_.ui_miPlugins.HasItems)
                    _.ui_miPlugins.IsEnabled = false;
            });
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Retrieves the object that is responsible for the user interface.
        /// </summary>
        public override object InterfaceObject { get { return m_window; } }

        /// <summary>
        /// Gets the collection of items currently loaded.
        /// </summary>
        public override IList<ViewerItem> Items { get { return m_items; } }

        public override ViewerItem SelectedItem { get { return m_window.SelectedItem; } }

        /// <summary>
        /// Gets or sets the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public override Protocol CurrentProtocol { get { return m_currentProtocol; } }

        /// <summary>
        /// Gets the currently loaded <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public override NetworkLog CurrentLog { get { return m_currentLog; } }

        /// <summary>
        /// Gets the handle of the window.
        /// </summary>
        public override IntPtr WindowHandle { get { return m_interopHelper.Handle; } }

        /// <summary>
        /// Occurs when <see cref="NetworkLogViewer.MainWindow.CurrentProtocol"/> changes.
        /// 
        /// Handlers of this event are called from any suiting thread.
        /// </summary>
        public override event EventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see cref="NetworkLogViewer.MainWindow.CurrentLog"/> property changes.
        /// 
        /// Handlers of this event are called from any suiting thread.
        /// </summary>
        public override event EventHandler NetworkLogChanged;

        /// <summary>
        /// Occurs when data of a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is queried.
        /// 
        /// Handlers of this event are called from the UI thread.
        /// </summary>
        public override event ViewerItemEventHandler ItemQueried;

        /// <summary>
        /// Occurs when a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is added.
        /// 
        /// Handlers of this event are called from any suiting thread.
        /// </summary>
        public override event ViewerItemEventHandler ItemAdded;
        #endregion

        #region Parsing
        void m_parsingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // THREADING DANGER ZONE!
            UICulture.Initialize();

            var worker = (BackgroundWorker)sender;
            var protocol = m_currentProtocol;
            int turnOffTimes = 0;

            while (!worker.CancellationPending)
            {
                ViewerItem item = null;

                lock (m_parsingQueue)
                {
                    if (m_parsingQueue.Count > 0)
                        item = m_parsingQueue.Dequeue();
                }

                if (item == null)
                {
                    ++turnOffTimes;
                    if (turnOffTimes == 50)
                        return;

                    Thread.Sleep(100);
                    continue;
                }

                if (item.Viewer != this || item.Log != m_currentLog)
                    continue;

                var parser = item.Parser;
                if (parser == null)
                {
                    turnOffTimes = 0;
                    protocol.CreateParser(item);
                    parser = item.Parser;
                }

                if (!parser.IsParsed)
                {
                    turnOffTimes = 0;
                    parser.Parse();
                }

                Thread.Sleep(1);
            }
        }

        internal void StopParsing()
        {
            if (m_parsingWorker.IsBusy)
                m_parsingWorker.CancelAsync();

            lock (m_parsingQueue)
                m_parsingQueue.Clear();
        }

        internal void StartParsing()
        {
            if (!m_parsingWorker.IsBusy)
            {
                lock (m_parsingQueue)
                {
                    if (m_parsingQueue.Count > 0)
                        m_parsingWorker.RunWorkerAsync();
                }
            }
        }

        public override void EnqueueParsing(ViewerItem item)
        {
            lock (m_parsingQueue)
                m_parsingQueue.Enqueue(item);

            this.StartParsing();
        }
        #endregion
    }
}
