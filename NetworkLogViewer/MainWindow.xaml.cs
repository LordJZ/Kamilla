using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Kamilla;
using Kamilla.Network.Logging;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.WPF;
using Microsoft.Win32;
using NetworkLogViewer.ViewTabs;

namespace NetworkLogViewer
{
    partial class MainWindow : Window
    {
        BackgroundWorker ui_loadingWorker;
        BackgroundWorker ui_readingWorker;

        ViewerImplementation m_implementation;
        internal ViewerImplementation Implementation { get { return m_implementation; } }

        #region .ctor
        public MainWindow()
        {
            UICulture.Initialize();
            UICulture.UICultureChanged += new EventHandler(UICulture_UICultureChanged);

            InitializeComponent();

            m_implementation = new ViewerImplementation(this);

            App.InitializeConsole(this);

            // Perform operations that alter UI here
            {
                // Use Minimized as special value
                var stateNotSet = WindowState.Minimized;
                var state = Configuration.GetValue("Window State", stateNotSet);

                if (state != WindowState.Maximized)
                {
                    var screenHeight = SystemParameters.PrimaryScreenHeight;
                    var screenWidth = SystemParameters.PrimaryScreenWidth;
                    var height = Configuration.GetValue("Window Height", this.Height);
                    var width = Configuration.GetValue("Window Width", this.Width);

                    if (width / screenWidth > 0.8)
                        width = screenWidth * 0.8;

                    if (height / screenHeight > 0.8)
                        height = screenHeight * 0.8;

                    this.Width = width;
                    this.Height = height;

                    var left = Math.Max(Configuration.GetValue("Window Left", this.Left), 0.0);
                    var top = Math.Max(Configuration.GetValue("Window Top", this.Top), 0.0);

                    if (left != 0.0 && top != 0.0)
                    {
                        if (left + width > screenWidth)
                            left = screenWidth - width;

                        if (top + height > screenHeight)
                            top = screenHeight - top;

                        this.Left = left;
                        this.Top = top;
                    }
                }

                if (state != stateNotSet)
                    this.WindowState = state;

                int val = Configuration.GetValue("Number of Views", 2);
                this.SetNViews(val);

                var result = Configuration.GetValue("Vertical Splitter", (double[])null);
                if (result != null && result.Length == 2)
                {
                    this.VerticalGrid.RowDefinitions[1].Height = new GridLength(result[0], GridUnitType.Star);
                    this.VerticalGrid.RowDefinitions[2].Height = new GridLength(result[1], GridUnitType.Star);
                }

                InitializeSkins();
            }

            // Command Bindings
            this.CommandBindings.AddRange(new[] {
                new CommandBinding(ApplicationCommands.Close, ApplicationClose_Executed),
                new CommandBinding(ApplicationCommands.Open, ApplicationOpen_Executed),
                new CommandBinding(NetworkLogViewerCommands.OpenConsole, OpenConsole_Executed),
                new CommandBinding(NetworkLogViewerCommands.CloseFile, CloseFile_Executed),
            });

            // Key Bindings
            this.InputBindings.AddRange(new[] {
                new KeyBinding(ApplicationCommands.Close, Key.X, ModifierKeys.Alt),
                new KeyBinding(ApplicationCommands.Open, (KeyGesture)ApplicationCommands.Open.InputGestures[0]),
                new KeyBinding(NetworkLogViewerCommands.OpenConsole, Key.F10, ModifierKeys.None),
            });

            // Background Workers
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

            m_implementation.ProtocolChanged += new ProtocolChangedEventHandler(MainWindow_ProtocolChanged);
            m_implementation.NetworkLogChanged += new NetworkLogChangedEventHandler(MainWindow_NetworkLogChanged);

            ui_lvPackets.ItemsSource = m_implementation.m_items;

            Console.WriteLine("MainWindow initialized.");
        }
        #endregion

        #region Implementation Interop
        protected override void OnStyleChanged(Style oldStyle, Style newStyle)
        {
            base.OnStyleChanged(oldStyle, newStyle);

            m_implementation.OnStyleChanged(oldStyle, newStyle);
        }

        internal Protocol CurrentProtocol
        {
            get { return m_implementation.CurrentProtocol; }
            set { m_implementation.SetProtocol(value); }
        }

        internal NetworkLog CurrentLog
        {
            get { return m_implementation.CurrentLog; }
            set { m_implementation.SetLog(value); }
        }
        #endregion

        #region Languages
        static CultureInfo[] s_supportedCultures = new[]
        {
            CultureInfo.GetCultureInfo("en"),
            CultureInfo.GetCultureInfo("ru"),
        };

        void InitializeLanguages()
        {
            this.ThreadSafe(_ =>
            {
                foreach (var culture in s_supportedCultures)
                {
                    var item = new MenuItem();

                    item.Header = culture.EnglishName;
                    item.Tag = culture;
                    item.Click += new RoutedEventHandler(LanguageItem_Click);

                    ui_miLanguage.Items.Add(item);
                }

                SetLanguageItemForCulture(UICulture.Culture);
            });
        }

        void LanguageItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var culture = (CultureInfo)item.Tag;

            UICulture.Culture = culture;
            // Items modified in event handler
        }

        void UICulture_UICultureChanged(object sender, EventArgs e)
        {
            SetLanguageItemForCulture(UICulture.Culture);
        }

        void SetLanguageItemForCulture(CultureInfo culture)
        {
            var code = culture.Name.Substring(0, 2);
            foreach (MenuItem item in ui_miLanguage.Items)
                item.IsChecked = ((CultureInfo)item.Tag).Name.SubstringEquals(0, code);
        }
        #endregion

        #region Loading Window
        Stack<LoadingState> m_loadingStateStack = new Stack<LoadingState>();
        LoadingWindow m_loadingWindow;

        void LoadingStatePush(LoadingState state)
        {
            this.ThreadSafeBegin(safeThis =>
            {
                m_loadingStateStack.Push(state);

                if (m_loadingWindow == null)
                {
                    m_loadingWindow = new LoadingWindow();
                    m_loadingWindow.Style = this.Style;
                    m_implementation.StyleChanged +=
                        (o, e) => this.ThreadSafe(_ => _.m_loadingWindow.Style = _.Style);
                }

                m_loadingWindow.SetLoadingState(state);

                m_loadingWindow.Owner = safeThis;
                if (!m_loadingWindow.IsVisible)
                    m_loadingWindow.ShowDialog();
            });
        }

        void LoadingStateSetProgress(int percent)
        {
            this.ThreadSafeBegin(safeThis =>
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
                try
                {
                    var file = Configuration.GetValue("Open File Name", string.Empty);
                    m_openFileDialog.FileName = Path.GetFileName(file);
                    m_openFileDialog.InitialDirectory = Path.GetDirectoryName(file);
                }
                catch
                {
                }
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

        void CloseFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.CloseFile();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ui_readingWorker.CancelAsync();
            m_implementation.CloseFile();

            var protocol = this.CurrentProtocol;

            Configuration.SuspendSaving();
            if (protocol != null)
                this.SaveProtocolColumnSettings(protocol);
            Configuration.SetValue("Number of Views", m_currentNViews);
            this.SaveCurrentViews();
            Configuration.SetValue("Vertical Splitter", new[] {
                this.VerticalGrid.RowDefinitions[1].Height.Value,
                this.VerticalGrid.RowDefinitions[2].Height.Value,
            });
            Configuration.SetValue("Window State", this.WindowState);
            Configuration.SetValue("Window Height", this.Height);
            Configuration.SetValue("Window Width", this.Width);
            Configuration.SetValue("Window Left", this.Left);
            Configuration.SetValue("Window Top", this.Top);
            Configuration.ResumeSaving();

            App.ConsoleWindow.m_closing = true;
            App.ConsoleWindow.Close();
        }

        void CloseFile()
        {
            m_implementation.CloseFile();
        }
        #endregion

        #region Reading
        // Code that reads the dump file.

        string m_currentFile;
        void OpenFile(string filename)
        {
            NetworkLog log;
            try
            {
                log = NetworkLogFactory.GetNetworkLog(filename);
            }
            catch
            {
                MessageWindow.Show(this, "FAIL!", "FAIL!");
                return;
            }
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

            this.CloseFile();

            m_currentFile = filename;
            this.CurrentLog = log;

            ui_readingWorker.RunWorkerAsync();
            this.LoadingStatePush(new LoadingState(string.Format(Strings.LoadingFile, filename)));
        }

        private void ui_readingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UICulture.Initialize();

            var sw = Stopwatch.StartNew();

            this.CurrentLog.OpenForReading(m_currentFile);

            if (this.CurrentLog.Capacity > m_implementation.m_items.Capacity)
                m_implementation.m_items.Capacity = this.CurrentLog.Capacity;

            m_implementation.m_items.SuspendUpdating();
            this.CurrentLog.Read(progress =>
            {
                LoadingStateSetProgress(progress);
            });

            e.Result = this.CurrentLog.SuggestedProtocol ?? ProtocolManager.FindWrapper(typeof(DefaultProtocol));

            sw.Stop();
            Console.WriteLine("Reading Worker finished in {0}", sw.Elapsed);
        }

        private void ui_readingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.CurrentLog.CloseStream();

            var wrapper = e.Result as ProtocolWrapper;
            if (wrapper != null)
            {
                this.CurrentProtocol = wrapper.Activate();
            }

            var sw = Stopwatch.StartNew();
            m_implementation.m_items.ResumeUpdating();
            m_implementation.m_items.Update();
            sw.Stop();
            Console.WriteLine("Updated items in {0}", sw.Elapsed);
            LoadingStatePop();

            if (e.Error != null)
            {
                MessageWindow.Show(this, Strings.Error, Strings.ErrorReading.LocalizedFormat(e.Error.ToString()));
            }
        }

        void MainWindow_NetworkLogChanged(object sender, NetworkLogChangedEventArgs e)
        {
            var newLog = e.NewLog;
            ui_sbiNetworkLog.Content = newLog != null ? newLog.Name : Strings.NoNetworkLog;
        }
        #endregion

        #region Loading
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ui_loadingWorker.RunWorkerAsync();
            this.LoadingStatePush(new LoadingState("Loading..."));
        }

        private void ui_loadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UICulture.Initialize();

            TypeManager.Initialize();
            ProtocolManager.Initialize();
            NetworkLogFactory.Initialize();
            InitializeProtocols();
            InitializeLanguages();
        }

        private void ui_loadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.LoadingStatePop();
        }
        #endregion

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

        class MyGridViewColumn : GridViewColumn
        {
            public int ColumnId;
        }

        void SaveProtocolColumnSettings(Protocol protocol)
        {
            var view = (GridView)ui_lvPackets.View;
            var columns = view.Columns;
            var nColumns = columns.Count;
            var typename = protocol.GetType().Name;

            var widths = new double[nColumns];
            var order = new int[nColumns];

            for (int i = 0; i < nColumns; i++)
            {
                var column = (MyGridViewColumn)columns[i];

                order[i] = column.ColumnId;
                widths[column.ColumnId] = column.Width;
            }

            Configuration.SetValue(typename + " Column Widths", widths);
            Configuration.SetValue(typename + " Column Order", order);
        }

        void MainWindow_ProtocolChanged(object sender, ProtocolChangedEventArgs e)
        {
            var newProtocol = e.NewProtocol;
            Type newProtocolType = null;
            if (newProtocol != null)
                newProtocolType = newProtocol.GetType();

            if (e.OldProtocol != null)
                SaveProtocolColumnSettings(e.OldProtocol);

            foreach (MenuItem itrItem in ui_miProtocol.Items)
                itrItem.IsChecked = itrItem.Tag == null ? newProtocol == null :
                    newProtocolType == ((ProtocolWrapper)itrItem.Tag).Type;

            ui_sbiProtocol.Content = newProtocol != null ? newProtocol.Name : Strings.NoProtocol;

            if (newProtocol != null)
            {
                var typename = newProtocolType.Name;
                int nColumns = newProtocol.ListViewColumns;
                if (nColumns < 0)
                    throw new InvalidOperationException("Protocol.ListViewColumns is invalid.");

                var headers = newProtocol.ListViewColumnHeaders;
                if (headers == null || headers.Length != nColumns || headers.Any(val => val == null))
                    throw new InvalidOperationException("Protocol.ListViewColumnHeaders is invalid.");

                double[] widths = Configuration.GetValue(typename + " Column Widths", (double[])null);
                if (widths == null || widths.Length != nColumns)
                {
                    widths = newProtocol.ListViewColumnWidths;

                    if (widths == null || widths.Length != nColumns)
                        throw new InvalidOperationException("Protocol.ListViewColumnWidths is invalid.");
                }

                int[] columnOrder = Configuration.GetValue(typename + " Column Order", (int[])null);
                if (columnOrder == null || columnOrder.Length != nColumns
                    || columnOrder.Any(val => val >= nColumns || val < 0))
                    columnOrder = Enumerable.Range(0, nColumns).ToArray();

                // Everything is valid
                var view = new GridView();

                for (int i = 0; i < nColumns; i++)
                {
                    int col = columnOrder[i];

                    var item = new MyGridViewColumn();
                    item.ColumnId = col;
                    item.Header = headers[col];
                    item.Width = widths[col];
                    item.DisplayMemberBinding = new Binding(".Data[" + col + "]");
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
            m_skins = new Dictionary<string, Style>()
            {
                { "KamillaStyle", (Style)this.FindResource("KamillaStyle") },
                { "Windows", null },
            };

            var resources = Strings.ResourceManager;
            var culture = CultureInfo.CurrentUICulture;

            int i = 0;
            foreach (var skin in m_skins)
            {
                var item = new MenuItem();
                item.Header = resources.GetString("Skin_" + skin.Key, culture);
                item.Tag = skin.Key;
                item.Click += new RoutedEventHandler(skinItem_Click);

                if (skin.Key == "Windows")
                    item.IsChecked = true;

                ui_miSkins.Items.Add(item);

                ++i;
            }

            var defaultStyle = "KamillaStyle";
            var usedSkin = Configuration.GetValue("Skin", defaultStyle);
            try
            {
                SetSkin(usedSkin);
            }
            catch
            {
                SetSkin(defaultStyle);
            }
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

        #region Viewing
        static Type[] s_viewTabTypes = new[]
        {
            typeof(PacketContents),
            typeof(ParsedText),
            //typeof(ImageContents),
        };

        int m_currentNViews;
        GridSplitter[] m_splitters = new GridSplitter[0];
        TabControl[] m_currentViews = new TabControl[0];

        private void ui_miViewsCount_Click(object sender, EventArgs e)
        {
            var str = ((MenuItem)sender).Tag.ToString();
            int nViews = int.Parse(str);

            if (nViews != m_currentNViews)
                this.SetNViews(nViews);
        }

        void SetNViews(int nViews)
        {
            if (m_currentNViews != 0)
                this.SaveCurrentViews();

            m_currentNViews = nViews;

            //Win32.SuspendDrawing(this.WindowHandle);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            ViewsGrid.ColumnDefinitions.Clear();
            ViewsGrid.Children.Clear();

            m_splitters = new GridSplitter[nViews - 1];
            m_currentViews = new TabControl[nViews];

            var distances = Configuration.GetValue("View" + nViews + " Distances Pct",
                Enumerable.Repeat(1.0 / nViews, nViews).ToArray());

            Array.Resize(ref distances, nViews);

            for (int i = 0; i < nViews; ++i)
            {
                var column = new ColumnDefinition();
                column.Width = new GridLength(distances[i], GridUnitType.Star);
                column.MinWidth = 50.0;
                ViewsGrid.ColumnDefinitions.Add(column);
            }

            for (int i = 0; i < m_splitters.Length; ++i)
            {
                var splitter = new GridSplitter();
                Panel.SetZIndex(splitter, 100);
                splitter.Background = Brushes.Transparent;
                splitter.Width = 6;
                splitter.HorizontalAlignment = HorizontalAlignment.Left;
                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                splitter.Margin = new Thickness(-3.0, 0.0, 0.0, 0.0);
                ViewsGrid.Children.Add(splitter);
                Grid.SetColumn(splitter, i + 1);
            }

            var selectedTabs = Configuration.GetValue("View" + nViews + " Selected Tabs",
                Enumerable.Range(0, nViews).ToArray());

            Array.Resize(ref selectedTabs, nViews);
            for (int i = 0; i < selectedTabs.Length; ++i)
            {
                if (selectedTabs[i] >= s_viewTabTypes.Length)
                    selectedTabs[i] %= s_viewTabTypes.Length;
            }

            for (int i = 0; i < nViews; ++i)
            {
                var tc = new TabControl();
                ViewsGrid.Children.Add(tc);
                Grid.SetColumn(tc, i);
                m_currentViews[i] = tc;

                // Initialize Tab Control

                foreach (var type in s_viewTabTypes)
                {
                    var content = (IViewTab)Activator.CreateInstance(type);

                    var tabItem = new TabItem();
                    tabItem.Content = content;
                    tabItem.Header = content.Header;
                    tc.Items.Add(tabItem);
                }

                tc.SelectedItem = tc.Items[selectedTabs[i]];
                tc.SelectionChanged += new SelectionChangedEventHandler(tc_Selected);
            }

            var nViewsStr = nViews.ToString();
            foreach (MenuItem item in ui_miViewsColumns.Items)
                item.IsChecked = item.Tag.ToString() == nViewsStr;

            //Win32.ResumeDrawing(this.WindowHandle);

            sw.Stop();
            Console.WriteLine("Debug: Spent {0} on building {1} views", sw.Elapsed, m_currentNViews);

            UpdateViews();
        }

        void tc_Selected(object sender, SelectionChangedEventArgs e)
        {
            int index = ui_lvPackets.SelectedIndex;

            if (index >= 0)
            {
                var tab = (IViewTab)((TabItem)((TabControl)sender).SelectedItem).Content;
                if (!tab.IsFilled)
                    tab.Fill(this.CurrentProtocol, m_implementation.m_items[index]);
            }
        }

        void UpdateViews()
        {
            var protocol = this.CurrentProtocol;
            int index = ui_lvPackets.SelectedIndex;
            ViewerItem item = null;
            if (index >= 0)
                item = m_implementation.m_items[index];

            foreach (var tc in m_currentViews)
            {
                foreach (TabItem tab in tc.Items)
                    ((IViewTab)tab.Content).Reset();

                if (index >= 0)
                    ((IViewTab)((TabItem)tc.SelectedItem).Content).Fill(protocol, item);
            }
        }

        void SaveCurrentViews()
        {
            var nViews = m_currentNViews;
            var distances = new double[nViews];
            var selectedTabs = new int[nViews];

            for (int i = 0; i < nViews; i++)
                distances[i] = this.ViewsGrid.ColumnDefinitions[i].Width.Value;

            for (int i = 0; i < nViews; i++)
                selectedTabs[i] = m_currentViews[i].SelectedIndex;

            Configuration.SetValue("View" + nViews + " Distances Pct", distances);
            Configuration.SetValue("View" + nViews + " Selected Tabs", selectedTabs);
        }

        private void ui_lvPackets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateViews();
        }
        #endregion
    }
}
