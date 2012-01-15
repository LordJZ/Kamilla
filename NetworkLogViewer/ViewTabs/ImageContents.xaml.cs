using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using System.IO;
using Kamilla;
using GdiBitmap = System.Drawing.Bitmap;

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

        void IViewTab.Reset()
        {
            ui_image.Source = null;
            this.IsFilled = false;
        }

        void IViewTab.Fill(Protocol protocol, ViewerItem item)
        {
            var parser = item.Parser;
            if (parser == null)
            {
                protocol.CreateParser(item);
                parser = item.Parser;
            }

            if (parser.IsParsed)
                parser.Parse();

            ImageSource source = null;
            bool fail = true;

            var data = parser.ContainedData;
            if (fail && data is GdiBitmap)
            {
                fail = false;
                var image = (GdiBitmap)data;

                var handle = image.GetHbitmap();
                try
                {
                    source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                catch
                {
                    fail = true;
                }
                finally
                {
                    Win32.DeleteObject(handle);
                }
            }
            if (fail && data is byte[])
            {
                fail = false;
                var arr = (byte[])data;
                using (var stream = new MemoryStream(arr))
                {
                    try
                    {
                        source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    catch
                    {
                        fail = true;
                    }
                }
            }

            ui_btnSave.IsEnabled = source != null;
            ui_cbFormat.IsEnabled = source != null;

            if (source != null)
                ui_image.Source = source;

            this.IsFilled = true;
        }
    }
}
