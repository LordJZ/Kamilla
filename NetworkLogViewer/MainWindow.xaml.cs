using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Kamilla;
using Kamilla.Network;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.WPF;
using Microsoft.Win32;

namespace NetworkLogViewer
{
    partial class MainWindow : Window, INetworkLogViewer
    {
        WindowInteropHelper m_interopHelper;

        BackgroundWorker ui_parsingWorker;
        BackgroundWorker ui_loadingWorker;
        BackgroundWorker ui_readingWorker;

        PacketAddedEventHandler m_packetAddedHandler;

        #region .ctor
        public MainWindow()
        {
            InitializeComponent();

            App.InitializeConsole();
            InitializeSkins();

            // Command Bindings
            this.CommandBindings.AddRange(new[] {
                new CommandBinding(ApplicationCommands.Close, ApplicationClose_Executed),
                new CommandBinding(ApplicationCommands.Open, ApplicationOpen_Executed),
                new CommandBinding(NetworkLogViewerCommands.OpenConsole, OpenConsole_Executed),
            });

            // Key Bindings
            this.InputBindings.AddRange(new[] {
                new KeyBinding(ApplicationCommands.Close, Key.X, ModifierKeys.Alt),
                new KeyBinding(ApplicationCommands.Open, Key.O, ModifierKeys.Control),
                new KeyBinding(NetworkLogViewerCommands.OpenConsole, Key.F10, ModifierKeys.None),
            });

            // Background Workers
            this.ui_parsingWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            //this.ui_parsingWorker.DoWork += new DoWorkEventHandler(this.ui_parsingWorker_DoWork);

            this.ui_loadingWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            this.ui_loadingWorker.DoWork += new DoWorkEventHandler(this.ui_loadingWorker_DoWork);
            this.ui_loadingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ui_loadingWorker_RunWorkerCompleted);

            this.ui_readingWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            this.ui_readingWorker.DoWork += new DoWorkEventHandler(this.ui_readingWorker_DoWork);
            this.ui_readingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ui_readingWorker_RunWorkerCompleted);

            this.ProtocolChanged += new ProtocolChangedEventHandler(MainWindow_ProtocolChanged);

            m_items = new ViewerItemCollection();
            m_items.ItemQueried += new ViewerItemEventHandler(m_items_ItemQueried);

            m_interopHelper = new WindowInteropHelper(this);

            ui_lvPackets.ItemsSource = m_items;

            m_packetAddedHandler = new PacketAddedEventHandler(m_currentLog_PacketAdded);

            Console.WriteLine("MainWindow initialized.");
        }

        void m_items_ItemQueried(object sender, ViewerItemEventArgs e)
        {
            this.ThreadSafe(_ =>
            {
                if (this.ItemQueried != null)
                    this.ItemQueried(this, e);
            });
        }
        #endregion

        #region INetworkLogViewer Implementation
        Protocol m_currentProtocol;
        NetworkLog m_currentLog;
        int m_packetItr;

        /// <summary>
        /// Gets the collection of items currently loaded.
        /// </summary>
        public IEnumerable<ViewerItem> Items { get { return m_items; } }

        /// <summary>
        /// Gets or sets the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public Protocol CurrentProtocol
        {
            get { return m_currentProtocol; }
            set
            {
                if (m_currentProtocol == value)
                    return;

                this.ThreadSafe(_ =>
                {
                    var old = m_currentProtocol;
                    if (old != null)
                        old.Unload();

                    m_currentProtocol = value;
                    m_currentProtocol.Load(this);

                    if (ProtocolChanged != null)
                        ProtocolChanged(this, new ProtocolChangedEventArgs(old, value));
                });
            }
        }

        /// <summary>
        /// Gets the currently loaded <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public NetworkLog CurrentLog
        {
            get { return m_currentLog; }
            set
            {
                if (m_currentLog == value)
                    return;

                this.ThreadSafe(_ =>
                {
                    var old = m_currentLog;
                    if (old != null)
                        old.PacketAdded -= m_packetAddedHandler;

                    m_currentLog = value;
                    m_currentLog.PacketAdded += m_packetAddedHandler;

                    if (this.NetworkLogChanged != null)
                        this.NetworkLogChanged(this, new NetworkLogChangedEventArgs(old, value));
                });
            }
        }

        /// <summary>
        /// Gets the handle of the window.
        /// </summary>
        public IntPtr WindowHandle
        {
            get { return m_interopHelper.Handle; }
        }

        /// <summary>
        /// Occurs when <see href="NetworkLogViewer.MainWindow.CurrentProtocol"/> changes.
        /// </summary>
        public event ProtocolChangedEventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see href="NetworkLogViewer.MainWindow.CurrentLog"/> property changes.
        /// </summary>
        public event NetworkLogChangedEventHandler NetworkLogChanged;

        /// <summary>
        /// Occurs when data of a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is queried.
        /// </summary>
        public event ViewerItemEventHandler ItemQueried;

        /// <summary>
        /// Occurs when a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is added.
        /// </summary>
        public event ViewerItemEventHandler ItemAdded;
        #endregion

        readonly ViewerItemCollection m_items;

        #region Loading Window
        Stack<LoadingState> m_loadingStateStack = new Stack<LoadingState>();
        LoadingWindow m_loadingWindow;

        void LoadingStatePush(LoadingState state)
        {
            this.ThreadSafe(safeThis =>
            {
                m_loadingStateStack.Push(state);

                if (m_loadingWindow == null)
                    m_loadingWindow = new LoadingWindow();

                m_loadingWindow.SetLoadingState(state);

                m_loadingWindow.Owner = safeThis;
                if (!m_loadingWindow.IsVisible)
                    m_loadingWindow.ShowDialog();
            });
        }

        void LoadingStateSetProgress(int percent)
        {
            this.ThreadSafe(safeThis =>
            {
                m_loadingWindow.SetProgress(percent);
            });
        }

        void LoadingStatePop()
        {
            this.ThreadSafe(safeThis =>
            {
                if (m_loadingStateStack.Count == 0)
                    throw new InvalidOperationException("Loading State stack is empty.");

                m_loadingStateStack.Pop();

                if (m_loadingStateStack.Count != 0)
                    m_loadingWindow.SetLoadingState(m_loadingStateStack.Peek());
                else
                    m_loadingWindow.Hide();
            });
        }
        #endregion

        #region Commands
        void ApplicationClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        OpenFileDialog m_openFileDialog;
        void ApplicationOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_openFileDialog == null)
            {
                m_openFileDialog = new OpenFileDialog();
                m_openFileDialog.AddExtension = false;
                m_openFileDialog.Filter = NetworkLogFactory.AllFileFiltersWithAny;
                m_openFileDialog.FilterIndex = NetworkLogFactory.AllFileFiltersWithAnyCount;
                m_openFileDialog.CheckFileExists = true;
                var file = Configuration.GetValue("Open File Name", string.Empty);
                m_openFileDialog.FileName = Path.GetFileName(file);
                m_openFileDialog.InitialDirectory = Path.GetDirectoryName(file);
                m_openFileDialog.Multiselect = false;
            }

            var result = m_openFileDialog.ShowDialog(this);
            if (true == result)
            {
                var filename = m_openFileDialog.FileName;
                Configuration.SetValue("Open File Name", filename);
                OpenFile(filename);
            }
        }

        void OpenConsole_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var console = App.ConsoleWindow;

            if (!console.IsVisible)
                console.Show();

            if (!console.IsFocused)
                console.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // TODO: Save Settings Here

            App.ConsoleWindow.m_closing = true;
            App.ConsoleWindow.Close();
        }
        #endregion

        #region Reading
        // Code that reads the dump file.

        void CloseFile()
        {
            //if (ui_lvPackets.Items.Count > 0)
            //    ui_lvPackets.ScrollIntoView(ui_lvPackets.Items[0]);

            //if (ui_parsingWorker.IsBusy)
            //    ui_parsingWorker.CancelAsync();

            ///* TODO
            //foreach (var tc in m_currentViews)
            //{
            //    foreach (PacketViewTabs.PacketViewTabPage tab in tc.Controls)
            //        tab.Reset();
            //}*/
            //ui_lvPackets.Items.Clear();

            //lock (m_itemsSyncRoot)
            //{
            //    m_items = new PacketViewerItem[0];
            //    if (ItemListUpdate != null)
            //        ItemListUpdate(this, new ItemListUpdateEventArgs(m_items));

            //    m_filterMap = new int[0];
            //    if (FilterMapUpdate != null)
            //        FilterMapUpdate(this, new FilterMapUpdateEventArgs(m_filterMap));

            //    // TODO
            //    //ui_lvPackets.VirtualListSize = 0;
            //}

            //this.Title = "Packet Viewer";
        }

        string m_currentFile;
        void OpenFile(string filename)
        {
            var log = NetworkLogFactory.GetNetworkLog(filename);
            if (log == null)
                throw new NotImplementedException("Select Network Log window is not implemented");

            this.OpenFile(filename, log);
        }

        void OpenFile(string filename, NetworkLog log)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            if (log == null)
                throw new ArgumentNullException("log");

            m_currentFile = filename;
            this.CurrentLog = log;

            ui_readingWorker.RunWorkerAsync();
            this.LoadingStatePush(new LoadingState(string.Format(Strings.LoadingFile, filename)));
        }

        void m_currentLog_PacketAdded(object sender, PacketAddedEventArgs e)
        {
            this.ThreadSafe(_ =>
            {
                var item = new ViewerItem(this, (NetworkLog)sender, m_packetItr++, e.Packet);
                m_items.Add(item);

                if (this.ItemAdded != null)
                    this.ItemAdded(this, new ViewerItemEventArgs(item));
            });
        }

        private void ui_readingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = Stopwatch.StartNew();

            this.CurrentLog.OpenForReading(m_currentFile);

            var suggestedProtocol = this.CurrentLog.SuggestedProtocol;
            if (suggestedProtocol != null)
                this.CurrentProtocol = suggestedProtocol.Activate();

            this.CurrentLog.Read(progress => LoadingStateSetProgress(progress));
        }

        private void ui_readingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadingStatePop();
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ui_loadingWorker.RunWorkerAsync();
            this.LoadingStatePush(new LoadingState("Loading..."));
        }

        private void ui_loadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TypeManager.Initialize();
            ProtocolManager.Initialize();
            InitializeProtocols();
            NetworkLogFactory.Initialize();
        }

        private void ui_loadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.LoadingStatePop();
        }

        #region Protocol
        void InitializeProtocols()
        {
            ui_miProtocol.ThreadSafeBegin(mi =>
            {
                mi.BeginInit();

                foreach (var wrapper in ProtocolManager.ProtocolWrappers)
                {
                    var item = new MenuItem();
                    item.Header = wrapper.Name;
                    item.Tag = wrapper;
                    item.Click += new RoutedEventHandler(protocolItem_Click);
                    if (this.CurrentProtocol == null)
                    {
                        this.CurrentProtocol = wrapper.Activate();
                        item.IsChecked = true;
                    }
                    mi.Items.Add(item);
                }

                mi.EndInit();
            });
        }

        void protocolItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var wrapper = (ProtocolWrapper)item.Tag;

            var protocol = wrapper.Activate();

            this.CurrentProtocol = protocol;
        }

        void MainWindow_ProtocolChanged(object sender, ProtocolChangedEventArgs e)
        {
            var newProtocol = e.NewProtocol;
            Type newProtocolType = null;
            if (newProtocol != null)
                newProtocolType = newProtocol.GetType();

            foreach (MenuItem itrItem in ui_miProtocol.Items)
                itrItem.IsChecked = itrItem.Tag == null ? newProtocol == null :
                    newProtocolType == ((ProtocolWrapper)itrItem.Tag).Type;

            if (newProtocol != null)
            {
                var typename = newProtocolType.Name;
                int nColumns = newProtocol.ListViewColumns;
                if (nColumns < 0)
                    throw new InvalidOperationException("Protocol.ListViewColumns is invalid.");

                var headers = newProtocol.ListViewColumnHeaders;
                if (headers == null || headers.Length != nColumns || headers.Any(val => val == null))
                    throw new InvalidOperationException("Protocol.ListViewColumnHeaders is invalid.");

                int[] widths = Configuration.GetValue("Column Widths - " + typename, (int[])null);
                if (widths == null || widths.Length != nColumns)
                {
                    widths = newProtocol.ListViewColumnWidths;

                    if (widths == null || widths.Length != nColumns)
                        throw new InvalidOperationException("Protocol.ListViewColumnWidths is invalid.");
                }

                int[] columnOrder = Configuration.GetValue("Column Order - " + typename, (int[])null);
                if (columnOrder == null || columnOrder.Length != nColumns
                    || columnOrder.Any(val => val >= nColumns || val < 0))
                    columnOrder = Enumerable.Range(0, nColumns).ToArray();

                // Everything is valid
                var view = new GridView();

                for (int i = 0; i < nColumns; i++)
                {
                    int col = columnOrder[i];

                    var item = new GridViewColumn();
                    item.Header = headers[col];
                    item.Width = widths[col];
                    item.DisplayMemberBinding = new Binding(".Data[" + i + "]");
                    view.Columns.Add(item);
                }

                ui_lvPackets.View = view;
            }
            else
            {
                ui_lvPackets.View = null;
            }
        }
        #endregion

        #region Skins
        Dictionary<string, Style> m_skins;

        void InitializeSkins()
        {
            this.ThreadSafe(w =>
            {
                m_skins = new Dictionary<string, Style>()
                {
                    { "KamillaStyle", (Style)this.FindResource("KamillaStyle") },
                    { "Windows", null },
                };

                var resources = Strings.ResourceManager;
                var culture = CultureInfo.CurrentUICulture;

                const string defaultSkin = "KamillaStyle";

                int i = 0;
                foreach (var skin in m_skins)
                {
                    var item = new MenuItem();
                    item.Header = resources.GetString("Skin_" + skin.Key, culture);
                    item.Tag = skin.Key;
                    item.Click += new RoutedEventHandler(skinItem_Click);

                    if (skin.Key == defaultSkin)
                        item.IsChecked = true;

                    ui_miSkins.Items.Add(item);

                    ++i;
                }

                var usedSkin = Configuration.GetValue("Skin", defaultSkin);
                try
                {
                    SetSkin(usedSkin);
                }
                catch
                {
                    SetSkin(defaultSkin);
                }
            });
        }

        void skinItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var skinName = (string)item.Tag;

            SetSkin(skinName);
        }

        void SetSkin(string name)
        {
            Style style;
            if (!m_skins.TryGetValue(name, out style))
                throw new ArgumentException("Skin '" + name + "' not found.");

            if (style == this.Style)
                return;

            this.Style = style;
            Configuration.SetValue("Skin", name);

            foreach (MenuItem item in ui_miSkins.Items)
                item.IsChecked = name == (string)item.Tag;
        }
        #endregion

        void ClearAll()
        {
            m_items.Clear();
            ui_lvPackets.View = null;
            this.CurrentProtocol = null;
            this.CurrentLog = null;
            m_packetItr = 0;
        }
    }
}
