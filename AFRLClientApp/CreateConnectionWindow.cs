using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AFRLClientApp
{
    /// <summary>
    /// Dialog box for requesting a hostname and port from a user.
    /// Data is valid when isCanceled is false.
    /// Port is stored as an integer in Port.
    /// Hostname is stored as a string in Hostname.
    /// </summary>
    public partial class CreateConnectionWindow : Form
    {
        public CreateConnectionWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Port input by user
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Hostname input by user
        /// </summary>
        public string Hostname { get; private set;  }
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
            Hostname = textBoxHostname.Text;
            Port = Int32.Parse(textBoxPort.Text);
            isCanceled = false;
            Close();
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
