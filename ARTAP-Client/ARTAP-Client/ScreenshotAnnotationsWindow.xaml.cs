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
using WpfApplication1;
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
        private AnnotatedImage _activeImage;

        /// <summary>
        /// History of images snapped or uploaded
        /// </summary>
        private List<AnnotatedImage> _imageHistory = new List<AnnotatedImage>();

        /// <summary>
        /// Inedex of current image in _imageHistory
        /// </summary>
        private int _currentImageIndex = 0;

        ///// <summary>
        ///// History of images snapped from the stream
        ///// </summary>
        //private List<ImageSource> _imageHistory = new List<ImageSource>();

        ///// <summary>
        ///// History of orignal images snapped from the stream
        ///// </summary>
        //private List<ImageSource> _imageOriginals = new List<ImageSource>();

        /// <summary>
        /// List containing all of the thumbnail pictureboxes to make updating easy
        /// </summary>
        private List<Image> _pictureBoxThumbnails = new List<Image>();

        /// <summary>
        /// List of all of the buttons we want to enable/disable during arrow placement
        /// </summary>
        private List<Button> _editButtons = new List<Button>();

        ///// <summary>
        ///// List containing all of the thumbnail pictureboxes to make updating easy
        ///// </summary>
        //private List<Polyline[]>_annotations = new List<Polyline[]>();

        /// <summary>
        /// Number of thumbnail images
        /// </summary>
        private const int NUMTHUMBNAILS = 5;

        /// <summary>
        /// Color used for canvas annotations, default to Red
        /// </summary>
        private Color _brushColor = Colors.Red;

        /// <summary>
        /// Size used for canvas annotations, default to 5
        /// </summary>
        private double _brushSize = 5;

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

        /// <summary>
        /// Are we currently in "Arrow place mode?"
        /// </summary>
        private bool _placingArrow;

        /// <summary>
        /// Was an arrow placed on the current image?"
        /// </summary>
        private bool _placedArrow;

        private int _thumbIndex = 0;


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
            _editButtons.Add(buttonClear);
            _editButtons.Add(buttonUndo);
            _editButtons.Add(buttonChangeColor);
            _editButtons.Add(buttonUploadImage);
            _editButtons.Add(buttonUploadImage);
            _editButtons.Add(buttonSendScreenshot);

            _videoStreamWindow = videoStreamWindow;
            _listener = listener;
            //_listener.ConnectionClosed += _listener_ConnectionClosed;
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Updates the thumbnails with the latest images captured
        /// </summary>
        private void UpdateThumbnails()
        {
            int numActiveThumbnails = 5;

            //Only use the number of active images
            if (_imageHistory.Count < 5)
            {
                numActiveThumbnails = _imageHistory.Count;

            }

            //int numActiveThumbnails = (_imageHistory.Count < 5) ? 
            //    _imageHistory.Count : NUMTHUMBNAILS;

            int index = _thumbIndex;
            //Loop through and update images for all of the thumbnail frames
            for (int i = 0; i < numActiveThumbnails; i++, index++)
            {
                _pictureBoxThumbnails[i].Source = _imageHistory[index].LatestImage;
            }
        }

        /// <summary>
        /// Render image to canvas
        /// </summary>
        private void SaveCanvasToActiveImage()
        {
            if (_imageHistory.Count != 0) {
                Rect bounds = VisualTreeHelper.GetDescendantBounds(canvasImageEditor);
                RenderTargetBitmap rtb =
                    new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                    DPIX, DPIY, System.Windows.Media.PixelFormats.Default);

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(canvasImageEditor);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);

                _activeImage.LatestImage = rtb.Clone();
                UpdateThumbnails();
            }
        }

        /// <summary>
        /// Loads the thumbnail image passed
        /// </summary>
        /// <param name="thumbnailNum">Thumnail num to load</param>
        public void SelectThumbnail(int thumbnailNum)
        {
            _currentImageIndex = thumbnailNum;
            _activeImage = _imageHistory[_currentImageIndex];
            _placedArrow = _imageHistory[_currentImageIndex].ArrowPlaced;
            CheckArrowPlacementAllowed();

            //Draw the orignal image to the canvas
            DrawImageToCanvas(_activeImage.OriginalImage);

            //Add the annotations over top
            foreach (Polyline line in _activeImage.GetAnnotations())
            {
                canvasImageEditor.Children.Add(line);
            }
        }

        /// <summary>
        /// Changes the Image that is being edited and updates the
        /// history thumbnails.
        /// </summary>
        /// <param name="source">Image to be displayed</param>
        private void AddNewImage(AnnotatedImage source)
        {
            DrawImageToCanvas(source.OriginalImage);

            //if (_imageHistory.Count >= 5)
            //{
            //    _imageHistory.RemoveAt(4);
            //}

            _activeImage = source;
            _imageHistory.Insert(0, source);
            _currentImageIndex = 0;
            CheckArrowPlacementAllowed();
            UpdateThumbnails();
        }

        /// <summary>
        /// Renders image to the canvas and clears any annotations
        /// </summary>
        /// <param name="image">Image to draw</param>
        private void DrawImageToCanvas(ImageSource image)
        {
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.Uniform;
            ib.ImageSource = image.Clone();
            canvasImageEditor.Children.Clear();
            if(image.Height < image.Width)
            {
                canvasImageEditor.Width = (360/image.Height) *image.Width;
                canvasImageEditor.Height = 360;
            } else
            {
                canvasImageEditor.Width = 640;
                canvasImageEditor.Height = (640/image.Width)*image.Height;
            }
            //canvasImageEditor.Width = image.Width;
            //canvasImageEditor.Height = image.Height;
            canvasImageEditor.Background = ib;
            canvasImageEditor.InvalidateVisual();
        }

        /// <summary>
        /// Checks if arrow placement can be preformed on selected image
        /// and enables/disables corresponding buttons
        /// </summary>
        private void CheckArrowPlacementAllowed()
        {
            if(_activeImage is LocatableImage)
            {
                buttonPlaceArrow.IsEnabled = true;
                buttonSendArrow.IsEnabled = true;
            }
            else
            {
                buttonPlaceArrow.IsEnabled = false;
                buttonSendArrow.IsEnabled = false;
            }
        }

        /// <summary>
        /// Enables or disables editing buttons and selection of thumbnails
        /// </summary>
        /// <param name="enabled">True for enable, false for disable</param>
        private void ControlsEnabled(bool enabled)
        {
            foreach(Button button in _editButtons)
            {
                button.IsEnabled = enabled;
            }
            foreach(Image thumb in _pictureBoxThumbnails)
            {
                thumb.IsEnabled = enabled;
            }
        }

        /// <summary>
        /// Sets visibility of all elements drawn on canvas
        /// </summary>
        /// <param name="visibility">Visibility to set to</param>
        private void SetAnnotationsVisibility(Visibility visibility)
        {
            foreach (UIElement line in canvasImageEditor.Children)
            {
                line.Visibility = visibility;
            }
        }

        #endregion

        #region EventHandlers

        private void _listener_ConnectionClosed(object sender, EventArgs e)
        {
            MessageBox.Show("Connection to HoloLens lost.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void canvasImageEditor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_placingArrow)
            {
                Point relativeClickPoint = e.GetPosition((Canvas)sender);
                double x = (_activeImage.OriginalImage.Width / canvasImageEditor.Width) * relativeClickPoint.X;
                double y = (_activeImage.OriginalImage.Height / canvasImageEditor.Height) * relativeClickPoint.Y;

                Point absoluteClickPoint = new Point(x, y);
                ((LocatableImage)_activeImage).ArrowPosition = absoluteClickPoint;
                _placedArrow = true;
                ///
                /// TODO: Place a marker on the arrow drop location
                ///
            }
            else
            {
                if (canvasImageEditor.Children.Count != 0)
                {
                    Polyline polyLine = new Polyline();
                    polyLine.Stroke = new SolidColorBrush(_brushColor);
                    polyLine.StrokeThickness = _brushSize;

                    //Add line to the canvas
                    canvasImageEditor.Children.Add(polyLine);
                    //Add memory of line to AnnotatedImage
                    _activeImage.AddAnnotation(polyLine);
                }
            }
           
        }

        private void canvasImageEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _activeImage != null)
            {
                Polyline polyLine = new Polyline();
                if (canvasImageEditor.Children.Count == 0)
                {
                    polyLine.Stroke = new SolidColorBrush(_brushColor);
                    polyLine.StrokeThickness = _brushSize;

                    canvasImageEditor.Children.Add(polyLine);
                    _activeImage.AddAnnotation(polyLine);
                }

                polyLine = (Polyline)canvasImageEditor.Children[_activeImage.CurrentAnnotationIndex];
                Point currentPoint = e.GetPosition(canvasImageEditor);
                polyLine.Points.Add(currentPoint);
            }
        }

        private void canvasImageEditor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveCanvasToActiveImage();
        }

        private void buttonCaptureScreenshot_Click(object sender, RoutedEventArgs e)
        {
            ImageSource screenshot = _videoStreamWindow.CaptureScreen();
            LocatableImage img = new LocatableImage(screenshot);
            AddNewImage(img);
            _listener.RequestLocationID(img);
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeImage.CurrentAnnotationIndex >= 0)
            {
                canvasImageEditor.Children.RemoveAt(_activeImage.CurrentAnnotationIndex);
                _activeImage.UndoAnnotation();

                SaveCanvasToActiveImage();
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            canvasImageEditor.Children.Clear();
            _activeImage.ClearAnnotations();
            SaveCanvasToActiveImage();
        }

        private void imageThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectThumbnail(0);
        }

        private void imageThumb1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectThumbnail(1);
        }

        private void imageThumb2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectThumbnail(2);
        }

        private void imageThumb3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectThumbnail(3);
        }

        private void imageThumb4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectThumbnail(4);
        }

        private void buttonSendScreenshot_Click(object sender, RoutedEventArgs e)
        {
            if (_placedArrow)
            {
                _listener.SendArrowLocation((LocatableImage)_activeImage);
                _placingArrow = false;
                _placedArrow = false;
                SetAnnotationsVisibility(Visibility.Visible);
                ControlsEnabled(true);
            }

            _listener.SendBitmap(_activeImage.LatestImage);
            
        }

        private void LoadPDF_Click(object sender, RoutedEventArgs e)
        {
            //
            // Allow the user to choose a file
            //
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();

            openDialog.DefaultExt = ".pdf";
            openDialog.Filter = "PDF Documents (.pdf) | *.pdf";

            bool? result = openDialog.ShowDialog();

            if(result == true)
            {
                string pdfFile = openDialog.FileName;
                PDFViewer.PDFViewerDialog pdfDialog = new PDFViewerDialog(pdfFile);
                result = pdfDialog.ShowDialog();
                if(result == true)
                {
                    ImageSource img = pdfDialog.selectedImage;
                    AddNewImage(new AnnotatedImage(img));
                }
            }
        }

        private void buttonUploadImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri imageUri = new Uri(openFileDialog.FileName, UriKind.Relative);
                ImageSource img = new BitmapImage(imageUri);
                AddNewImage(new AnnotatedImage(img));
            }

        }

        private void buttonChangeColor_Click(object sender, RoutedEventArgs e)
        {
            WPFColorPickerLib.ColorDialog colorDialog = new WPFColorPickerLib.ColorDialog();
            colorDialog.SelectedColor = _brushColor;
            colorDialog.SelectedSize = _brushSize;
            colorDialog.Owner = this;
            if ((bool)colorDialog.ShowDialog())
            {
                _brushColor = colorDialog.SelectedColor;
                _brushSize = colorDialog.SelectedSize;
            }
        }

        private void buttonPlaceArrow_Click(object sender, RoutedEventArgs e)
        {
            if (_placingArrow == false)
            {
                _placingArrow = true;
                //Temporarily hide all drawings on canvas
                SetAnnotationsVisibility(Visibility.Hidden);
                ControlsEnabled(false);
                buttonPlaceArrow.Background = Brushes.LightGreen;
            }
            else
            {
                _placingArrow = false;
                //Temporarily hide all drawings on canvas
                SetAnnotationsVisibility(Visibility.Visible);
                ControlsEnabled(true);
                buttonPlaceArrow.Background = Brushes.LightGray;
            }
        }

        private void buttonSendArrow_Click(object sender, RoutedEventArgs e)
        {
            _listener.SendArrowLocation((LocatableImage)_activeImage);
            _placingArrow = false;
            SetAnnotationsVisibility(Visibility.Visible);
            ControlsEnabled(true);
        }

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _thumbIndex++;
            UpdateThumbnails();
        }
    }
}
