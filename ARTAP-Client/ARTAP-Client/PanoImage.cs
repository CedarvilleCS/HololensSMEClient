using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WpfApplication1
{
    public class PanoImage
    {
        public byte[] position;
        public byte[] imageData;
        public Image image;

        public PanoImage() { }

        public PanoImage(byte[] imageData, byte[] position, Image image)
        {
            this.imageData = imageData;
            this.position = position;
            this.image = image;
        }

        public byte[] toByteArray()
        {
            var imageBytesLength = BitConverter.GetBytes(imageData.Length);
            byte[] finalBytes = new byte[imageData.Length + 48];
            Buffer.BlockCopy(imageBytesLength, 0, finalBytes, 0, 4);
            Buffer.BlockCopy(position, 0, finalBytes, 4, 44);
            Buffer.BlockCopy(imageData, 0, finalBytes, 52, imageData.Length);
            return finalBytes;
        }

        public static PanoImage FromByteArray(byte[] bytes)
        {
            byte[] positionBytes = new byte[44];
            byte[] imageBytes = new byte[bytes.Length - 44];
            Buffer.BlockCopy(bytes, 0, positionBytes, 0, 44);
            Buffer.BlockCopy(bytes, 44, imageBytes, 0, bytes.Length - 44);
            var imagePosition = positionBytes;
            Image img = null;
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                ms.Seek(0, SeekOrigin.Begin);
                Image bmp = Bitmap.FromStream(ms);
                img = bmp;
            }
            return new PanoImage(imageBytes, imagePosition, img);
        }
    }
}
