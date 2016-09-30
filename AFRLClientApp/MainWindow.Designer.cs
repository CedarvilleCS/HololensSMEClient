namespace AFRLClientApp
{
  partial class MainWindow
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBoxCapturedImage = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBoxThumbnail1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxThumbnail2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxThumbnail3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxThumbnail4 = new System.Windows.Forms.PictureBox();
            this.pictureBoxThumbnail5 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCapturedImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail5)).BeginInit();
            this.SuspendLayout();
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(12, 12);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(640, 360);
            this.axWindowsMediaPlayer1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(499, 404);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(136, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Capture Screenshot";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonScreenshot_Click);
            // 
            // pictureBoxCapturedImage
            // 
            this.pictureBoxCapturedImage.Location = new System.Drawing.Point(665, 12);
            this.pictureBoxCapturedImage.Name = "pictureBoxCapturedImage";
            this.pictureBoxCapturedImage.Size = new System.Drawing.Size(640, 360);
            this.pictureBoxCapturedImage.TabIndex = 2;
            this.pictureBoxCapturedImage.TabStop = false;
            this.pictureBoxCapturedImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCapturedImage_MouseDown);
            this.pictureBoxCapturedImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCapturedImage_MouseMove);
            this.pictureBoxCapturedImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCapturedImage_MouseUp);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(499, 433);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(136, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Send Screenshot";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // pictureBoxThumbnail1
            // 
            this.pictureBoxThumbnail1.Location = new System.Drawing.Point(665, 393);
            this.pictureBoxThumbnail1.Name = "pictureBoxThumbnail1";
            this.pictureBoxThumbnail1.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail1.TabIndex = 4;
            this.pictureBoxThumbnail1.TabStop = false;
            this.pictureBoxThumbnail1.Click += new System.EventHandler(this.pictureBoxThumbnail1_Click);
            // 
            // pictureBoxThumbnail2
            // 
            this.pictureBoxThumbnail2.Location = new System.Drawing.Point(799, 393);
            this.pictureBoxThumbnail2.Name = "pictureBoxThumbnail2";
            this.pictureBoxThumbnail2.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail2.TabIndex = 5;
            this.pictureBoxThumbnail2.TabStop = false;
            this.pictureBoxThumbnail2.Click += new System.EventHandler(this.pictureBoxThumbnail2_Click);
            // 
            // pictureBoxThumbnail3
            // 
            this.pictureBoxThumbnail3.Location = new System.Drawing.Point(933, 393);
            this.pictureBoxThumbnail3.Name = "pictureBoxThumbnail3";
            this.pictureBoxThumbnail3.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail3.TabIndex = 6;
            this.pictureBoxThumbnail3.TabStop = false;
            this.pictureBoxThumbnail3.Click += new System.EventHandler(this.pictureBoxThumbnail3_Click);
            // 
            // pictureBoxThumbnail4
            // 
            this.pictureBoxThumbnail4.Location = new System.Drawing.Point(1067, 393);
            this.pictureBoxThumbnail4.Name = "pictureBoxThumbnail4";
            this.pictureBoxThumbnail4.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail4.TabIndex = 7;
            this.pictureBoxThumbnail4.TabStop = false;
            this.pictureBoxThumbnail4.Click += new System.EventHandler(this.pictureBoxThumbnail4_Click);
            // 
            // pictureBoxThumbnail5
            // 
            this.pictureBoxThumbnail5.Location = new System.Drawing.Point(1201, 394);
            this.pictureBoxThumbnail5.Name = "pictureBoxThumbnail5";
            this.pictureBoxThumbnail5.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail5.TabIndex = 8;
            this.pictureBoxThumbnail5.TabStop = false;
            this.pictureBoxThumbnail5.Click += new System.EventHandler(this.pictureBoxThumbnail5_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1351, 478);
            this.Controls.Add(this.pictureBoxThumbnail5);
            this.Controls.Add(this.pictureBoxThumbnail4);
            this.Controls.Add(this.pictureBoxThumbnail3);
            this.Controls.Add(this.pictureBoxThumbnail2);
            this.Controls.Add(this.pictureBoxThumbnail1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.pictureBoxCapturedImage);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Name = "MainWindow";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCapturedImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail5)).EndInit();
            this.ResumeLayout(false);

    }

        #endregion

        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBoxCapturedImage;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail1;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail2;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail3;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail4;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail5;
    }
}

