using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AFRLClientApp
{
  public partial class MainWindow : Form
  {

        #region Fields

        /// <summary>
        /// Current bitmap active for drawing
        /// </summary>
        Bitmap _activeBMP;

        /// <summary>
        /// History of images snapped from the stream
        /// </summary>
        List<Bitmap> _imageHistory = new List<Bitmap>();

        /// <summary>
        /// List containing all of the thumbnail pictureboxes to make updating easy
        /// </summary>
        List<PictureBox> _pictureBoxThumbnails = new List<PictureBox>();

        /// <summary>
        /// Pen for drawing annotations on the images captured
        /// </summary>
        private Pen _pen;

        /// <summary>
        /// Are we currently drawing?
        /// </summary>
        private bool _draw = false;

        /// <summary>
        /// Color of the pen being drawn
        /// </summary>
        Color _color = Color.Red;

        /// <summary>
        /// Size of the pen being drawn
        /// </summary>
        private float _brushSize;

        /// <summary>
        /// Previous x point for the line being drawn
        /// </summary>
        private int _x;

        /// <summary>
        /// Previous y point for the line being drawn
        /// </summary>
        private int _y;

        /// <summary>
        /// Number of thumbnail images
        /// </summary>
        private const int NUMTHUMBNAILS = 5;

        /// <summary>
        /// Number of thumbnail images
        /// </summary>
        private bool _screenshotPresent = false;

        /// <summary>
        /// Allows communication over the network
        /// </summary>
        //private NetworkManager networkManager;

        #endregion

        #region Constructor

        public MainWindow()
        {
          InitializeComponent();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the thumbnails with the latest images captured
        /// </summary>
        private void UpdateThumbnails()
        {
            //Number of thumbnails presumed to be set
            int numActiveThumbnails = NUMTHUMBNAILS;

            //If there aren't enough history images for all 5, adjust
            if (_imageHistory.Count < 5)
            {
                numActiveThumbnails = _imageHistory.Count;
            }

            //Loop through and update images for all of the thumbnail frames
            for (int i = 0; i < numActiveThumbnails; i++)
            {
                _pictureBoxThumbnails[i].Image = new Bitmap(_imageHistory[i], 128, 72);
            }
        }

        /// <summary>
        /// Resizes the image while maintaining the aspect ratio,
        /// then centers image on the display
        /// Modified from http://stackoverflow.com/questions/1922040/resize-an-image-c-sharp
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            if (image.Width / image.Height > width / height)
            {
                int newHeight;
                if (image.Width > width)
                {
                    newHeight = image.Height / (image.Width / width);
                }
                else
                {
                    newHeight = image.Height *width / image.Width;
                }
                destRect = new Rectangle(0, (height - newHeight) / 2, width, newHeight);
            }
            else
            {
                int newWidth;
                if (image.Height > height)
                {
                    newWidth = image.Width / (image.Height / height);
                }
                else
                {
                    newWidth = image.Width * height / image.Height;
                }
                destRect = new Rectangle((width - newWidth) / 2, 0, newWidth, height);
            }



            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        #endregion

        #region Event Handlers

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.AutoSize = true;
            
            //Setup initial pen for drawing
            _brushSize = 5;
            _pen = new Pen(_color, _brushSize);
            _pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            _pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            //Add thumbnail picture boxes so they can be loop updated
            _pictureBoxThumbnails.Add(pictureBoxThumbnail1);
            _pictureBoxThumbnails.Add(pictureBoxThumbnail2);
            _pictureBoxThumbnails.Add(pictureBoxThumbnail3);
            _pictureBoxThumbnails.Add(pictureBoxThumbnail4);
            _pictureBoxThumbnails.Add(pictureBoxThumbnail5);
        }

        private void buttonScreenshot_Click(object sender, EventArgs e)
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
            pictureBoxCapturedImage.Image = bmpScreenshot;
            pictureBoxCapturedImage.Height = bmpScreenshot.Height;
            pictureBoxCapturedImage.Width = bmpScreenshot.Width;

            //Add the latest image
            _imageHistory.Insert(0, bmpScreenshot);
            //Update the thumbnails 
            UpdateThumbnails();
            //Set the active bitmap for drawing
            _activeBMP = bmpScreenshot;
            //Make note that a screenshot has been captured
            _screenshotPresent = true;
        }
     
        private void pictureBoxCapturedImage_MouseDown(object sender, MouseEventArgs e)
        {
            //Ensure that there is a screenshot before allowing annotations
            if (_screenshotPresent)
            {
                //Allow drawing of a single point, not just lines (because we rely on mouse move)
                Graphics drawPanel = Graphics.FromImage(_activeBMP);
                drawPanel.FillRectangle(new SolidBrush(_color), e.X, e.Y, _brushSize, _brushSize);
                drawPanel.Save();
                pictureBoxCapturedImage.Image = _activeBMP;

                //Start line drawing for drag
                _draw = true;
                _x = e.X;
                _y = e.Y;
            }

        }

        private void pictureBoxCapturedImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draw)
            {
                Graphics g = Graphics.FromImage(_activeBMP);
                g.DrawLine(_pen, _x, _y, e.X, e.Y);
                g.Save();
                pictureBoxCapturedImage.Image = _activeBMP;
            }

            _x = e.X;
            _y = e.Y;
        }

        private void pictureBoxCapturedImage_MouseUp(object sender, MouseEventArgs e)
        {
            _draw = false;
        }

        private void pictureBoxThumbnail1_Click(object sender, EventArgs e)
        {
            if(_imageHistory.Count >= 1)
            {
                pictureBoxCapturedImage.Image = _imageHistory[0];
                _activeBMP = _imageHistory[0];
                UpdateThumbnails();
            }

        }

        private void pictureBoxThumbnail2_Click(object sender, EventArgs e)
        {
            if (_imageHistory.Count >= 2)
            {
                pictureBoxCapturedImage.Image = _imageHistory[1];
                _activeBMP = _imageHistory[1];
                UpdateThumbnails();
            }
        }

        private void pictureBoxThumbnail3_Click(object sender, EventArgs e)
        {
            if (_imageHistory.Count >= 3)
            {
                pictureBoxCapturedImage.Image = _imageHistory[2];
                _activeBMP = _imageHistory[2];
                UpdateThumbnails();
            }
        }

        private void pictureBoxThumbnail4_Click(object sender, EventArgs e)
        {
            if (_imageHistory.Count >= 4)
            {
                pictureBoxCapturedImage.Image = _imageHistory[3];
                _activeBMP = _imageHistory[3];
                UpdateThumbnails();
            }
        }

        private void pictureBoxThumbnail5_Click(object sender, EventArgs e)
        {
            if (_imageHistory.Count >= 5)
            {
                pictureBoxCapturedImage.Image = _imageHistory[4];
                _activeBMP = _imageHistory[4];
                UpdateThumbnails();
            }
        }
        private void buttonColorChooser_Click(object sender, EventArgs e)
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                _color = colorDialog1.Color;
                _pen.Color = _color;
            }
        }

        private void numericUpDownBrushSize_ValueChanged(object sender, EventArgs e)
        {
            _brushSize = (float)numericUpDownBrushSize.Value;
            _pen.Width = _brushSize;
        }

        private void buttonUploadImage_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.gif,"
                                    + " *.png) | *.bmp; *.jpg; *.jpeg; *.gif; "
                                    + " *.png";

            openFileDialog.Multiselect = false;
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                Bitmap imageUpload = (Bitmap)Image.FromFile(fileName);
                Bitmap resizedImage = ResizeImage(imageUpload, pictureBoxCapturedImage.Width, pictureBoxCapturedImage.Height);
                //imageUpload.SetResolution(pictureBoxCapturedImage.Width, pictureBoxCapturedImage.Height);
                // Print out the screenshot to the picture box.
                pictureBoxCapturedImage.Image = resizedImage;

                //Add the latest image
                _imageHistory.Insert(0, resizedImage);
                //Update the thumbnails 
                UpdateThumbnails();
                //Set the active bitmap for drawing
                _activeBMP = resizedImage;
                //Make note that a screenshot has been captured
                _screenshotPresent = true;
            }

        }
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            CreateConnectionWindow connectionWindow = new CreateConnectionWindow();
            connectionWindow.ShowDialog();

            if (!connectionWindow.isCanceled)
            {
              /*  
                
                networkManager = new NetworkManager();

                networkManager.connect(connectionWindow.Hostname,
                                       connectionWindow.Port,
                                       connectionWindow.Port);
               */
            }
        }

        #endregion

    }
}