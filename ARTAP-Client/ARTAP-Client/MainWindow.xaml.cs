using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using WpfApplication1;

namespace ARTAPclient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// Test Commit! Spencer
    public partial class MainWindow : Window
    {
        private AsynchronousSocketListener _listener;

        private string _ip;

        private string _userName;

        private string _password;

        private string _streamQuality;

        private int _streamQualityIndex;

        private bool _showAnnotations;

        private bool _rememberMe;

        VideoStreamWindow _videoWindow;

        ScreenshotAnnotationsWindow _annotationsWindow;

        public MainWindow()
        {
            InitializeComponent();
            textBoxIP.Text = AppSettings.Default.ipAddress;
            textBoxPort.Text = AppSettings.Default.portNum;
            textBoxUserName.Text = AppSettings.Default.username;
            comboBoxStreamQuality.SelectedIndex = AppSettings.Default.streamQuality;
            checkBoxHolograms.IsChecked = AppSettings.Default.showAnnotations;
            checkBoxRemember.IsChecked = AppSettings.Default.rememberMe;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            buttonConnect.IsEnabled = false;
            buttonConnect.Background = Brushes.Yellow;
            _ip = textBoxIP.Text;
            string port = textBoxPort.Text;
            _userName = textBoxUserName.Text;
            _password = passwordBoxPassword.Password;
            _streamQuality = comboBoxStreamQuality.Text;
            _showAnnotations = (bool)checkBoxHolograms.IsChecked;
            _rememberMe = (bool)checkBoxRemember.IsChecked;

            if (ValidateText(_ip, "IP") && ValidateText(port, "port") &&
                ValidateText(_userName, "user name") && ValidateText(_password, "password"))
            {
                IPEndPoint hostEndPoint;
                if (!TryGetIPEndPoint(_ip, port, out hostEndPoint))
                {
                    return;
                }

                _listener = new AsynchronousSocketListener(hostEndPoint);
                _listener.ConnectionEstablished += Listener_ConnectionEstablished;
                _listener.ConnectionTimedOut += Listener_ConnectionTimedOut;
                _listener.Connect();
            }
            else
            {
                EnableConnectButton();
            }
        }

        private void Listener_ConnectionEstablished(object sender, EventArgs e)
        {
            _listener.ConnectionEstablished -= Listener_ConnectionEstablished;
            _listener.ConnectionTimedOut -= Listener_ConnectionTimedOut;

            ///
            /// Needed for cross-thread window launch
            ///
            this.Dispatcher.BeginInvoke((Action) (() =>
            {
                _videoWindow = new ARTAPclient.VideoStreamWindow(_ip, _userName, _password,
                    _streamQuality, _showAnnotations.ToString());
                _videoWindow.ConnectionFailed += _videoWindow_ConnectionFailed;
                _videoWindow.ConnectionSuccesful += _videoWindow_ConnectionSuccesful;
               
                _annotationsWindow = new ARTAPclient.ScreenshotAnnotationsWindow(_videoWindow, _listener);

                _videoWindow.StartVideo();
                ///
                /// Check to see if Remember Me has been selected
                if ((bool)checkBoxRemember.IsChecked)
                /// 
                {
                    AppSettings.Default.username = textBoxUserName.Text;
                    AppSettings.Default.portNum = textBoxPort.Text;
                    AppSettings.Default.ipAddress = textBoxIP.Text;
                    AppSettings.Default.streamQuality = comboBoxStreamQuality.SelectedIndex;
                    AppSettings.Default.rememberMe = true;
                    AppSettings.Default.showAnnotations = (bool)checkBoxHolograms.IsChecked;
                    AppSettings.Default.Save();
                }
                else
                { 
                    AppSettings.Default.ipAddress = "";
                    AppSettings.Default.username = "";
                    AppSettings.Default.portNum = "";
                    AppSettings.Default.streamQuality = 0;
                    AppSettings.Default.rememberMe = false;
                    AppSettings.Default.showAnnotations = false;
                    AppSettings.Default.Save();
                }
            }));
        }

        private void _videoWindow_ConnectionSuccesful(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(() => 
                    {
                        _videoWindow.Show();
                        _annotationsWindow.Show();
                        this.Hide();
                    }));
        }

        private void _videoWindow_ConnectionFailed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    _videoWindow.Close();
                    _annotationsWindow.Close();

                    MessageBox.Show("Connection to the HoloLens video stream was unsuccesful, " +
                        "please check your connection and login info and try again.", "Connection Error", MessageBoxButton.OK);
                    EnableConnectButton();
                }));
        }

        private void Listener_ConnectionTimedOut(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                EnableConnectButton();
                MessageBox.Show("Connection timed out, please verify IP and Port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        private bool TryGetIPEndPoint(string ip, string port, out IPEndPoint endPoint)
        {
            endPoint = null;
            IPAddress hostAddr;
            if (!IPAddress.TryParse(ip, out hostAddr))
            {
                MessageBox.Show("Please provide a valid IP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            int portNum;
            if (!int.TryParse(port, out portNum))
            {
                MessageBox.Show("Please provide a valid port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            endPoint = new System.Net.IPEndPoint(hostAddr, portNum);
            return true;
        }

        /// <summary>
        /// Checks if string is null or empty, launches window if it is
        /// </summary>
        /// <param name="input">The string to be tested</param>
        /// <param name="name">Name of the field</param>
        /// <returns></returns>
        private bool ValidateText(string input, string name)
        {
            if (String.IsNullOrEmpty(input))
            {
                MessageBox.Show("Please provide input for " + name + ".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Makes the connect button functional again.
        /// </summary>
        private void EnableConnectButton()
        {
            buttonConnect.Background = Brushes.LightGray;
            buttonConnect.IsEnabled = true;
        }
    }
}
