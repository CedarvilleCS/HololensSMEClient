using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for PanoramaWindow.xaml
    /// </summary>
    public partial class PanoramaWindow : Window
    {
        #region Private Fields

        /// <summary>
        /// URL to connect to
        /// </summary>
        private readonly Uri _conURI;

        /// <summary>
        /// Location of temp dir
        /// </summary>
        private const string _tmpLocation = "C:\\tmp";

        /// <summary>
        /// Is the height being adjusted
        /// </summary>
        private bool? _adjustingHeight = null;
        private string _ip;
        private string _userName;
        private string _password;
        private string _streamQuality;
        private Func<string> toString;

        #endregion

        #region Constuctor

        public PanoramaWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Methods

        public BitmapImage CaptureScreen()
        {
            string tmpBmpPath = System.IO.Path.Combine(_tmpLocation, System.IO.Path.GetRandomFileName().Substring(0, 5));
            tmpBmpPath += ".bmp";
            //mediaControl.MediaPlayer.TakeSnapshot(tmpBmpPath);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(tmpBmpPath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            File.Delete(tmpBmpPath);

            return bitmap.Clone();
        }
        #endregion

        #region Event Handlers

        private void Window_Closed(object sender, EventArgs e)
        {
            Directory.Delete(_tmpLocation);
            var windows = Application.Current.Windows;
            foreach (var item in windows)
            {
                (item as Window).Close();
            }

            Application.Current.Shutdown();
        }

        #endregion
    }
}

