using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Kamilla;
using Kamilla.Network.Logging;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.WPF;
using System.Windows.Input;
using Kamilla.Network.Viewing.Plugins;
using System.Windows.Controls;

namespace NetworkLogViewer
{
    internal sealed class ViewerImplementation : NetworkLogViewerBase
    {
        #region Dealloc Queue
        class DeallocQueue
        {
            ViewerItem[] m_items;
            int m_index;
            int m_capacity;

            internal DeallocQueue(int capacity)
            {
                if (capacity <= 0)
                    throw new ArgumentOutOfRangeException("capacity");

                m_items = new ViewerItem[capacity];
                m_capacity = capacity;
            }

            internal void Push(ViewerItem item)
            {
                ++m_index;
                m_index %= m_capacity;

                var old = m_items[m_index];
                if (old != null)
                {
                    old.Parser = null;
                    old.Data = null;
                }

                m_items[m_index] = item;
            }

            internal void Clear()
            {
                Array.Clear(m_items, 0, m_capacity);
                m_index = 0;
            }
        }
        #endregion

        bool m_autoParse;
        bool m_deallocQueueEnabled;

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
                    m_deallocQueue.Clear();
            }
        }

        MainWindow m_window;
        PacketAddedEventHandler m_packetAddedHandler;
        Protocol m_currentProtocol;
        NetworkLog m_currentLog;
        internal readonly ViewerItemCollection m_items;
        WindowInteropHelper m_interopHelper;
        BackgroundWorker m_parsingWorker;
        DeallocQueue m_deallocQueue;
        INetworkLogViewerPlugin[] m_plugins;
        List<PluginCommand> m_pluginCommands;

        internal ViewerImplementation(MainWindow window)
        {
            m_window = window;
            m_interopHelper = new WindowInteropHelper(window);

            m_items = new ViewerItemCollection(this);
            m_items.ItemQueried += new ViewerItemEventHandler(m_items_ItemQueried);
            m_packetAddedHandler = new PacketAddedEventHandler(m_currentLog_PacketAdded);

            m_deallocQueue = new DeallocQueue(100);

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
            foreach (var item in m_items)
            {
                item.Parser = null;
                item.Data = null;
            }

            m_items.Update();
        }

        internal void SetProtocol(Protocol value)
        {
            if (m_currentProtocol == value)
                return;

            m_parsingWorker.CancelAsync();

            var old = m_currentProtocol;

            // We should allow the protocol to integrate with viewer in viewer's thread.
            m_window.ThreadSafe(_ =>
            {
                if (old != null)
                    old.Unload();

                m_currentProtocol = value;

                if (value != null)
                    value.Load(this);
            });

            if (this.ProtocolChanged != null)
                this.ProtocolChanged(this, new ProtocolChangedEventArgs(old, value));

            this.DropCache();
        }

        internal void SetLog(NetworkLog value)
        {
            if (m_currentLog == value)
                return;

            m_parsingWorker.CancelAsync();

            var old = m_currentLog;
            if (old != null)
                old.PacketAdded -= m_packetAddedHandler;

            m_currentLog = value;
            if (value != null)
                value.PacketAdded += m_packetAddedHandler;

            if (this.NetworkLogChanged != null)
                this.NetworkLogChanged(this, new NetworkLogChangedEventArgs(old, value));
        }

        protected override void OnParsingDone(ViewerItem item)
        {
            base.OnParsingDone(item);

            if (m_deallocQueueEnabled)
                m_deallocQueue.Push(item);
        }

        internal void OnStyleChanged(Style oldStyle, Style newStyle)
        {
            m_window.ThreadSafe(_ =>
            {
            });
        }

        internal void CloseFile()
        {
            m_items.Clear();
            this.SetLog(null);
            m_parsingWorker.CancelAsync();
        }

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

            Console.WriteLine("Debug: RegisterPluginCommand '{0}' ({1}, {2})",
                command.Title, command.Gesture.DisplayString, command.Gesture);

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
            });
        }

        #region Overrides
        /// <summary>
        /// Retrieves the object that is responsible for the user interface.
        /// </summary>
        public override object InterfaceObject { get { return m_window; } }

        /// <summary>
        /// Gets the collection of items currently loaded.
        /// </summary>
        public override IEnumerable<ViewerItem> Items { get { return m_items; } }

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
        public override event ProtocolChangedEventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see cref="NetworkLogViewer.MainWindow.CurrentLog"/> property changes.
        /// 
        /// Handlers of this event are called from any suiting thread.
        /// </summary>
        public override event NetworkLogChangedEventHandler NetworkLogChanged;

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
        ConcurrentQueue<ViewerItem> m_parsingQueue = new ConcurrentQueue<ViewerItem>();

        void m_parsingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // THREADING DANGER ZONE!

            var worker = (BackgroundWorker)sender;
            int turnOffTimes = 0;

            do
            {
                while (!worker.CancellationPending && !m_parsingQueue.IsEmpty && m_currentProtocol != null)
                {
                    // Cache protocol
                    var protocol = m_currentProtocol;

                    ViewerItem item;
                    if (!m_parsingQueue.TryDequeue(out item))
                        break;

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

                while (!worker.CancellationPending && (m_parsingQueue.IsEmpty || m_currentProtocol == null))
                {
                    Thread.Sleep(100);
                    if (++turnOffTimes == 50)
                        return;
                }
            }
            while (!worker.CancellationPending);
        }

        public override void EnqueueParsing(ViewerItem item)
        {
            m_parsingQueue.Enqueue(item);
            if (!m_parsingWorker.IsBusy)
                m_parsingWorker.RunWorkerAsync();
        }
        #endregion
    }
}
