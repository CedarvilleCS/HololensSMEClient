using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AFRLClientApp
{
    /// <summary>
    /// Dialog box for requesting a hostname and port from a user.
    /// Data is valid when isCanceled is false.
    /// ServerEndpoint is the endpoint to connect to.
    /// </summary>
    public partial class CreateConnectionWindow : Form
    {
        public CreateConnectionWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Endpoint for the connection to the server (HoloLens)
        /// </summary>
        public IPEndPoint ServerEndpoint { get; private set; }
        
        /// <summary>
        /// False when the cancel button was not clicked and data is valid
        /// If this is true ignore any other values in this class.
        /// </summary>
        public bool isCanceled { get; private set; }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            //
            // Connect button was clicked so extract the hostname
            // and port from the text boxes in the dialog and
            // set isCanceled to false, then close the
            // dialog box.
            //

            IPAddress ip;
            int port;

            bool goodIP = IPAddress.TryParse(textBoxHostname.Text, out ip);
            bool goodPort = int.TryParse(textBoxPort.Text, out port);

            if(goodIP && goodPort)
            {
                ServerEndpoint = new IPEndPoint(ip, port);
                isCanceled = false;
                Close();
            }
            else
            {
                MessageBox.Show("Error parsing the provided connection info," +
                    "please veriy and try again.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }      
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //
            // Cancel button was clicked so set isCanceled to
            // true to indicate the data is invalid and
            // close the dialog box
            //
            isCanceled = true;
            Close();
        }
    }
}
