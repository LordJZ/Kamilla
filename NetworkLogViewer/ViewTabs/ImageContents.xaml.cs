using System.Windows.Controls;
using System.Windows.Media;
using Kamilla;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;

namespace NetworkLogViewer.ViewTabs
{
    /// <summary>
    /// Interaction logic for ImageContents.xaml
    /// </summary>
    public partial class ImageContents : UserControl, IViewTab
    {
        public ImageContents()
        {
            InitializeComponent();
        }

        string IViewTab.Header { get { return Strings.View_Image; } }

        public bool IsFilled { get; set; }

        ValueTuple<object, ImageSource>[] m_images;

        void IViewTab.Reset()
        {
            m_images = null;
            ui_image.Source = null;
            ui_cbImages.IsEnabled = false;
            ui_cbImages.Items.Clear();
            ui_cbImages.Items.Add(Strings.View_NoImages);
            ui_cbImages.SelectedIndex = 0;
            ui_cbFormat.IsEnabled = false;
            ui_btnSave.IsEnabled = false;

            this.IsFilled = false;
        }

        void SelectImage(int index)
        {
            ui_image.Source = m_images[index].Item2;

            var obj = m_images[index].Item1;
            if (obj is ImageSource)
            {
                ui_cbFormat.IsEnabled = true;
                ui_btnSave.IsEnabled = true;
            }
            else
            {
                ui_cbFormat.IsEnabled = false;
                ui_btnSave.IsEnabled = false;
            }
        }

        void IViewTab.Fill(Protocol protocol, ViewerItem item)
        {
            ((IViewTab)this).Reset();

            m_images = ParsingHelper.ExtractImages(protocol, item, true);
            int count = m_images.Length;

            if (count > 0)
            {
                ui_cbImages.Items.Clear();

                for (int i = 0; i < count; i++)
                {
                    var cbItem = new ComboBoxItem();

                    cbItem.Content = ParsingHelper.GetContentName(m_images[i].Item1, i);

                    ui_cbImages.Items.Add(cbItem);
                }

                ui_cbImages.SelectedIndex = 0;

                if (count > 1)
                    ui_cbImages.IsEnabled = true;

                //this.SelectImage(0);
            }

            this.IsFilled = true;
        }

        private void ui_cbImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ui_cbImages.SelectedIndex;

            if (index >= 0 && m_images != null && m_images.Length > 0)
                this.SelectImage(index);

            e.Handled = true;
        }
    }
}
