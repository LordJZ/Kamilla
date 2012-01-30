using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kamilla;
using Kamilla.Network;
using Kamilla.Network.Viewing;
using Kamilla.WPF;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    partial class SearchWindow : Window
    {
        const int s_nRecentSearches = 15;
        MainWindow m_window;
        ObservableCollection<string> m_recentSearches;
        string[] m_opcodeNames;

        public SearchWindow(MainWindow window)
        {
            this.Owner = window;
            InitializeComponent();

            m_window = window;
            this.Style = window.Style;
            window.Implementation.StyleChanged += (o, e) => this.Style = this.Owner.Style;
            window.Implementation.ProtocolChanged +=
                new ProtocolChangedEventHandler(Implementation_ProtocolChanged);

            if (window.CurrentProtocol != null && window.CurrentProtocol.OpcodesEnumType != null)
                this.UpdateOpcodeNames();

            var modeRadioButtons = new[]
            {
                ui_rbBinaryContents,
                ui_rbOpcodes,
                ui_rbPacketContents,
                ui_rbParserOutput,
                ui_rbTextContents
            };

            // Load Recent Searches, must be first
            var recentSearches = Configuration.GetValue("Searches", new List<string>());
            int delta = recentSearches.Count - s_nRecentSearches;
            if (delta > 0)
                recentSearches.RemoveRange(s_nRecentSearches, delta);

            m_recentSearches = new ObservableCollection<string>(recentSearches);

            // Load Search Mode
            var mode = Configuration.GetValue("Mode", SearchMode.Opcodes);
            if (!Enum.IsDefined(typeof(SearchMode), mode))
                mode = SearchMode.Opcodes;

            var modeString = mode.ToString();
            modeRadioButtons.Single(button => (string)button.Tag == modeString).IsChecked = true;

            // Load Misc Options
            ui_cbMatchCase.IsChecked = Configuration.GetValue("CaseSensitive", false);
            ui_cbRegex.IsChecked = Configuration.GetValue("Regex", false);
            ui_cbAllowSpecialChars.IsChecked = Configuration.GetValue("Chars", false);
        }

        void Implementation_ProtocolChanged(object sender, ProtocolChangedEventArgs e)
        {
            var type = e.NewProtocol != null ? e.NewProtocol.OpcodesEnumType : null;
            if (type == null)
            {
                ui_rbOpcodes.IsEnabled = false;
                if (ui_rbOpcodes.IsChecked == true)
                    ui_cbSearch.ItemsSource = null;
            }
            else
            {
                ui_rbOpcodes.IsEnabled = true;
                this.UpdateOpcodeNames();
                if (ui_rbOpcodes.IsChecked == true)
                    ui_cbSearch.ItemsSource = m_opcodeNames;
            }
        }

        void UpdateOpcodeNames()
        {
            var fields = m_window.CurrentProtocol.OpcodesEnumType
                .GetFields(BindingFlags.Static | BindingFlags.Public);

            var list = new List<string>(fields.Length);
            foreach (var field in fields)
            {
                var value = (uint)field.GetRawConstantValue();
                if (value != SpecialOpcodes.UnknownOpcode)
                    list.Add(field.Name);
            }
            list.Sort();

            m_opcodeNames = list.ToArray();
        }

        private void ui_rbSearchMode_Checked(object sender, RoutedEventArgs e)
        {
            var button = (RadioButton)sender;
            var mode = (SearchMode)Enum.Parse(typeof(SearchMode), (string)button.Tag);

            m_window.m_searchMode = mode;

            ui_cbAllowSpecialChars.IsEnabled =
            ui_cbRegex.IsEnabled =
            ui_cbMatchCase.IsEnabled =
                mode != SearchMode.Opcodes &&
                mode != SearchMode.PacketContents &&
                mode != SearchMode.BinaryContents;

            if (mode == SearchMode.Opcodes)
                ui_cbSearch.ItemsSource = m_opcodeNames;
            else
                ui_cbSearch.ItemsSource = m_recentSearches;

            ui_tbModeDescription.Text = Strings.ResourceManager.GetString("SearchModeDesc_" + mode.ToString());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ui_btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ui_cbRegex_Checked(object sender, RoutedEventArgs e)
        {
            m_window.m_regex = true;
        }

        private void ui_cbRegex_Unchecked(object sender, RoutedEventArgs e)
        {
            m_window.m_regex = false;
        }

        private void ui_cbMatchCase_Checked(object sender, RoutedEventArgs e)
        {
            m_window.m_matchCase = true;
        }

        private void ui_cbMatchCase_Unchecked(object sender, RoutedEventArgs e)
        {
            m_window.m_matchCase = false;
        }

        private void ui_cbAllowSpecialChars_Checked(object sender, RoutedEventArgs e)
        {
            m_window.m_allowChars = true;
        }

        private void ui_cbAllowSpecialChars_Unchecked(object sender, RoutedEventArgs e)
        {
            m_window.m_allowChars = false;
        }

        private void btn_SearchDown_Click(object sender, RoutedEventArgs e)
        {
            var matcher = m_window.GetSearchMatcher();
            if (matcher == null)
                return;

            m_window.StartSearch(new SearchRequest(true, true, matcher, FinishSearch));
        }

        private void ui_btnSearchUp_Click(object sender, RoutedEventArgs e)
        {
            var matcher = m_window.GetSearchMatcher();
            if (matcher == null)
                return;

            m_window.StartSearch(new SearchRequest(false, true, matcher, FinishSearch));
        }

        void FinishSearch(ViewerItem item)
        {
            if (item != null)
            {
                m_window.ui_lvPackets.SelectedIndex = item.Index;
                m_window.ui_lvPackets.ScrollIntoView(item);
            }
            else
                MessageWindow.Show(this, Strings.Menu_Search, Strings.Search_NotFound);
        }
    }

    internal class SearchRequest
    {
        /// <summary>
        /// Search should be performed down.
        /// </summary>
        public readonly bool IsDown;

        /// <summary>
        /// Search should be continued from the current position.
        /// </summary>
        public readonly bool IsContinue;

        /// <summary>
        /// Determines whether the item matches the search criteria.
        /// </summary>
        public readonly Predicate<ViewerItem> Matches;

        /// <summary>
        /// Called from the interface thread when the search completed.
        /// </summary>
        public readonly Action<ViewerItem> Completed;

        /// <summary>
        /// Initializes a new instance of SearchRequest class.
        /// </summary>
        /// <param name="isDown">
        /// Search should be performed down.
        /// </param>
        /// <param name="isContinue">
        /// Search should be continued from the current position.
        /// </param>
        /// <param name="matches">
        /// Determines whether the item matches the search criteria.
        /// </param>
        /// <param name="completed">
        /// Called from the interface thread when the search completed.
        /// </param>
        public SearchRequest(bool isDown, bool isContinue,
            Predicate<ViewerItem> matches, Action<ViewerItem> completed)
        {
            if (matches == null)
                throw new ArgumentNullException("matches");

            if (completed == null)
                throw new ArgumentNullException("completed");

            this.IsDown = isDown;
            this.IsContinue = isContinue;
            this.Matches = matches;
            this.Completed = completed;
        }
    }

    internal enum SearchMode
    {
        Opcodes,
        ParserOutput,
        PacketContents,
        BinaryContents,
        TextContents,
    }
}
