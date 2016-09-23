using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AFRLClientApp
{
  public partial class MainWindow : Form
  {
        Bitmap activeBMP;

        int numPics = 0;

        List<Bitmap> imageHistoryThumbnails = new List<Bitmap>();

        List<PictureBox> pictureBoxThumbnails = new List<PictureBox>();

        private Pen _pen;

        private bool _draw = false;

        private int _x = -1;

        private int _y = -1;

        public MainWindow()
        {
          InitializeComponent();
          _pen = new Pen(Color.Black, 5);
          _pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
          _pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            pictureBoxThumbnails.Add(pictureBoxThumbnail1);
            pictureBoxThumbnails.Add(pictureBoxThumbnail2);
            pictureBoxThumbnails.Add(pictureBoxThumbnail3);
            pictureBoxThumbnails.Add(pictureBoxThumbnail4);
            pictureBoxThumbnails.Add(pictureBoxThumbnail5);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Create a new bitmap.
            Bitmap bmpScreenshot = new Bitmap(640, 360, PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot according to the positioning of the video player.
            gfxScreenshot.CopyFromScreen(axWindowsMediaPlayer1.PointToScreen(new System.Drawing.Point()).X,
                                        axWindowsMediaPlayer1.PointToScreen(new System.Drawing.Point()).Y,
                                        0, 0, axWindowsMediaPlayer1.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            
            // Print out the screenshot to the picture box.
            pictureBox1.Image = bmpScreenshot;
            pictureBox1.Height = bmpScreenshot.Height;
            pictureBox1.Width = bmpScreenshot.Width;

            imageHistoryThumbnails.Insert(0, bmpScreenshot);
            UpdateThumbnails();
            activeBMP = bmpScreenshot;
        }

        private void UpdateThumbnails()
        {
            int numThumbnails = 5;

            if (imageHistoryThumbnails.Count < 5)
            {
                numThumbnails = imageHistoryThumbnails.Count;
            }

            for(int i = 0; i < numThumbnails; i++)
            {
                pictureBoxThumbnails[i].Image = new Bitmap(imageHistoryThumbnails[i], 128, 72);
            }
        }


        Color color = Color.Red;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
           _draw = true;

            Graphics drawPanel = Graphics.FromImage(activeBMP);

            _pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            _pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            drawPanel.DrawRectangle(_pen, e.X, e.Y, 2, 2);
            drawPanel.Save();
            pictureBox1.Image = activeBMP;
 
            _x = e.X;
            _y = e.Y;

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draw)
            {
                Graphics g = Graphics.FromImage(activeBMP);
                SolidBrush brush = new SolidBrush(color);
                g.FillRectangle(brush, e.X, e.Y, 2, 2);
                g.Save();
                pictureBox1.Image = activeBMP;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _draw = false;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = imageHistoryThumbnails[0];
            activeBMP = imageHistoryThumbnails[0];
        }

        private void pictureBoxThumbnail2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = imageHistoryThumbnails[1];
            activeBMP = imageHistoryThumbnails[1];
        }

        private void pictureBoxThumbnail3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = imageHistoryThumbnails[2];
            activeBMP = imageHistoryThumbnails[2];
        }

        private void pictureBoxThumbnail4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = imageHistoryThumbnails[3];
            activeBMP = imageHistoryThumbnails[3];
        }

        private void pictureBoxThumbnail5_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = imageHistoryThumbnails[4];
            activeBMP = imageHistoryThumbnails[4];
        }
    }
}