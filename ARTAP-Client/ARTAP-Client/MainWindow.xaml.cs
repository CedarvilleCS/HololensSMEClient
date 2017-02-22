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

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            //
            // If we are in a DEBUG build show the other 
            // windows without connecting to a HoloLens.
            //
            ScreenshotAnnotationsWindow annotations = new ARTAPclient.ScreenshotAnnotationsWindow(null,null);
            annotations.Show();
#endif
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            buttonConnect.IsEnabled = false;
            buttonConnect.Content = "Connecting...";
            _ip = textBoxIP.Text;
            string port = textBoxPort.Text;
            _userName = textBoxUserName.Text;
            _password = passwordBoxPassword.Password;

            if (ValidateText(_ip, "IP") && ValidateText(port, "port") &&
                ValidateText(_userName, "user name") && ValidateText(_password, "password"))
            {
                IPEndPoint hostEndPoint;
                if(!TryGetIPEndPoint(_ip, port, out hostEndPoint))
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
                VideoStreamWindow video = new ARTAPclient.VideoStreamWindow(_ip, _userName, _password);
                video.Show();

                ScreenshotAnnotationsWindow annotations = new ARTAPclient.ScreenshotAnnotationsWindow(video, _listener);
                annotations.Show();
                this.Close();
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
