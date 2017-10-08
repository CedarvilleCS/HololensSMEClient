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
using System.Diagnostics;

namespace ARTAPclient
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    /// 
    public partial class ScreenshotAnnotationsWindow : MahApps.Metro.Controls.MetroWindow
    {
        #region Fields

        /// <summary>
        /// Maximum number of images to hold in the history.
        /// </summary>
        private const int MAX_IMAGE_HISTORY_SIZE = 50;

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
        private bool _placingMarker;

        private int _thumbIndex = 0;

        private bool _isSelectMultiple = false;

        private List<int> _selectedImages;


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

            _selectedImages = new List<int>();

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

            if ((_thumbIndex + 5) < _imageHistory.Count)
            {
                buttonNext.IsEnabled = true;
            } else
            {
                buttonNext.IsEnabled = false;
            }

            if (_thumbIndex > 0)
            {
                buttonPrev.IsEnabled = true;
            } else
            {
                buttonPrev.IsEnabled = false;
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
            CheckMarkerPlacementAllowed();

            //Draw the orignal image to the canvas
            DrawImageToCanvas(_activeImage.OriginalImage);

            //Add the annotations over top
            foreach (Polyline line in _activeImage.Annotations)
            {
                canvasImageEditor.Children.Add(line);
            }

            //Add the markers if they exist
            if(_activeImage is LocatableImage)
            {
                foreach(Marker m in (_activeImage as LocatableImage).Markers)
                {
                    canvasImageEditor.Children.Add(m.Annotation);
                }
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

            if (_imageHistory.Count >= MAX_IMAGE_HISTORY_SIZE)
            {
                _imageHistory.RemoveAt(_imageHistory.Count - 1);
            }

            _activeImage = source;
            _imageHistory.Insert(0, source);
            _currentImageIndex = 0;
            CheckMarkerPlacementAllowed();
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
            }
            else
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
        /// Checks if marker placement can be preformed on selected image
        /// and enables/disables corresponding buttons
        /// </summary>
        private void CheckMarkerPlacementAllowed()
        {
            if(_activeImage is LocatableImage)
            {
                buttonPlaceArrow.IsEnabled = true;
            }
            else
            {

                //
                // Make sure we are not in marker placing mode
                // if placing markers is not allowed
                //
                SetPlacingMarkers(false);
                buttonPlaceArrow.IsEnabled = false;
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
            if (_activeImage != null && !_isSelectMultiple)
            {
                Debug.WriteLine("Placing Arrow: " + _placingMarker);
                if (_placingMarker)
                {
                    //
                    // Get the pixel value in the original image
                    //
                    Point relativeClickPoint = e.GetPosition((Canvas)sender);
                    int x = (int)((_activeImage.OriginalImage.Width / canvasImageEditor.Width) * relativeClickPoint.X);
                    int y = (int)((_activeImage.OriginalImage.Height / canvasImageEditor.Height) * relativeClickPoint.Y);

                    Point absoluteClickPoint = new Point(x, y);

                    canvasImageEditor.Children.Add((_activeImage as LocatableImage).AddMarker(relativeClickPoint, absoluteClickPoint, _brushColor));

                    //
                    // Enable the undo button for placing arrows
                    //
                    buttonUndo.IsEnabled = true;
                }
                else
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
            if (e.LeftButton == MouseButtonState.Pressed && 
                _activeImage != null &&
                !_placingMarker)
            {
                Polyline polyLine;
                if (_activeImage.NumAnnotations == 0)
                {
                    polyLine = new Polyline();
                    polyLine.Stroke = new SolidColorBrush(_brushColor);
                    polyLine.StrokeThickness = _brushSize;

                    canvasImageEditor.Children.Add(polyLine);
                    _activeImage.AddAnnotation(polyLine);
                }
                else
                {
                    polyLine = (Polyline)_activeImage.GetLastAnnotation();
                }
                
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
            //
            // We can't undo annotations if there is no
            // active image
            //
            if (_activeImage != null) {
                if (_placingMarker) {
                    //
                    // If there are unsent markers we can undo
                    // 
                    if ((_activeImage as LocatableImage).NumMarkers > 0 &&
                       !(_activeImage as LocatableImage).GetLastMarker().Sent)
                    {
                        canvasImageEditor.Children.Remove((_activeImage as LocatableImage).GetLastMarker().Annotation);
                        (_activeImage as LocatableImage).UndoMarker();

                        //
                        // If there are no more unsent markers disable the undo button
                        //
                        buttonUndo.IsEnabled = (_activeImage as LocatableImage).NumMarkers > 0 ||
                                               !(_activeImage as LocatableImage).GetLastMarker().Sent;
                    }
                }
                else
                {
                    if (_activeImage.NumAnnotations > 0)
                    {
                        canvasImageEditor.Children.Remove(_activeImage.GetLastAnnotation());
                        _activeImage.UndoAnnotation();

                        SaveCanvasToActiveImage();
                    }
                }
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeImage != null && !_isSelectMultiple)
            {
                //
                // The sender == buttonClear clause makes sure that
                // if the clear annotations toolbar item is clicked that
                // Annotations will be cleared instead of markers.
                //
                if (_placingMarker && sender == buttonClear)
                {
                    _listener.SendEraseMarkers(_activeImage as LocatableImage);
                    foreach (Marker m in (_activeImage as LocatableImage).Markers)
                    {
                        canvasImageEditor.Children.Remove(m.Annotation);
                    }
                    (_activeImage as LocatableImage).ClearMarkers();
                    SaveCanvasToActiveImage();
                }
                else
                {
                    foreach (Polyline p in _activeImage.Annotations)
                    {
                        canvasImageEditor.Children.Remove(p);
                    }
                    _activeImage.ClearAnnotations();
                    SaveCanvasToActiveImage();
                }
            }
            else if (_isSelectMultiple)
            {
                var borders = new Border[] {
                    imageThumbBorder, imageThumb1Border, imageThumb2Border, imageThumb3Border, imageThumb4Border
                };

                foreach (var border in borders)
                {
                    border.BorderBrush = Brushes.White;
                }
            }
        }

        private void imageThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var thumbnailBorder = GetBorderFromThumbnailName(((Image)sender).Name);
            if (_isSelectMultiple)
            {
                if (thumbnailBorder.BorderBrush == Brushes.White)
                {
                    thumbnailBorder.BorderBrush = Brushes.Cyan;
                }
                else
                {
                    thumbnailBorder.BorderBrush = Brushes.White;
                }

                if (_selectedImages.Any(x => x == _thumbIndex))
                {
                    _selectedImages.Remove(_thumbIndex);
                }
                else
                {
                    _selectedImages.Add(_thumbIndex);
                }
            }

            SelectThumbnail(0 + _thumbIndex);

            buttonUndo.IsEnabled = !_isSelectMultiple;
        }

        private Border GetBorderFromThumbnailName(string name)
        {
            var character = name[name.Length - 1];
            switch (character)
            {
                case '1':
                    return imageThumb1Border;
                case '2':
                    return imageThumb2Border;
                case '3':
                    return imageThumb3Border;
                case '4':
                    return imageThumb4Border;
                default:
                    return imageThumbBorder;
            }
        }

        private void buttonSendScreenshot_Click(object sender, RoutedEventArgs e)
        {
            if (_activeImage != null)
            {
                if (_placingMarker)
                {
                    _listener.SendArrowLocation((LocatableImage)_activeImage);
                }
                else
                {
                    _listener.SendBitmap(_activeImage.LatestImage);
                }
            }
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
                LoadPDF(pdfFile);
            }
        }

        private void LoadPDF(string pdfFile)
        {
            try
            {
                PDFViewer.PDFViewerDialog pdfDialog = new PDFViewerDialog(pdfFile);
                bool? result = pdfDialog.ShowDialog();
                if (result == true)
                {
                    List<ImageSource> images = new List<ImageSource>();
                    foreach (var image in pdfDialog.selectedImages)
                    {
                        images.Add(image);
                        AddNewImage(new AnnotatedImage(image));
                    }
                }
            }
            catch (TypeInitializationException)
            {
                MessageBoxResult result = MessageBox.Show
                    ("GhostScript must be installed to support this feature.\nWould you like to download it?",
                     "Dependency Missing",
                     MessageBoxButton.YesNo,
                     MessageBoxImage.Error);

                if(result == MessageBoxResult.Yes)
                {
                    result = MessageBox.Show
                    ("NOTE: After installing, you must restart the application", "NOTE",
                     MessageBoxButton.OK,
                     MessageBoxImage.Exclamation);

                    Process.Start("https://github.com/ArtifexSoftware/ghostpdl-downloads/releases/download/gs921/gs921w32.exe");
                }
            }
        }

        private void buttonUploadImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith(".pdf"))
                {
                    LoadPDF(openFileDialog.FileName);
                }
                else
                {
                    Uri imageUri = new Uri(openFileDialog.FileName, UriKind.Relative);
                    ImageSource img = new BitmapImage(imageUri);
                    AddNewImage(new AnnotatedImage(img));
                }
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
            SetPlacingMarkers(!_placingMarker);
        }
        private void buttonShowFlyout_Click(object sender, RoutedEventArgs e)
        {
            TestFlyout.IsOpen = true;
        }

        private void SetPlacingMarkers(bool placingArrow)
        {
            _placingMarker = placingArrow;
            _activeImage.SetAnnotationsVisibility(placingArrow ? Visibility.Hidden : Visibility.Visible);

            if (placingArrow)
            {
                buttonUndo.IsEnabled = (_activeImage as LocatableImage).NumMarkers > 0 &&
                                       !(_activeImage as LocatableImage).GetLastMarker().Sent;
                
                buttonPlaceArrow.Background = Brushes.LightGreen;
            }
            else
            {
                buttonUndo.IsEnabled = true;
                
                buttonPlaceArrow.Background = Brushes.LightGray;
            }
        }

        private void buttonSelectMultiple_Click(object sender, EventArgs e)
        {
            _isSelectMultiple = !_isSelectMultiple;

            buttonUndo.IsEnabled = !_isSelectMultiple;
            buttonChangeColor.IsEnabled = !_isSelectMultiple;
            buttonUploadImage.IsEnabled = !_isSelectMultiple;
            buttonCaptureScreenshot.IsEnabled = !_isSelectMultiple;
            buttonSendScreenshot.IsEnabled = !_isSelectMultiple;
            buttonPlaceArrow.IsEnabled = !_isSelectMultiple;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if ((_thumbIndex + 5) < _imageHistory.Count)
            {
                _thumbIndex++;
                UpdateThumbnails();
            }
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_thumbIndex > 0)
            {
                _thumbIndex--;
                UpdateThumbnails();
            }
        }

        private void clearAllAnnotationsMenu_Click(object sender, RoutedEventArgs e)
        {
            //
            // If it's the active image, remove the markers from the cavas
            // This will happen automatically for others when they are loaded
            //
            if (_activeImage is LocatableImage)
            {
                foreach (Marker m in (_activeImage as LocatableImage).Markers)
                {
                    canvasImageEditor.Children.Remove(m.Annotation);
                }
                SaveCanvasToActiveImage();
            }

            if (_listener != null && _listener.Connected)
            {
                _listener.SendEraseMarkers();
                foreach(LocatableImage image in _imageHistory.OfType<LocatableImage>())
                {
                    image.ClearMarkers();
                }
            }
        }

        #endregion

    }
}
