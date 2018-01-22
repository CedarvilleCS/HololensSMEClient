using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1
{
    public class TaskListUI
    {
        public TaskList TaskList { get; set; }
        public List<TaskUI> TaskUIs { get; set; }
        public Button Button { get; set; }
        public TextBox NameTextBox { get; set; }

        public TaskListUI(TaskList taskList, Style style)
        {
            TaskList = taskList;

            Button = new Button
            {
                Content = $"list{TaskList.Id + 1}",
                Height = 30,
                Name = $"List{TaskList.Id + 1}",
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150
            };

            var name = TaskList.Name;

            NameTextBox = new TextBox
            {
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = name,
                Text = name,
                VerticalAlignment = VerticalAlignment.Top,
            };

            AddTaskUI(style, 60);
        }

        public void Remove(List<TaskListUI> buttons)
        {
            buttons.Remove(this);
        }

        public void AddTaskUI(Style style, int margin)
        {
            TaskUIs.Add(new TaskUI(new Task(), margin, style));
        }
    }
}
