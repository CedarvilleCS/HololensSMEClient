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
            this.buttonColorChooser = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.numericUpDownBrushSize = new System.Windows.Forms.NumericUpDown();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonUploadImage = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.colorDialog2 = new System.Windows.Forms.ColorDialog();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCapturedImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBrushSize)).BeginInit();
            this.SuspendLayout();
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(13, 21);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(640, 360);
            this.axWindowsMediaPlayer1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(517, 390);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(136, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Capture Screenshot";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonScreenshot_Click);
            // 
            // pictureBoxCapturedImage
            // 
            this.pictureBoxCapturedImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxCapturedImage.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBoxCapturedImage.Location = new System.Drawing.Point(665, 21);
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
            this.button2.Location = new System.Drawing.Point(94, 387);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(136, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Send Screenshot";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // pictureBoxThumbnail1
            // 
            this.pictureBoxThumbnail1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxThumbnail1.Location = new System.Drawing.Point(1311, 12);
            this.pictureBoxThumbnail1.Name = "pictureBoxThumbnail1";
            this.pictureBoxThumbnail1.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail1.TabIndex = 4;
            this.pictureBoxThumbnail1.TabStop = false;
            this.pictureBoxThumbnail1.Click += new System.EventHandler(this.pictureBoxThumbnail1_Click);
            // 
            // pictureBoxThumbnail2
            // 
            this.pictureBoxThumbnail2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxThumbnail2.Location = new System.Drawing.Point(1311, 90);
            this.pictureBoxThumbnail2.Name = "pictureBoxThumbnail2";
            this.pictureBoxThumbnail2.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail2.TabIndex = 5;
            this.pictureBoxThumbnail2.TabStop = false;
            this.pictureBoxThumbnail2.Click += new System.EventHandler(this.pictureBoxThumbnail2_Click);
            // 
            // pictureBoxThumbnail3
            // 
            this.pictureBoxThumbnail3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxThumbnail3.Location = new System.Drawing.Point(1311, 168);
            this.pictureBoxThumbnail3.Name = "pictureBoxThumbnail3";
            this.pictureBoxThumbnail3.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail3.TabIndex = 6;
            this.pictureBoxThumbnail3.TabStop = false;
            this.pictureBoxThumbnail3.Click += new System.EventHandler(this.pictureBoxThumbnail3_Click);
            // 
            // pictureBoxThumbnail4
            // 
            this.pictureBoxThumbnail4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxThumbnail4.Location = new System.Drawing.Point(1311, 246);
            this.pictureBoxThumbnail4.Name = "pictureBoxThumbnail4";
            this.pictureBoxThumbnail4.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail4.TabIndex = 7;
            this.pictureBoxThumbnail4.TabStop = false;
            this.pictureBoxThumbnail4.Click += new System.EventHandler(this.pictureBoxThumbnail4_Click);
            // 
            // pictureBoxThumbnail5
            // 
            this.pictureBoxThumbnail5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxThumbnail5.Location = new System.Drawing.Point(1311, 324);
            this.pictureBoxThumbnail5.Name = "pictureBoxThumbnail5";
            this.pictureBoxThumbnail5.Size = new System.Drawing.Size(128, 72);
            this.pictureBoxThumbnail5.TabIndex = 8;
            this.pictureBoxThumbnail5.TabStop = false;
            this.pictureBoxThumbnail5.Click += new System.EventHandler(this.pictureBoxThumbnail5_Click);
            // 
            // buttonColorChooser
            // 
            this.buttonColorChooser.Location = new System.Drawing.Point(1195, 390);
            this.buttonColorChooser.Name = "buttonColorChooser";
            this.buttonColorChooser.Size = new System.Drawing.Size(110, 23);
            this.buttonColorChooser.TabIndex = 9;
            this.buttonColorChooser.Text = "Change Color";
            this.buttonColorChooser.UseVisualStyleBackColor = true;
            this.buttonColorChooser.Click += new System.EventHandler(this.buttonColorChooser_Click);
            // 
            // numericUpDownBrushSize
            // 
            this.numericUpDownBrushSize.Location = new System.Drawing.Point(1078, 391);
            this.numericUpDownBrushSize.Name = "numericUpDownBrushSize";
            this.numericUpDownBrushSize.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownBrushSize.TabIndex = 10;
            this.numericUpDownBrushSize.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownBrushSize.ValueChanged += new System.EventHandler(this.numericUpDownBrushSize_ValueChanged);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // buttonUploadImage
            // 
            this.buttonUploadImage.Location = new System.Drawing.Point(665, 390);
            this.buttonUploadImage.Name = "buttonUploadImage";
            this.buttonUploadImage.Size = new System.Drawing.Size(99, 23);
            this.buttonUploadImage.TabIndex = 11;
            this.buttonUploadImage.Text = "Upload Image";
            this.buttonUploadImage.UseVisualStyleBackColor = true;
            this.buttonUploadImage.Click += new System.EventHandler(this.buttonUploadImage_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(13, 387);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 12;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1012, 395);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Brush Size:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1446, 421);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.buttonUploadImage);
            this.Controls.Add(this.numericUpDownBrushSize);
            this.Controls.Add(this.buttonColorChooser);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBrushSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button buttonColorChooser;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.NumericUpDown numericUpDownBrushSize;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button buttonUploadImage;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ColorDialog colorDialog2;
        private System.Windows.Forms.Label label1;
    }
}

