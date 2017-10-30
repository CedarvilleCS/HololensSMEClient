using System.Collections.Generic;
using System.Linq;

namespace WpfApplication1
{
    public class TaskList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Task> Tasks { get; set; }
        public bool IsCompleted => !Tasks.AsParallel().Any(task => !task.IsCompleted);
    }
}
