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

        private Direction _markerDirection = Direction.MiddleMiddle;

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
            }
            else
            {
                buttonNext.IsEnabled = false;
            }

            if (_thumbIndex > 0)
            {
                buttonPrev.IsEnabled = true;
            }
            else
            {
                buttonPrev.IsEnabled = false;
            }
        }

        /// <summary>
        /// Render image to canvas
        /// </summary>
        private void SaveCanvasToActiveImage()
        {
            if (_imageHistory.Count != 0)
            {
                Rect bounds = VisualTreeHelper.GetDescendantBounds(canvasImageEditor);
                var rtb =
                    new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                    DPIX, DPIY, PixelFormats.Default);

                var dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    var vb = new VisualBrush(canvasImageEditor);
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
            if (_activeImage is LocatableImage)
            {
                foreach (var m in (_activeImage as LocatableImage).Markers)
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
            if (image.Height < image.Width)
            {
                canvasImageEditor.Width = (360 / image.Height) * image.Width;
                canvasImageEditor.Height = 360;
            }
            else
            {
                canvasImageEditor.Width = 640;
                canvasImageEditor.Height = (640 / image.Width) * image.Height;
            }

            canvasImageEditor.Background = ib;
            canvasImageEditor.InvalidateVisual();
        }

        /// <summary>
        /// Checks if marker placement can be preformed on selected image
        /// and enables/disables corresponding buttons
        /// </summary>
        private void CheckMarkerPlacementAllowed()
        {
            if (_activeImage is LocatableImage)
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

                    canvasImageEditor.Children.Add((_activeImage as LocatableImage).AddMarker(relativeClickPoint, absoluteClickPoint, _markerDirection, _brushColor));
                    _listener.SendArrowLocation((LocatableImage)_activeImage);

                    //se
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
            if (_activeImage != null)
            {
                //
                // If there are unsent markers we can undo
                // 
                var locatableImage = _activeImage as LocatableImage;

                if (locatableImage.NumMarkers > 0 &&
                   !locatableImage.HasUnsentMarkers())
                {
                    canvasImageEditor.Children.Remove(locatableImage.GetLastMarker().Annotation);
                    _listener.EraseOneMarker(locatableImage);

                    //
                    // If there are no more unsent markers disable the undo button
                    //
                    buttonUndo.IsEnabled = locatableImage.NumMarkers > 0;
                }
                
                if (_activeImage.NumAnnotations > 0)
                {
                    canvasImageEditor.Children.Remove(_activeImage.GetLastAnnotation());
                    _activeImage.UndoAnnotation();

                    SaveCanvasToActiveImage();
                }
            }
        }

        //Function implemented but not used, values used elsewhere
        private void buttonDirection_Click(object sender, RoutedEventArgs e)
        {
            var name = ((Button)sender).Name;
            _markerDirection = (Direction)name[name.Length - 1];
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
                    buttonUndo.IsEnabled = false;
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
                _listener.SendBitmap(_activeImage.LatestImage);
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

            if (result == true)
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

                if (result == MessageBoxResult.Yes)
                {
                    //<<<<<<< HEAD
                    //=======
                    result = MessageBox.Show
                    ("NOTE: After installing, you must restart the application", "NOTE",
                     MessageBoxButton.OK,
                     MessageBoxImage.Exclamation);

                    //>>>>>>> PDF-Multiple-Pages
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
            //Toggles the menu upon click
            MenuFlyout.IsOpen = !MenuFlyout.IsOpen;

        }

        private void ChooseMarkerType(object sender, RoutedEventArgs e)
        {

            Button btn = (Button)sender;
            string btnName = btn.Name;
            //Char.GetNumericValue returns a floating point double, casting to int should be fine since we only have whole numbers
            var direction = (int)Char.GetNumericValue(btnName[btnName.Length - 1]);

            //Set the correction marker type
            _markerDirection = (Direction)(direction - 1);

            //Works in theory, need to test
            Image content = (Image)btn.Content;
            var test = content.Source;
            buttonPlaceArrow.Content = (Image)btn.Content;

            //System.Drawing.Bitmap(WpfApplication1.Properties.Resources.filled_circle);
            String correctPhoto = "";

            switch (direction)
            {
                case 1:
                    correctPhoto = "downright";
                    break;
                case 2:
                    correctPhoto = "down";
                    break;
                case 3:
                    correctPhoto = "downleft";
                    break;
                case 4:
                    correctPhoto = "right";
                    break;
                case 5:
                    correctPhoto = "filled_circle";
                    break;
                case 6:
                    correctPhoto = "left";
                    break;
                case 7:
                    correctPhoto = "upright";
                    break;
                case 8:
                    correctPhoto = "up";
                    break;
                case 9:
                    correctPhoto = "upleft";
                    break;
            }
            btn.Content = new Image
            {
                Source = new BitmapImage(new Uri("Resources/" + correctPhoto + ".png", UriKind.Relative)),
            };
            //Maybe always set it to true?
            SetPlacingMarkers(true);

            //Disable all buttons that do not need to be available for this mode
            buttonChangeColor.IsEnabled = false;
            buttonUploadImage.IsEnabled = false;
            buttonCaptureScreenshot.IsEnabled = false;

            //The menu will have to be open to click a button so we don't have to toggle here
            MenuFlyout.IsOpen = false;
        }

        private void SetPlacingMarkers(bool placingArrow)
        {
            _placingMarker = placingArrow;
            _activeImage.SetAnnotationsVisibility(placingArrow ? Visibility.Hidden : Visibility.Visible);

            if (placingArrow)
            {
                buttonUndo.IsEnabled = (_activeImage as LocatableImage).NumMarkers > 0 &&
                                       !(_activeImage as LocatableImage).HasUnsentMarkers();

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
            //If in placingMarker mode get out and reset everything
            if (_placingMarker)
            {
                SetPlacingMarkers(!_placingMarker);
                buttonChangeColor.IsEnabled = !_placingMarker;
                buttonUploadImage.IsEnabled = !_placingMarker;
                buttonCaptureScreenshot.IsEnabled = !_placingMarker;
                //var pathGeo = new PathGeometry();
                //pathGeo.Figures = Geometry.Parse("M2.69,320.439c - 3.768,4.305 - 3.553,10.796,0.494,14.842l1.535,1.536c4.047,4.046,10.537,4.262,14.842,0.493l105.377 - 92.199l - 30.049 - 30.049L2.69,320.439z M339.481,119.739c - 0.359 - 1.118 - 9.269 - 27.873 - 50.31 - 68.912C248.133,9.788,221.377,0.878,220.262,0.52c - 3.879 - 1.244 - 8.127 - 0.217 - 11.008,2.664l - 40.963,40.963c - 4.242,4.243 - 4.242,11.125,0,15.369l4.533,4.534L65.086,147.171c - 2.473,1.909 - 4.006,4.79 - 4.207,7.908c - 0.199,3.118,0.953,6.172,3.162,8.381l41.225,41.226l30.051,30.051l41.225,41.226c2.211,2.209,5.266,3.361,8.381,3.161c3.119 - 0.201,6 - 1.732,7.91 - 4.207l83.119 - 107.738l4.535,4.533c4.239,4.244,11.123,4.244,15.367,0l40.963 - 40.962C339.698,127.866,340.726,123.618,339.481,119.739z M187.751,109.478l - 66.539,56.51c - 4.346,3.691 - 10.75,3.372 - 14.713 - 0.589c - 0.209 - 0.209 - 0.412 - 0.429 - 0.607 - 0.659c - 3.883 - 4.574 - 3.324 - 11.434,1.25 - 15.318l66.537 - 56.509c4.574 - 3.886,11.428 - 3.333,15.318,1.249C192.882,98.735,192.322,105.595,187.751,109.478z");
                //buttonPlaceArrow.Content = pathGeo;

            }
            else
            {
                _isSelectMultiple = !_isSelectMultiple;

                buttonUndo.IsEnabled = !_isSelectMultiple;
                buttonChangeColor.IsEnabled = !_isSelectMultiple;
                buttonUploadImage.IsEnabled = !_isSelectMultiple;
                buttonCaptureScreenshot.IsEnabled = !_isSelectMultiple;
                buttonSendScreenshot.IsEnabled = !_isSelectMultiple;
                buttonPlaceArrow.IsEnabled = !_isSelectMultiple;
            }

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
                foreach (LocatableImage image in _imageHistory.OfType<LocatableImage>())
                {
                    image.ClearMarkers();
                }
            }
        }

        #endregion

    }
}
