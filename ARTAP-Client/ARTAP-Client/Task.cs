using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public Image Attachment { get; set; }

        public Task()
        {
            IsCompleted = false;
        }

        public Task(int id, string name)
        {
            Id = id;
            Name = Name;
            IsCompleted = false;
        }

        public static Task FromByteArray(byte[] bytes)
        {
            var id = BitConverter.ToInt32(SubArray(bytes, 0, 4), 0);
            var isCompleted = BitConverter.ToBoolean(SubArray(bytes, 4, 1), 0);

            var nameLength = BitConverter.ToInt32(SubArray(bytes, 5, 4), 0);
            var currentPosition = 9 + nameLength;
            var name = Encoding.ASCII.GetString(SubArray(bytes, 9, nameLength));

            var imageLength = BitConverter.ToInt32(SubArray(bytes, currentPosition, 4), 0);
            var imageBytes = SubArray(bytes, currentPosition + 4, imageLength);
            Bitmap image = null;
            if (imageBytes.Length > 0)
            {
                image = (Bitmap)((new ImageConverter()).ConvertFrom(imageBytes));
            }

            return new Task
            {
                Id = id,
                Name = name,
                IsCompleted = isCompleted,
                Attachment = image
            };
        }

        public byte[] ToByteArray()
        {
            var idBytes = BitConverter.GetBytes(Id);

            var nameBytes = Encoding.ASCII.GetBytes(Name);
            var nameBytesLength = BitConverter.GetBytes(nameBytes.Length);

            var completedBytes = BitConverter.GetBytes(IsCompleted);

            var converter = new ImageConverter();
            var imageBytes = (byte[])converter.ConvertTo(Attachment, typeof(byte[]));
            var imageBytesLength = BitConverter.GetBytes(imageBytes.Length);
            
            var allBytes = idBytes.Concat(completedBytes)
                .Concat(nameBytesLength)
                .Concat(nameBytes)
                .Concat(imageBytesLength)
                .Concat(imageBytes)
                .ToArray();

            return (BitConverter.GetBytes(allBytes.Length + 4)).Concat(allBytes).ToArray();
        }

        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] toReturn = new byte[length];
            Array.Copy(data, start, toReturn, 0, length);
            return toReturn;
        }
    }
}
