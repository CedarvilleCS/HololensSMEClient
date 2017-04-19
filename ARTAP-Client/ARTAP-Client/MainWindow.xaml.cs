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

        public MainWindow()
        {
            InitializeComponent();
            textBoxIP.Text = AppSettings.Default.ipAddress;
            textBoxPort.Text = AppSettings.Default.portNum;
            textBoxUserName.Text = AppSettings.Default.username;
            comboBoxStreamQuality.SelectedIndex = AppSettings.Default.streamQuality;
            checkBoxAnnotations.IsChecked = AppSettings.Default.showAnnotations;
            checkBoxRemember.IsChecked = AppSettings.Default.rememberMe;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            buttonConnect.IsEnabled = false;
            buttonConnect.Content = "Connecting...";
            _ip = textBoxIP.Text;
            string port = textBoxPort.Text;
            _userName = textBoxUserName.Text;
            _password = passwordBoxPassword.Password;
            _streamQuality = comboBoxStreamQuality.Text;
            _showAnnotations = (bool)checkBoxAnnotations.IsChecked;
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
                VideoStreamWindow video = new ARTAPclient.VideoStreamWindow(_ip, _userName, _password, _streamQuality, _showAnnotations.ToString().ToLower());
                video.Show();

                ScreenshotAnnotationsWindow annotations = new ARTAPclient.ScreenshotAnnotationsWindow(video, _listener);
                annotations.Show();
                this.Close();

                ///
                /// Check to see if Remember Me has been selected
                /// 
                if ((bool)checkBoxRemember.IsChecked)
                {
                    AppSettings.Default.username = textBoxUserName.Text;
                    AppSettings.Default.ipAddress = textBoxIP.Text;
                    AppSettings.Default.portNum = textBoxPort.Text;
                    AppSettings.Default.streamQuality = comboBoxStreamQuality.SelectedIndex;
                    AppSettings.Default.showAnnotations = (bool)checkBoxAnnotations.IsChecked;
                    AppSettings.Default.rememberMe = (bool)checkBoxRemember.IsChecked;
                    AppSettings.Default.Save();
                } else
                {
                    AppSettings.Default.username = "";
                    AppSettings.Default.ipAddress = "";
                    AppSettings.Default.portNum = "";
                    AppSettings.Default.streamQuality = 0;
                    AppSettings.Default.showAnnotations = false;
                    AppSettings.Default.rememberMe = false;
                    AppSettings.Default.Save();
                }

            }));
        }

        private void Listener_ConnectionTimedOut(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                buttonConnect.Content = "Connect";
                buttonConnect.IsEnabled = true;
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
    }
}
