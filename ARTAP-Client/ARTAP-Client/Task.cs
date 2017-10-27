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
    }
}
