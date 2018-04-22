using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace WpfApplication1
{
    public class Task
    {
        public int Id { get; set; }
        public bool IsNew { get; set; }
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public BitmapImage Attachment { get; set; }

        public Task()
        {
            Id = 0;
            IsNew = true;
            Name = $"Task{Id + 1}";
            IsCompleted = false;
        }

        public Task(int id)
        {
            Id = id;
            IsNew = true;
            Name = $"Task{Id + 1}";
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

            BitmapImage bitmap = null;
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                bitmap = image;
            }

            return new Task
            {
                Id = id,
                Name = name,
                IsCompleted = isCompleted,
                Attachment = bitmap
            };
        }

        public byte[] ToByteArray()
        {
            var idBytes = BitConverter.GetBytes(Id);

            var nameBytes = Encoding.ASCII.GetBytes(Name);
            var nameBytesLength = BitConverter.GetBytes(nameBytes.Length);

            var completedBytes = BitConverter.GetBytes(IsCompleted);

            var converter = new ImageConverter();
            byte[] imageBytes;

            if (Attachment != null)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Attachment));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    imageBytes = ms.ToArray();
                }
            } 
            else
            {
                imageBytes = new byte[0];
            }

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
