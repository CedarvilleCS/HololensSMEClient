using ARTAPclient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        public byte[] _headPositionData;
        private Func<string> toString;

        public AsynchronousSocketListener _socketListener;

        #endregion

        #region Constuctor

        public PanoramaWindow(AsynchronousSocketListener listener)
        {
            _socketListener = listener;
            InitializeComponent();
            pollPanoImage();
            _headPositionData = new byte[44];
        }
        #endregion

        #region Public Methods

        public void AddImage(ImageSource image)
        {
            //panoImage.Source = image;
            //ImageBrush img = new ImageBrush();
            //img.ImageSource = image;
            //PanoramaGrid.Background = img;
        }
        public void HeadPosition_TimerElapsed(object sender, EventArgs e)
        {
            _socketListener.RequestHeadPosition(_headPositionData);
        }

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


        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private void pollPanoImage()
        {
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(_socketListener.IsPanoDone)
            {
                panoImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "pano.png"));
                dispatcherTimer.Stop();
            }
        }

        #endregion
    }
}

