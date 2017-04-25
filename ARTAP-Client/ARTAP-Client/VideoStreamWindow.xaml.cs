using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;

namespace ARTAPclient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class VideoStreamWindow : Window
    {
        #region Private Fields

        /// <summary>
        /// Aspect ratio of screen
        /// </summary>
        private double _aspectRatio;

        /// <summary>
        /// URL to connect to
        /// </summary>
        private readonly Uri _conURI;

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

        public VideoStreamWindow(string ip, string user, string password, string quality, string annotations)
        {
            InitializeComponent();
            this.SourceInitialized += Window_SourceInitialized;

            string url = String.Format("http://{0}:{1}@{2}/api/holographic/stream/live_{3}.mp4?holo={4}&pv=true&mic=false&loopback=false",
                user, password, ip, quality, annotations.ToLower());
            _conURI = new Uri(url);

            mediaControl.MediaPlayer.VlcLibDirectoryNeeded += OnVlcControlNeedsLibDirectory;
            mediaControl.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            mediaControl.MediaPlayer.EndInit();
        }

        private void OnVlcControlNeedsLibDirectory(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x86\"));
            else
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x64\"));
        }

        #endregion

        #region Public Methods

        public BitmapImage CaptureScreen()
        {
            string tmpBmpPath = Path.Combine(Path.GetTempPath(), "ARTAP", Path.GetRandomFileName());
            tmpBmpPath += ".bmp";
            mediaControl.MediaPlayer.TakeSnapshot(tmpBmpPath);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(tmpBmpPath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            File.Delete(tmpBmpPath);

            return bitmap.Clone();
        }

        public void StartVideo()
        {
            mediaControl.MediaPlayer.Playing += MediaPlayer_Playing;
            mediaControl.MediaPlayer.Play(_conURI);
        }

        private void MediaPlayer_Playing(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {
            ConnectionSuccesful?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Event Handlers

        private void Window_Closed(object sender, EventArgs e)
        {
            if (mediaControl.MediaPlayer.IsPlaying)
            {
                var windows = Application.Current.Windows;
                foreach (var item in windows)
                {
                    (item as Window).Close();
                }
                Application.Current.Shutdown();
            }
        }

        private void MediaPlayer_EncounteredError(object sender, Vlc.DotNet.Core.VlcMediaPlayerEncounteredErrorEventArgs e)
        {
            ConnectionFailed?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Events
    
        /// <summary>
        /// Fires if connection fails (bad login or connection info)
        /// </summary>
        public event EventHandler ConnectionFailed;

        /// <summary>
        /// Fires if connection is successful
        /// </summary>
        public event EventHandler ConnectionSuccesful;

        #endregion

        #region Fixed Aspect Ratio Components

        ///
        /// Code used for fixed aspect ratio borrowed from here:
        /// http://stackoverflow.com/questions/2471867/resize-a-wpf-window-but-maintain-proportions
        ///

        internal enum SWP
        {
            NOMOVE = 0x0002
        }
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
            EXITSIZEMOVE = 0x0232,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static System.Windows.Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new System.Windows.Point(w32Mouse.X, w32Mouse.Y);
        }

        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);

            _aspectRatio = this.Width / this.Height;
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                        if ((pos.flags & (int)SWP.NOMOVE) != 0)
                            return IntPtr.Zero;

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;

                        // determine what dimension is changed by detecting the mouse position relative to the 
                        // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            System.Windows.Point p = GetMousePosition();

                            double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

                            _adjustingHeight = diffHeight > diffWidth;
                        }

                        if (_adjustingHeight.Value)
                            pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

    }
}
