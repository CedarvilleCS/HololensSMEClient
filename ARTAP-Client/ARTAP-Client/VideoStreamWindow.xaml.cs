using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace ARTAPclient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class VideoStreamWindow : Window
    {
        #region Private Fields

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
    }
}
