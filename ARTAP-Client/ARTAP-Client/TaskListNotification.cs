using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class TaskListNotification
    {
        public bool Completed { get; set; }
        public int TaskIndex { get; set; }
        public int TaskListId { get; set; }

        public TaskListNotification(byte[] data)
        {
            TaskListId = BitConverter.ToInt32(data, 0);
            TaskIndex = BitConverter.ToInt32(data, 4);
            Completed = BitConverter.ToBoolean(data, 9);
        }
    }
}
