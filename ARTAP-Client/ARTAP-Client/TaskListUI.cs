using System.Collections.Generic;
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

        public TaskListUI(TaskList taskList, Style buttonStyle, Style title, Style tStyle)
        {
            TaskList = taskList;

            TaskUIs = new List<TaskUI>();

            var name = $"List{TaskList.Id + 1}";
            Button = new Button
            {
                Content = $"list{TaskList.Id + 1}",
                Height = 30,
                Tag = name,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150
            };

            NameTextBox = new TextBox
            {
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = name,
                Text = name,
                VerticalAlignment = VerticalAlignment.Top,
                Style = title
            };

            AddTaskUI(buttonStyle, tStyle, 60);
        }

        public void AddTaskUI(Style buttonStyle, Style taskStyle, int margin)
        {
            TaskUIs.Add(new TaskUI(new Task(), margin, buttonStyle, taskStyle));
        }

        public void RemoveTaskUI(TaskUI uiTask, Grid taskGrid)
        {
            taskGrid.Children.Remove(uiTask.IsCompletedUI);
            taskGrid.Children.Remove(uiTask.NameUI);
            taskGrid.Children.Remove(uiTask.Remove);
            taskGrid.Children.Remove(uiTask.AddImage);

            TaskUIs.Remove(uiTask);
            TaskList.Tasks.Remove(uiTask.Task);
        }

        public void SetTaskUIMargins(int margin)
        {
            var offset = 0;
            foreach (var taskUI in TaskUIs)
            {
                taskUI.SetMargins(margin + offset);
                offset += 30;
            }
        }

        public void RecreateUIElements(Style buttonStyle, Style title, Style taskStyle)
        {
            var name = TaskList.Name;
            NameTextBox = new TextBox
            {
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = name,
                VerticalAlignment = VerticalAlignment.Top,
                Style = title
            };

            var margin = 60;
            foreach (var taskUI in TaskUIs)
            {
                taskUI.RecreateUIElements(margin, buttonStyle, taskStyle);
                margin += 30;
            }
        }

        public void ReorderIds()
        {
            TaskList.ReorderIds();
            for (var i = 0; i < TaskUIs.Count; i++)
            {
                TaskUIs[i].Id = i;
            }
        }
    }
}
