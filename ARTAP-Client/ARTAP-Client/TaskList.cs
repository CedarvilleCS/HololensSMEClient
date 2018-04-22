using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class TaskList
    {
        public TaskList()
        {
        }

        public TaskList(int id)
        {
            Id = id;
            Name = $"List{id + 1}";
            Tasks = new List<Task>
            {
                new Task(0)
            };
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<Task> Tasks { get; set; }
        public bool IsCompleted { get; private set; }

        public static TaskList FromByteArray(byte[] bytes)
        {
            var id = BitConverter.ToInt32(SubArray(bytes, 0, 4), 0);

            var nameLength = BitConverter.ToInt32(SubArray(bytes, 4, 4), 0);
            var currentPosition = 8 + nameLength;
            var name = Encoding.ASCII.GetString(SubArray(bytes, 8, nameLength));

            var tasks = new List<Task>();
            while (currentPosition < bytes.Length)
            {
                var taskLength = BitConverter.ToInt32(SubArray(bytes, currentPosition, 4), 0);
                tasks.Add(Task.FromByteArray(SubArray(bytes, currentPosition + 4, taskLength - 4)));
                currentPosition += taskLength;
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

            var nameBytes = Encoding.ASCII.GetBytes(Name);
            var nameBytesLength = BitConverter.GetBytes(nameBytes.Length);

            var taskBytes = Tasks.AsParallel().SelectMany(t => t.ToByteArray()).ToArray();

            return idBytes.Concat(nameBytesLength)
                .Concat(nameBytes)
                .Concat(taskBytes)
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
