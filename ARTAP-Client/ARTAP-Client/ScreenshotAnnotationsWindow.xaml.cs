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
using PDFToImage;
using System.Timers;

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
        private const int PDF_GALLERY_SIZE = 4;

        /// <summary>
        /// Connection to the HoloLens
        /// </summary>
        private AsynchronousSocketListener _listener;

        private PanoramaWindow _panoramaWindow;

        public TaskListUI CurrentTaskList;

        private AnnotatedImage _activeImage;
        private Color _brushColor = Colors.Red;
        private double _brushSize = 5;
        private System.Timers.Timer _checkTaskListTimer;
        private int _currentImageIndex = 0;
        private List<Button> _editButtons;
        private bool _isPano;
        private List<ThumbnailImage> _pictureBoxThumbnails;
        private System.Windows.Shapes.Path _placeArrowPath;
        private int _thumbIndex = 0;
        private bool _isPlacingMarker;
        private Direction _markerDirection = Direction.MiddleMiddle;
        private Style _removeButtonStyle;
        private List<Image> _selectedImages;
        private List<TaskListUI> _taskLists;
        private Style _taskStyle;
        private Style _taskTitleStyle;
        private Style _addImageStyle;
        private Style _afterImageStyle;
        private TaskListUserControl _userControl;
        private VideoStreamWindow _videoStreamWindow;


        /// <summary>
        /// History of images snapped or uploaded
        /// </summary>
        private List<AnnotatedImage> _imageHistory = new List<AnnotatedImage>();

        private ThumbnailImage[] _pdfPages;
        private int _pdfStartingIndex = 0;
        private Timer _headPositionTimer;

        private byte[] _headPositionData;
        #endregion

        #region Constructor

        public ScreenshotAnnotationsWindow(VideoStreamWindow videoStreamWindow, AsynchronousSocketListener listener)
        {
            InitializeComponent();

            _checkTaskListTimer = new Timer(2000);
            _checkTaskListTimer.Elapsed += SendTaskListRequest;

            _editButtons = new List<Button>();
            _imageHistory = new List<AnnotatedImage>();
            _listener = listener;

            _pictureBoxThumbnails = new List<ThumbnailImage>
            {
                new ThumbnailImage(imageThumb0, false),
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

        public ScreenshotAnnotationsWindow(PanoramaWindow panoramaWindow, AsynchronousSocketListener listener)
        {
            InitializeComponent();


            _pictureBoxThumbnails = new List<ThumbnailImage>
            {
                new ThumbnailImage(imageThumb0, false),
                new ThumbnailImage(imageThumb1, false),
                new ThumbnailImage(imageThumb2, false),
                new ThumbnailImage(imageThumb3, false),
                new ThumbnailImage(imageThumb4, false)
            };

            _panoramaWindow = panoramaWindow;
            _listener = listener;
            _isPano = true;

            _removeButtonStyle = FindResource("RoundX") as Style;
            _selectedImages = new List<Image>();
            _taskLists = new List<TaskListUI>();
            _taskStyle = FindResource("Task") as Style;
            _taskTitleStyle = FindResource("Title") as Style;
            _addImageStyle = FindResource("imageButton") as Style;
            _afterImageStyle = FindResource("imageAdded") as Style;
            _userControl = new TaskListUserControl(this);

            _listener.SendIpAddress(_panoramaWindow, _headPositionData);
        }
        #endregion

        #region PrivateMethods

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

        private void CheckMarkerPlacementAllowed()
        {
            if (_activeImage is LocatableImage)
            {
                buttonPlaceArrow.IsEnabled = true;
            }
            else
            {
                SetPlacingMarkers(false);
                buttonPlaceArrow.IsEnabled = false;
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
                    thumbnailBorder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF5B5B5B"));
                }
            }
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

            var task = CurrentTaskList.TaskList.Tasks.Find(x => x.Id == uiTask.TaskId);
            if (!task.IsNew) nameBox.Text = task.Name;
            else nameBox.SetValue(TextBoxHelper.WatermarkProperty, task.Name);

            if (task.Attachment != null)
            {
                Image content = new Image();
                Image toolTip = new Image();
                content.Source = task.Attachment;
                toolTip.Source = task.Attachment;

                addImage.Content = content;
                ToolTip tt = new ToolTip
                {
                    Content = toolTip,
                    Width = 500,
                    Height = 500,
                };
                addImage.ToolTip = tt;
            }

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

        public void removeTask_Click(object sender, RoutedEventArgs e)
        {
            var button = ((Button)sender);
            var taskUI = CurrentTaskList.TaskUIs.Find(x => x.Id.ToString() == button.Tag.ToString());
            var task = CurrentTaskList.TaskList.Tasks.Find(x => x.Id == taskUI.TaskId);

            CurrentTaskList.RemoveTaskUI(taskUI, _userControl.IndividualTasks);
            CurrentTaskList.ReorderTasks();
            CurrentTaskList.SetTaskUIMargins(60);
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

                var taskUI = CurrentTaskList.TaskUIs.Find(x => x.Id.ToString() == btn.Tag.ToString());
                var task = CurrentTaskList.TaskList.Tasks.Find(x => x.Id == taskUI.TaskId);
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
            var task = CurrentTaskList.TaskList.Tasks.Find(x => x.Id == taskUI.TaskId);

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
                    if ((int)button.Tag == CurrentTaskList.TaskList.Id)
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
            var task = CurrentTaskList.TaskList.Tasks.Find(x => x.Id == taskUI.TaskId);

            task.Name = box.Text;
            task.IsNew = false;
        }

        #endregion

        #region EventHandlers

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
        }

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

        private void list_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var buttons = taskListButtons.Children;

            // crashing here right now
            var taskList = _taskLists.Find(x => x.TaskList.Id == (int)button.Tag);
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

        private void _listener_ConnectionClosed(object sender, EventArgs e)
        {
            MessageBox.Show("Connection to HoloLens lost.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            openFileDialog.Filter = "Image Files (*.BMP;*.JPG;*.GIF; *.JPEG; *.PNG)|*.BMP;*.JPG;*.GIF; *.JPEG; *.PNG";
            if (openFileDialog.ShowDialog() == true)
            {
                Uri imageUri = new Uri(openFileDialog.FileName, UriKind.Relative);
                ImageSource img = new BitmapImage(imageUri);
                AddNewImage(new AnnotatedImage(img));
            }

        }

        private void canvasImageEditor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_activeImage != null)
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

        private void buttonCaptureScreenshot_Click(object sender, RoutedEventArgs e)
        {
            _placeArrowPath = (System.Windows.Shapes.Path)buttonPlaceArrow.Content;
            ImageSource screenshot;
            if (_isPano) screenshot = _panoramaWindow.panoImage.Source;
            else screenshot = _videoStreamWindow.CaptureScreen();
            LocatableImage img = new LocatableImage(screenshot);
            AddNewImage(img);
            _listener.RequestLocationID(img);
        }

        private void clearAllAnnotationsMenu_Click(object sender, RoutedEventArgs e)
        {
            ImageSource screenshot = _videoStreamWindow.CaptureScreen();
            LocatableImage img = new LocatableImage(screenshot);
            AddNewImage(img);
            _listener.RequestLocationID(img);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeImage != null)
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
        }

        private void imageThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var thumbName = ((Image)sender).Name;
            var thumbNailNum = (int)Char.GetNumericValue(thumbName[thumbName.Length - 1]);

            SelectThumbnail(thumbNailNum + _thumbIndex);

            UpdateThumbnailBorders();
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

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeImage != null)
            {
                var locatableImage = _activeImage as LocatableImage;

                if (locatableImage.NumMarkers > 0)
                {
                    canvasImageEditor.Children.Remove(locatableImage.GetLastMarker().Annotation);
                    locatableImage.UndoMarker();
                    _listener.EraseOneMarker(locatableImage);
                    
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

        private void buttonPrevPDF_Click(object sender, RoutedEventArgs e)
        {
            var images = new Image[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };

            int imageNum = 0;
            _pdfStartingIndex -= PDF_GALLERY_SIZE;
            for (int i = _pdfStartingIndex; i < _pdfStartingIndex + PDF_GALLERY_SIZE; i++)
            {
                images[imageNum].Source = _pdfPages[i].Image.Source;
                imageNum++;
            }

            pdfToCanvas(_pdfPages[_pdfStartingIndex].Image, pdfViewer);


            if (_pdfStartingIndex == 0)
            {
                buttonPrevPDF.IsEnabled = false;
            }
            if (_pdfStartingIndex + 4 < _pdfPages.Length)
            {
                buttonNextPDF.IsEnabled = true;
            }
            UpdatePDFBorders();
        }

        private void buttonNextPDF_Click(object sender, RoutedEventArgs e)
        {
            if (_pdfStartingIndex + PDF_GALLERY_SIZE < _pdfPages.Length)
            {
                var images = new Image[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };
                //var images = new Canvas[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };
                _pdfStartingIndex += PDF_GALLERY_SIZE;
                int imageNum = 0;
                //Hot fix, clear all images before
                ClearPDFImages();
                for (int i = _pdfStartingIndex; i < _pdfStartingIndex + PDF_GALLERY_SIZE && i < _pdfPages.Length; i++)
                {
                    //pdfToCanvas(_pdfPages[i], images[imageNum]);
                    images[imageNum].Source = _pdfPages[i].Image.Source;
                    imageNum++;
                }
                pdfToCanvas(_pdfPages[_pdfStartingIndex].Image, pdfViewer);

                if (_pdfStartingIndex > 0)
                {
                    buttonPrevPDF.IsEnabled = true;
                }
                if (_pdfStartingIndex + 4 > _pdfPages.Length)
                {
                    buttonNextPDF.IsEnabled = false;
                }

            }
            UpdatePDFBorders();
        }

        private void ClearPDFImages()
        {
            var images = new Image[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };
            //var images = new Canvas[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };
            for (int i = 0; i < images.Length; i++)
            {
                images[i].Source = null;
            }
        }

        private void buttonLoadPDF_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "PDF Files (*.PDF)|*.PDF;";

            if (openFileDialog.ShowDialog() == true)
            {
                ResetPDFGallery();
                _pdfStartingIndex = 0;
                int numPDFPages = PDFManager.getNumPages(openFileDialog.FileName);
                _pdfPages = new ThumbnailImage[numPDFPages];
                //Get the first page
                System.Drawing.Image image = PDFManager.getImage(openFileDialog.FileName, 1);

                BitmapImage bmi = convertDrawiningImageToBitmap(image);
                Image img = new Image();
                img.Source = bmi;

                pdfToCanvas(img, pdfViewer);
                Button btn = (Button)sender;
                string btnName = btn.Name;

                //Char.GetNumericValue returns a floating point double, casting to int should be fine since we only have whole numbers
                var direction = (int)Char.GetNumericValue(btnName[btnName.Length - 1]);

                var images = new Image[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };
                //var images = new Canvas[] { pdfThumb0, pdfThumb1, pdfThumb2, pdfThumb3 };
                //pdfToCanvas(bmi.Clone(), images[0]);
                images[0].Source = bmi.Clone();
                _pdfPages[0] = new ThumbnailImage(img, true, true);

                //Set the first 4 border/images now
                //only have 4 display images and don't want to loop if we have less than 4 pages
                //int maxValue = System.Math.Min(4, numPDFPages);
                for (int i = 1; i < PDF_GALLERY_SIZE && i < numPDFPages; i++)
                {
                    BitmapImage temp = convertDrawiningImageToBitmap(PDFManager.getImage(openFileDialog.FileName, i + 1));
                    //pdfToCanvas(temp, images[i]);
                    images[i].Source = temp;
                    Image firstFour = new Image();
                    firstFour.Source = temp;
                    _pdfPages[i] = new ThumbnailImage(firstFour, true, true);

                }

                //Get the rest of the pages if there are any
                for (int i = PDF_GALLERY_SIZE; i < numPDFPages; i++)
                {
                    BitmapImage temp = convertDrawiningImageToBitmap(PDFManager.getImage(openFileDialog.FileName, i + 1));
                    Image rest = new Image();
                    rest.Source = temp;
                    _pdfPages[i] = new ThumbnailImage(rest, true, true);
                }

                if (numPDFPages > PDF_GALLERY_SIZE)
                {
                    buttonNextPDF.IsEnabled = true;
                }

                buttonPDFSelectAll.IsEnabled = true;
                sendPDF.IsEnabled = true;
                UpdatePDFBorders();

            }
        }

        private void sendPDF_Click(object sender, RoutedEventArgs e)
        {
            var document = new PDFDocument();
            for (int i = 0; i < _pdfPages.Length; i++)
            {
                if (_pdfPages[i].IsSelected)
                {
                    byte[] bytes;
                    var encoder = new PngBitmapEncoder();

                    if (_pdfPages[i].Image.Source is BitmapSource bitmapSource)
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

            }

            _listener.SendPDF(document);

            for (int i = 0; i < _pdfPages.Length; i++)
            {
                _pdfPages[i].IsSelected = false;
                UpdatePDFBorders();
            }
        }

        private void pdfThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var thumbName = ((Image)sender).Name;
            var thumbNailNum = (int)Char.GetNumericValue(thumbName[thumbName.Length - 1]);
            var curPDF = _pdfPages[_pdfStartingIndex + thumbNailNum];
            curPDF.IsSelected = !(curPDF.IsSelected);

            pdfToCanvas(curPDF.Image, pdfViewer);
            UpdatePDFBorders();
            //UpdateThumbnailBorders();
        }

        private void ResetPDFGallery()
        {
            ClearPDFImages();
            buttonNextPDF.IsEnabled = false;
            buttonPrevPDF.IsEnabled = false;
            pdfViewer.Background = null;


        }

        private void pdfToCanvas(Image img, Canvas can)
        {
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.Uniform;
            ib.ImageSource = img.Source;
            can.Background = ib;
        }

        private void buttonPDFSelectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _pdfPages.Length; i++)
            {
                _pdfPages[i].IsSelected = !_pdfPages[i].IsSelected;
                UpdatePDFBorders();
            }
        }

        private void UpdatePDFBorders()
        {
            var borders = new Border[] {
                pdfThumbBorder0, pdfThumbBorder1, pdfThumbBorder2, pdfThumbBorder3
            };

            for (var i = 0; i < PDF_GALLERY_SIZE; i++)
            {
                var thumbnailBorder = borders[i];
                if (i + _pdfStartingIndex < _pdfPages.Length && _pdfPages[i + _pdfStartingIndex].IsSelected)
                {
                    //thumbnailBorder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00ccff"));
                    thumbnailBorder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#53c653"));
                    thumbnailBorder.BorderThickness = new Thickness(2.5);
                    //thumbnailBorder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0099ff"));
                    //Brushes.Cyan; #53c653 #0099ff
                }
                else
                {
                    thumbnailBorder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF5B5B5B"));
                    thumbnailBorder.BorderThickness = new Thickness(1.0);
                    //thumbnailBorder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFE8E8E8"));
                }
            }
        }

        private BitmapImage convertDrawiningImageToBitmap(System.Drawing.Image im)
        {
            BitmapImage bmi;

            var ms = new MemoryStream();

            im.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.StreamSource = ms;
            bmi.EndInit();

            return bmi;
        }
    }
}

#endregion 