using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Controls;

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

            var nameLength = BitConverter.ToInt32(SubArray(bytes, 5, 9), 0);
            var currentPosition = 9 + nameLength;
            var name = BitConverter.ToString(SubArray(bytes, 9, currentPosition), 0);

            var imageLength = BitConverter.ToInt32(SubArray(bytes, currentPosition, currentPosition += 4), 0);
            var image = BitConverter.ToString(SubArray(bytes, currentPosition, currentPosition + imageLength), 0);

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

            var nameBytesLength = BitConverter.GetBytes(Name.Length);
            var nameBytes = Encoding.ASCII.GetBytes(Name);

            var completedBytes = BitConverter.GetBytes(IsCompleted);

            var converter = new ImageConverter();
            var imageBytes = (byte[])converter.ConvertTo(Attachment, typeof(byte[]));
            var imageBytesLength = BitConverter.GetBytes(imageBytes.Length);

            var allNameBytes = Concat(nameBytesLength, nameBytes);
            var allImageBytes = Concat(imageBytesLength, imageBytes);

            return Concat(Concat(idBytes, completedBytes), Concat(allNameBytes, allImageBytes));
        }

        public static byte[] Concat(byte[] arr1, byte[] arr2)
        {
            byte[] toReturn = new byte[arr1.Length + arr2.Length];
            arr1.CopyTo(toReturn, 0);
            arr2.CopyTo(toReturn, arr1.Length);
            return toReturn;
        }

        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] toReturn = new byte[length];
            Array.Copy(data, start, toReturn, 0, length);
            return toReturn;
        }
    }
}
