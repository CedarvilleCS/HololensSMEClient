using ARTAPclient;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PanoramaWindow.xaml
    /// </summary>
    public partial class PanoramaWindow : Window
    {
        #region Private Fields
        
        /// <summary>
        /// Location of temp dir
        /// </summary>
        private const string _tmpLocation = "C:\\tmp";
        
        private Bitmap _panoramaBitmap;
        private BitmapImage _panoramaBitmapSource;
        private System.Drawing.Pen _headPositionPen;

        public float[] HeadPositionCoordinates { get; set; }
        public static int test = 0;

        public AsynchronousSocketListener _socketListener;

        private DispatcherTimer _checkHeadData;

        #endregion

        #region Constuctor

        public PanoramaWindow(AsynchronousSocketListener listener)
        {
            _panoramaBitmap = null;
            _socketListener = listener;
            InitializeComponent();
            PollPanoImage();
            HeadPositionCoordinates = new float[2];
            _headPositionPen = new System.Drawing.Pen(new SolidBrush(System.Drawing.Color.Red), 7);

            _checkHeadData = new DispatcherTimer();
            _checkHeadData.Tick += new EventHandler(CheckIfHeadDataUpdated);
            _checkHeadData.Interval = new TimeSpan(0, 0, 0, 0, 100);

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        #endregion

        #region Public Methods

        public void HeadPosition_TimerElapsed(object sender, EventArgs e)
        {
            _socketListener.RequestHeadPosition();
            _checkHeadData.Start();
        }

        private void CheckIfHeadDataUpdated(object sender, EventArgs e)
        {
            var drawnImage = ConvertImageSourceToBitmap();
            panoImage.Source = drawnImage;
        }

        public BitmapImage CaptureScreen()
        {
            string tmpBmpPath = Path.Combine(_tmpLocation, Path.GetRandomFileName().Substring(0, 5));
            tmpBmpPath += ".bmp";

            var bitmap = new BitmapImage();
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
        private void PollPanoImage()
        {
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }



        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_socketListener.IsPanoDone)
            {
                _panoramaBitmapSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "pano.png"));
                panoImage.Source = _panoramaBitmapSource;
                dispatcherTimer.Stop();
            }
        }

        private BitmapImage AddHeadPositionBox()
        {

            using (var graphics = Graphics.FromImage(_panoramaBitmap))
            {
                var coordinates = AdjustCoordinates((1 - HeadPositionCoordinates[0]), 1 - HeadPositionCoordinates[1]);
                graphics.DrawRectangle(_headPositionPen, coordinates[0] - 50, coordinates[1] - 18, coordinates[0] + 50, coordinates[1] + 18);
                return ConvertBitmaptoImageSource();
            }
        }

        public float[] AdjustCoordinates(float x, float y)
        {
            if (x > 1)
            {
                x = 1;
            }
            else if (x < 0)
            {
                x = 0;
            }

            if (y > 1)
            {
                y = 1;
            }
            else if (y < 0)
            {
                y = 0;
            }

            var xCoordinate = _panoramaBitmap.Width * x;
            var yCoordinate = _panoramaBitmap.Height * y;

            return new float[] { xCoordinate, yCoordinate };
        }

        public BitmapImage ConvertImageSourceToBitmap()
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/73d4ff45-d1aa-457e-8048-c35a79b35c19/image-control-to-bitmap?forum=csharpgeneral
            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_panoramaBitmapSource));
                encoder.Save(ms);
                _panoramaBitmap = new Bitmap(ms);
                return AddHeadPositionBox();
            }
        }

        public BitmapImage ConvertBitmaptoImageSource()
        {
            // https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
            var bitmapimage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                _panoramaBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
            }

            return bitmapimage;
        }

        #endregion
    }
}

