using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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
using PDFViewer;

namespace ARTAPclient
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    /// 
    public partial class ScreenshotAnnotationsWindow : Window
    {
        #region Fields

        /// <summary>
        /// Current bitmap active for drawing
        /// </summary>
        private ImageSource _activeBMP;

        /// <summary>
        /// History of images snapped from the stream
        /// </summary>
        private List<ImageSource> _imageHistory = new List<ImageSource>();

        /// <summary>
        /// History of orignal images snapped from the stream
        /// </summary>
        private List<ImageSource> _imageOriginals = new List<ImageSource>();

        /// <summary>
        /// List containing all of the thumbnail pictureboxes to make updating easy
        /// </summary>
        private List<Image> _pictureBoxThumbnails = new List<Image>();

        /// <summary>
        /// List containing all of the thumbnail pictureboxes to make updating easy
        /// </summary>
        private List<UIElement[]>_annotations = new List<UIElement[]>();

        /// <summary>
        /// Number of thumbnail images
        /// </summary>
        private const int NUMTHUMBNAILS = 5;

        /// <summary>
        /// Int to track current image
        /// </summary>
        private int _currentImage = 0;

        /// <summary>
        /// Int to track current image
        /// </summary>
        private int _annotationIndex = 0;

        /// <summary>
        /// Polyline used for canvas annotations
        /// </summary>
        private Polyline _polyLine;

        /// <summary>
        /// Point used for canvas annotations
        /// </summary>
        private Point _currentPoint = new Point();

        /// <summary>
        /// X DPI of the screen
        /// </summary>
        private const int DPIX = 96;

        /// <summary>
        /// Y DPI of the screen
        /// </summary>
        private const int DPIY = 96;

        /// <summary>
        /// Window that the video view is on
        /// </summary>
        private VideoStreamWindow _videoStreamWindow;

        /// <summary>
        /// Connection to the HoloLens
        /// </summary>
        private AsynchronousSocketListener _listener;

        #endregion

        #region Constructor

        public ScreenshotAnnotationsWindow(VideoStreamWindow videoStreamWindow, AsynchronousSocketListener listener)
        {
            InitializeComponent();
            _pictureBoxThumbnails.Add(imageThumb);
            _pictureBoxThumbnails.Add(imageThumb1);
            _pictureBoxThumbnails.Add(imageThumb2);
            _pictureBoxThumbnails.Add(imageThumb3);
            _pictureBoxThumbnails.Add(imageThumb4);

            _videoStreamWindow = videoStreamWindow;
            _listener = listener;
            _listener.ConnectionClosed += _listener_ConnectionClosed;
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Updates the thumbnails with the latest images captured
        /// </summary>
        private void UpdateThumbnails()
        {
            //Number of thumbnails presumed to be set
            int numActiveThumbnails = NUMTHUMBNAILS;

            //If there aren't enough history images for all 5, adjust
            if (_imageHistory.Count < 5)
            {
                numActiveThumbnails = _imageHistory.Count;
            }

            //Loop through and update images for all of the thumbnail frames
            for (int i = 0; i < numActiveThumbnails; i++)
            {
                //ImageSource test = new Media();
                _pictureBoxThumbnails[i].Source = _imageHistory[i].Clone();
            }
        }


        private void renderImage()
        {
            if (_imageHistory.Count != 0) {
                Rect bounds = VisualTreeHelper.GetDescendantBounds(capturedImage);
                RenderTargetBitmap rtb =
                    new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                    DPIX, DPIY, System.Windows.Media.PixelFormats.Default);

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(capturedImage);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);

                _activeBMP = rtb.Clone();
                _imageHistory[_currentImage] = rtb.Clone();
                UpdateThumbnails();
            }
        }

        private void thumbnailSelect(int thumbnailNum)
        {
            _currentImage = thumbnailNum;

            capturedImage.Children.Clear();
            int index = 0;

            while (_annotations[_currentImage][index] != null)
            {
                capturedImage.Children.Add(_annotations[_currentImage][index]);
                index++;
            }

            _annotationIndex = index;
            ImageBrush ib = new ImageBrush();
            ib.ImageSource = _imageOriginals[_currentImage];
            capturedImage.Background = ib;
            _activeBMP = _imageOriginals[_currentImage];
        }

        #endregion

        #region EventHandlers

        private void _listener_ConnectionClosed(object sender, EventArgs e)
        {
            MessageBox.Show("Connection to HoloLens lost.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void capturedImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _currentPoint = e.GetPosition(this);
            }

            if (capturedImage.Children.Count != 0)
            {
                Polyline polyLine;
                polyLine = new Polyline();
                polyLine.Stroke = new SolidColorBrush(Colors.Black);
                polyLine.StrokeThickness = 5;

                capturedImage.Children.Add(polyLine);
                _annotations[_currentImage][_annotationIndex] = polyLine;
            }
        }

        private void capturedImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _activeBMP != null)
            {
                if (capturedImage.Children.Count == 0)
                {
                    _polyLine = new Polyline();
                    _polyLine.Stroke = new SolidColorBrush(Colors.Black);
                    _polyLine.StrokeThickness = 5;

                    capturedImage.Children.Add(_polyLine);
                    _annotations[_currentImage][0] = _polyLine;
                    _annotationIndex = 0;
                }

                _polyLine = (Polyline)capturedImage.Children[_annotationIndex];
                Point currentPoint = e.GetPosition(capturedImage);
                _polyLine.Points.Add(currentPoint);
            }
        }

        private void capturedImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            renderImage();
            _annotationIndex++;

        }

        private void buttonCaptureScreenshot_Click(object sender, RoutedEventArgs e)
        {
            ImageSource screenshot = _videoStreamWindow.CaptureScreen();
            _activeBMP = screenshot;
            ImageBrush ib = new ImageBrush();
            ib.ImageSource = screenshot;
            capturedImage.Children.Clear();
            capturedImage.Background = ib;
            if (_imageHistory.Count >= 5)
            {
                _imageHistory.RemoveAt(4);
                _imageOriginals.RemoveAt(4);
            }
            _imageHistory.Insert(0, screenshot.Clone());
            _imageOriginals.Insert(0, screenshot.Clone());
            UIElement[] elementArray = new UIElement[10];
            _annotations.Insert(0, elementArray);
            _currentImage = 0;
            UpdateThumbnails();
        }

        private BitmapImage ConvertBitmapType(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_annotationIndex > 0)
            {
                capturedImage.Children.RemoveAt(_annotationIndex - 1);
                _annotations[_currentImage][_annotationIndex - 1] = null;

                renderImage();
                _annotationIndex--;
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            capturedImage.Children.Clear();
            _annotations[_currentImage] = new UIElement[10];

            renderImage();
            _annotationIndex = 0;
        }

        private void imageThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            thumbnailSelect(0);
        }

        private void imageThumb1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            thumbnailSelect(1);
        }

        private void imageThumb2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            thumbnailSelect(2);
        }

        private void imageThumb3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            thumbnailSelect(3);
        }

        private void imageThumb4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            thumbnailSelect(4);
        }

        private void buttonSendScreenshot_Click(object sender, RoutedEventArgs e)
        {
            _listener.SendBitmap(_imageHistory[_currentImage]);
        }

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            _listener.CloseConnection();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();

            openDialog.DefaultExt = ".pdf";
            openDialog.Filter = "PDF Documents (.pdf) | *.pdf";

            Nullable<bool> result = openDialog.ShowDialog();

            if(result == true)
            {
                string pdfFile = openDialog.FileName;
                PDFViewer.PDFViewerDialog pdfDialog = new PDFViewerDialog(pdfFile);
                result = pdfDialog.ShowDialog();
                if(result == true)
                {

                }
            }
        }
    }
}
