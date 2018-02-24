using System;

namespace WpfApplication1
{
    public class PanoImage
    {
        public byte[] position;
        public byte[] image;

        public PanoImage() { }

        public PanoImage(byte[] image, byte[] position)
        {
            this.image = image;
            this.position = position;
        }

        public byte[] toByteArray()
        {
            var imageBytesLength = BitConverter.GetBytes(image.Length);
            byte[] finalBytes = new byte[image.Length + 48];
            Buffer.BlockCopy(imageBytesLength, 0, finalBytes, 0, 4);
            Buffer.BlockCopy(position, 0, finalBytes, 4, 44);
            Buffer.BlockCopy(image, 0, finalBytes, 52, image.Length);
            return finalBytes;
        }

        public static PanoImage FromByteArray(byte[] bytes)
        {
            byte[] positionBytes = new byte[44];
            byte[] imageBytes = new byte[bytes.Length - 44];
            Buffer.BlockCopy(bytes, 0, positionBytes, 0, 44);
            Buffer.BlockCopy(bytes, 44, imageBytes, 0, bytes.Length - 44);
            var imagePosition = positionBytes;
            return new PanoImage(imageBytes, imagePosition);
        }
    }
}
