using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class TaskList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Task> Tasks { get; set; }
        public bool IsCompleted => !Tasks.AsParallel().Any(task => !task.IsCompleted);

        public static TaskList FromByteArray(byte[] bytes)
        {
            var id = BitConverter.ToInt32(SubArray(bytes, 0, 4), 0);

            var nameLength = BitConverter.ToInt32(SubArray(bytes, 4, 8), 0);
            var currentPosition = 8 + nameLength;
            var name = BitConverter.ToString(SubArray(bytes, 8, currentPosition), 0);

            var tasks = new List<Task>();
            while (currentPosition < bytes.Length)
            {
                var taskLength = BitConverter.ToInt32(SubArray(bytes, currentPosition, currentPosition += 4), 0);
                tasks.Add(Task.FromByteArray(SubArray(bytes, currentPosition, currentPosition + taskLength)));
            }

            return new TaskList
            {
                Id = id,
                Name = name,
                Tasks = tasks,
            };
        }

        public byte[] ToByteArray()
        {
            var idBytes = BitConverter.GetBytes(Id);

            var nameBytesLength = BitConverter.GetBytes(Name.Length);
            var nameBytes = Encoding.ASCII.GetBytes(Name);

            var taskBytes = Tasks.AsParallel().SelectMany(t => t.ToByteArray()).ToArray();

            var completedBytes = BitConverter.GetBytes(IsCompleted);

            return idBytes.Concat(nameBytesLength)
                .Concat(nameBytes)
                .Concat(taskBytes)
                .Concat(completedBytes)
                .ToArray();
        }

        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] toReturn = new byte[length];
            Array.Copy(data, start, toReturn, 0, length);
            return toReturn;
        }
    }
}
