using ARTAPclient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for thumbnailWindow.xaml
    /// </summary>
    public partial class thumbnailWindow : Window
    {

        /// <summary>
        /// List containing all of the thumbnail pictureboxes to make updating easy
        /// </summary>
        private List<Image> _pictureBoxThumbnails = new List<Image>();

        ScreenshotAnnotationsWindow _screenshotAnnotationsWindow;

        public List<Image> PictureBoxThumbnails
        {
            get
            {
                return _pictureBoxThumbnails;
            }
            set
            {
                _pictureBoxThumbnails = value;
            }
        }


        public thumbnailWindow(ScreenshotAnnotationsWindow screenshotAnnotationsWindow)
        {
            InitializeComponent();
            _screenshotAnnotationsWindow = screenshotAnnotationsWindow;

        }

        private void imageThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _screenshotAnnotationsWindow.SelectThumbnail(0);

        }

        private void imageThumb1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _screenshotAnnotationsWindow.SelectThumbnail(1);
        }

        private void imageThumb2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _screenshotAnnotationsWindow.SelectThumbnail(2);
        }

        private void imageThumb3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _screenshotAnnotationsWindow.SelectThumbnail(3);
        }

        private void imageThumb4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _screenshotAnnotationsWindow.SelectThumbnail(4);
        }

    }
}
