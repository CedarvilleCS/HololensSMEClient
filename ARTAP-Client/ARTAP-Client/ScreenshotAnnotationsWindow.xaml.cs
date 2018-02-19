using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApplication1;
using PDFViewer;
using System.Diagnostics;
using System.IO;
using MahApps.Metro.Controls;

namespace ARTAPclient
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    ///
    public partial class ScreenshotAnnotationsWindow : MahApps.Metro.Controls.MetroWindow
    {
        #region Fields

        private const int DPIX = 96;
        private const int DPIY = 96;
        private const int MAX_IMAGE_HISTORY_SIZE = 50;
        private const int THUMBNAIL_GALLERY_SIZE = 5;

        public TaskListUI CurrentTaskList;

        private AnnotatedImage _activeImage;
        private System.Timers.Timer _checkTaskListTimer;
        private List<Button> _editButtons;
        private List<AnnotatedImage> _imageHistory;
        private AsynchronousSocketListener _listener;
        private List<ThumbnailImage> _pictureBoxThumbnails;
        private System.Windows.Shapes.Path _placeArrowPath;
        private Style _removeButtonStyle;
        private List<Image> _selectedImages;
        private List<TaskListUI> _taskLists;
        private Style _taskStyle;
        private Style _taskTitleStyle;
        private Style _addImageStyle;
        private Style _afterImageStyle;
        private TaskListUserControl _userControl;
        private VideoStreamWindow _videoStreamWindow;

        private Color _brushColor = Colors.Red;
        private double _brushSize = 5;
        private int _currentImageIndex = 0;
        private bool _isPlacingMarker;
        private bool _isSelectMultiple = false;
        private Direction _markerDirection = Direction.MiddleMiddle;
        private string _oldText = "";
        private int _thumbIndex = 0;
        #endregion

        #region Constructor

        public ScreenshotAnnotationsWindow(VideoStreamWindow videoStreamWindow, AsynchronousSocketListener listener)
        {
            InitializeComponent();


            _checkTaskListTimer = new System.Timers.Timer(2000);
            _checkTaskListTimer.Elapsed += SendTaskListRequest;

            _editButtons = new List<Button>();
            _imageHistory = new List<AnnotatedImage>();
            _listener = listener;

            _pictureBoxThumbnails = new List<ThumbnailImage>
            {
                new ThumbnailImage(imageThumb, false),
                new ThumbnailImage(imageThumb1, false),
                new ThumbnailImage(imageThumb2, false),
                new ThumbnailImage(imageThumb3, false),
                new ThumbnailImage(imageThumb4, false)
            };

            _removeButtonStyle = FindResource("RoundX") as Style;
            _selectedImages = new List<Image>();
            _taskLists = new List<TaskListUI>();
            _taskStyle = FindResource("Task") as Style;
            _taskTitleStyle = FindResource("Title") as Style;
            _addImageStyle = FindResource("imageButton") as Style;
            _afterImageStyle = FindResource("imageAdded") as Style;
            _userControl = new TaskListUserControl(this);
            _videoStreamWindow = videoStreamWindow;
        }
        #endregion

        #region PrivateMethods

        private void AddAllTaskUIs(List<TaskUI> uiTasks)
        {
            foreach (var uiTask in uiTasks)
            {
                AddUITask(uiTask);
            }
        }

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
            UpdateThumbnailBorders();
        }

        public void AddNewTask(object sender, RoutedEventArgs e)
        {
            var order = CurrentTaskList.TaskList.Tasks.Count;
            var task = new Task(order);
            CurrentTaskList.TaskList.Tasks.Add(task);

            var uiTask = new TaskUI(task, 60 + (30 * order), _removeButtonStyle, _taskStyle, _addImageStyle, order);
            CurrentTaskList.TaskUIs.Add(uiTask);
            AddUITask(uiTask);
        }

        private void AddTaskListName(TaskListUI taskList)
        {
            var nameBox = taskList.NameTextBox;
            taskList.NameTextBox.TextChanged += UpdateTaskListName;
            _userControl.IndividualTasks.Children.Add(nameBox);
        }

        private void AddUITask(TaskUI uiTask)
        {
            var checkbox = uiTask.IsCompletedUI;
            var remove = uiTask.Remove;
            var nameBox = uiTask.NameUI;
            var addImage = uiTask.AddImage;

            if (!uiTask.Task.IsNew) nameBox.Text = uiTask.Task.Name;
            else nameBox.SetValue(TextBoxHelper.WatermarkProperty, uiTask.Task.Name);

            //Make it so images are set

            remove.Click += removeTask_Click;
            uiTask.NameUI.TextChanged += UpdateTaskName;
            checkbox.Checked += UpdateTaskCompletion;
            checkbox.Unchecked += UpdateTaskCompletion;
            addImage.Click += AddImageClick;

            _userControl.IndividualTasks.Children.Add(remove);
            _userControl.IndividualTasks.Children.Add(nameBox);
            _userControl.IndividualTasks.Children.Add(checkbox);
            _userControl.IndividualTasks.Children.Add(addImage);
        }

        private void AddImageClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.BMP;*.JPG;*.GIF; *.JPEG; *.PNG)|*.BMP;*.JPG;*.GIF; *.JPEG; *.PNG";

            if (openFileDialog.ShowDialog() == true)
            {
                var btn = (Button)sender;
                var img = new Image();
                var toolTipImg = new Image();

                var bitmap = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Absolute));
                img.Source = bitmap;
                toolTipImg.Source = bitmap;

                //Think we have a problem with ID's so not sure this is the right one to find stuff
                var taskUI = CurrentTaskList.TaskUIs.Find(x => x.Id.ToString() == btn.Tag.ToString());
                var task = taskUI.Task;

                //Ask if should do actual size or scaling height or flat (currently what I am doing)
                //scaling
                ToolTip tt = new ToolTip
                {
                    Content = toolTipImg,
                    Width = 500,
                    Height = 500,
                };

                task.Attachment = bitmap;

                btn.ToolTip = tt;
                btn.Content = img;
                //CurrentTaskList.RecreateUIElements(_taskTitleStyle);
            }
        }

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

        private void ChooseMarkerType(object sender, RoutedEventArgs e)
        {

            Button btn = (Button)sender;
            string btnName = btn.Name;

            var direction = (int)Char.GetNumericValue(btnName[btnName.Length - 1]);

            _markerDirection = (Direction)(direction - 1);

            Image content = (Image)btn.Content;
            var test = content.Source;
            buttonPlaceArrow.Content = (Image)btn.Content;

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

        private void DrawImageToCanvas(ImageSource image)
        {
            var ib = new ImageBrush();

            try
            {
                ib.Stretch = Stretch.Uniform;
                ib.ImageSource = image.Clone();
            }
            catch (NotSupportedException)
            {
                MessageBox.Show("The selected file must be an image.", "Not an image file", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        private int GetIndexFromThumbnailName(string name)
        {
            var character = name[name.Length - 1];
            switch (character)
            {
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                default:
                    return 0;
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
                        AddNewImage(new AnnotatedImage(image, true));
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
                    result = MessageBox.Show
                    ("NOTE: After installing, you must restart the application", "NOTE",
                     MessageBoxButton.OK,
                     MessageBoxImage.Exclamation);

                    Process.Start("https://github.com/ArtifexSoftware/ghostpdl-downloads/releases/download/gs921/gs921w32.exe");
                }
            }
        }

        private void ReceiveTaskListCallback(IAsyncResult ar)
        {
            var notification = new TaskListNotification((byte[])ar.AsyncState);
            _listener.ClientEndReceive(ar);
        }

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
                UpdateThumbnailBorders();
            }
        }

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

        public void SendTaskList()
        {
            _listener.SendTaskList(CurrentTaskList.TaskList);
        }

        private void SendTaskListRequest(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var state = new StateObject();
                _listener.ClientReceive(state.buffer, 0, 10, 0,
                    new AsyncCallback(ReceiveTaskListCallback), state);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred receiving the Task List from the HoloLens.",
                    "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetPlacingMarkers(bool placingArrow)
        {
            _isPlacingMarker = placingArrow;
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

        public void UpdateTaskCompletion(object sender, RoutedEventArgs e)
        {
            var box = (CheckBox)sender;
            var taskUI = CurrentTaskList.TaskUIs.Find(x => x.Id.ToString() == box.Tag.ToString());
            var task = taskUI.Task;

            task.IsCompleted = (bool)(box.IsChecked);
            taskUI.NameUI.IsEnabled = !task.IsCompleted;
            taskUI.AddImage.IsEnabled = !task.IsCompleted;
        }

        public void UpdateTaskListName(object sender, RoutedEventArgs e)
        {
            var stackPanel = (StackPanel)(TaskListGrid.Children[0]);
            var box = (TextBox)sender;

            foreach (var panelChild in stackPanel.Children)
            {
                if (panelChild is Button button)
                {
                    if (button.Tag.ToString() == CurrentTaskList.TaskList.Name)
                    {
                        CurrentTaskList.TaskList.Name = box.Text;
                        button.Content = box.Text;
                    }
                }
            }
        }

        public void UpdateTaskName(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;

            var taskUI = CurrentTaskList.TaskUIs.Find(x => x.Id.ToString() == box.Tag.ToString());
            var task = taskUI.Task;

            task.Name = box.Text;
            task.IsNew = false;

            taskUI.Task.IsNew = false;
            taskUI.Task.Name = box.Text;
        }

        private void UpdateThumbnailBorders()
        {
            var borders = new Border[] {
                imageThumbBorder, imageThumb1Border, imageThumb2Border, imageThumb3Border, imageThumb4Border
            };

            for (var i = 0; i < THUMBNAIL_GALLERY_SIZE; i++)
            {
                var thumbnailBorder = borders[i];
                if (_pictureBoxThumbnails[i].IsSelected)
                {
                    thumbnailBorder.BorderBrush = Brushes.Cyan;
                }
                else
                {
                    thumbnailBorder.BorderBrush = Brushes.White;
                }
            }
        }

        private void UpdateThumbnails()
        {
            int numActiveThumbnails = THUMBNAIL_GALLERY_SIZE;

            //Only use the number of active images
            if (_imageHistory.Count < THUMBNAIL_GALLERY_SIZE)
            {
                numActiveThumbnails = _imageHistory.Count;
            }

            int index = _thumbIndex;
            //Loop through and update images for all of the thumbnail frames
            for (int i = 0; i < numActiveThumbnails; i++, index++)
            {
                _pictureBoxThumbnails[i].Image.Source = _imageHistory[index].LatestImage;
                _pictureBoxThumbnails[i].IsPdf = _imageHistory[index].IsPdf;
                _pictureBoxThumbnails[i].IsSelected = _imageHistory[index].IsSelected;
            }

            if ((_thumbIndex + THUMBNAIL_GALLERY_SIZE) < _imageHistory.Count)
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

        #endregion

        #region EventHandlers
        private void buttonAddList_Click(object sender, RoutedEventArgs e)
        {
            var count = _taskLists.Count;

            if (count < 14)
            {
                var taskListUI = new TaskListUI(new TaskList(count), _removeButtonStyle, _taskTitleStyle, _taskStyle, _addImageStyle);
                var button = taskListUI.Button;
                _taskLists.Add(taskListUI);

                button.Click += list_Click;

                taskListButtons.Children.Add(button);
                list_Click(button, null);
            }
        }

        private void buttonCaptureScreenshot_Click(object sender, RoutedEventArgs e)
        {
            _placeArrowPath = (System.Windows.Shapes.Path)buttonPlaceArrow.Content;
            ImageSource screenshot = _videoStreamWindow.CaptureScreen();
            LocatableImage img = new LocatableImage(screenshot);
            AddNewImage(img);
            _listener.RequestLocationID(img);
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

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if ((_thumbIndex + THUMBNAIL_GALLERY_SIZE) < _imageHistory.Count)
            {
                _thumbIndex++;

                UpdateThumbnails();
                UpdateThumbnailBorders();
            }
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_thumbIndex > 0)
            {
                _thumbIndex--;
                UpdateThumbnails();
                UpdateThumbnailBorders();
            }
        }

        private void buttonRemoveList_Click(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBox.Show("Are you sure you want to delete this TaskList?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (dialogResult == MessageBoxResult.Yes)
            {
                var button = (Button)sender;
                var index = _taskLists.FindIndex(x => x == CurrentTaskList);

                _taskLists.RemoveAt(index);

                var buttons = taskListButtons.Children;
                buttons.RemoveAt(index);

                if (buttons.Count > 0)
                {
                    list_Click(buttons[buttons.Count - 1], null);
                }
                else
                {
                    buttonAddList_Click(null, null);
                }
            }
        }

        private void buttonSelectMultiple_Click(object sender, EventArgs e)
        {
            if (_isPlacingMarker)
            {
                SetPlacingMarkers(!_isPlacingMarker);
                buttonChangeColor.IsEnabled = !_isPlacingMarker;
                buttonUploadImage.IsEnabled = !_isPlacingMarker;
                buttonCaptureScreenshot.IsEnabled = !_isPlacingMarker;
                buttonPlaceArrow.Content = _placeArrowPath;
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

        private void buttonSendScreenshot_Click(object sender, RoutedEventArgs e)
        {
            if (_activeImage != null)
            {
                _listener.SendBitmap(_activeImage.LatestImage);
                if (_selectedImages.Any())
                {
                    var document = new PDFDocument();
                    foreach (var image in _selectedImages)
                    {
                        byte[] bytes;
                        var encoder = new PngBitmapEncoder();

                        if (image.Source is BitmapSource bitmapSource)
                        {
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                            using (var stream = new MemoryStream())
                            {
                                encoder.Save(stream);
                                bytes = stream.ToArray();
                            }

                            document.Pages.Add(bytes);
                        }
                    }

                    _listener.SendPDF(document);
                }
                else if (_isPlacingMarker)
                {
                    if (((LocatableImage)_activeImage).PositionID != null)
                    {
                        _listener.SendArrowLocation((LocatableImage)_activeImage);
                    }
                    else
                    {
                        MessageBox.Show("Waiting for Location ID from Image", "Error", MessageBoxButton.OK);
                    }
                }
                else
                {
                    _listener.SendBitmap(_activeImage.LatestImage);
                }
            }
        }

        private void buttonShowFlyout_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout.IsOpen = !MenuFlyout.IsOpen;
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

        private void canvasImageEditor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_activeImage != null && !_isSelectMultiple)
            {
                Debug.WriteLine("Placing Arrow: " + _isPlacingMarker);
                if (_isPlacingMarker)
                {
                    //
                    // Get the pixel value in the original image
                    //
                    Point relativeClickPoint = e.GetPosition((Canvas)sender);
                    int x = (int)((_activeImage.OriginalImage.Width / canvasImageEditor.Width) * relativeClickPoint.X);
                    int y = (int)((_activeImage.OriginalImage.Height / canvasImageEditor.Height) * relativeClickPoint.Y);

                    Point absoluteClickPoint = new Point(x, y);

                    var locatableImage = _activeImage as LocatableImage;
                    canvasImageEditor.Children.Add(locatableImage.AddMarker(relativeClickPoint, absoluteClickPoint, _markerDirection, _brushColor));

                    _listener.SendArrowLocation(locatableImage);

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
                !_isPlacingMarker)
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
                _listener.EraseAllMarkers();
                foreach (LocatableImage image in _imageHistory.OfType<LocatableImage>())
                {
                    image.ClearMarkers();
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
                if (_isPlacingMarker && sender == buttonClear)
                {
                    _listener.EraseMarkersOnImage(_activeImage as LocatableImage);
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
            if (_isSelectMultiple)
            {
                var index = GetIndexFromThumbnailName(((Image)sender).Name);
                var thumbnail = _pictureBoxThumbnails[index];
                buttonUndo.IsEnabled = false;

                if (!thumbnail.IsSelected && thumbnail.IsPdf)
                {
                    _selectedImages.Add(thumbnail.Image);
                    _imageHistory[_thumbIndex + index].IsSelected = true;
                    thumbnail.IsSelected = true;
                }
                else if (thumbnail.IsSelected && thumbnail.IsPdf)
                {
                    _selectedImages.Remove(thumbnail.Image);
                    _imageHistory[_thumbIndex + index].IsSelected = false;
                    thumbnail.IsSelected = false;
                }
            }

            SelectThumbnail(0 + _thumbIndex);

            UpdateThumbnailBorders();
        }

        private void list_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var buttons = taskListButtons.Children;

            // crashing here right now
            var taskList = _taskLists.Find(x => x.NameTextBox.Tag.ToString() == button.Tag.ToString());
            if (CurrentTaskList != taskList)
            {
                _userControl = new TaskListUserControl(this);
                CurrentTaskList = taskList;
                CurrentTaskList.RecreateUIElements(_taskTitleStyle);

                AddTaskListName(taskList);
                AddAllTaskUIs(taskList.TaskUIs);

                TaskListGrid.Children.Add(_userControl);

                Grid.SetColumn(_userControl, 1);
                Grid.SetRow(_userControl, 0);
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


        public void removeTask_Click(object sender, RoutedEventArgs e)
        {
            var button = ((Button)sender);
            var taskUI = CurrentTaskList.TaskUIs.Find(x => x.Id.ToString() == button.Tag.ToString());
            var task = taskUI.Task;

            CurrentTaskList.RemoveTaskUI(taskUI, _userControl.IndividualTasks);
            CurrentTaskList.ReorderTasks();
            CurrentTaskList.SetTaskUIMargins(60);
            //CurrentTaskList.RecreateUIElements(_taskTitleStyle);
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

                if (locatableImage.NumMarkers > 0)
                {
                    canvasImageEditor.Children.Remove(locatableImage.GetLastMarker().Annotation);
                    locatableImage.UndoMarker();
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

        #endregion
    }
}
