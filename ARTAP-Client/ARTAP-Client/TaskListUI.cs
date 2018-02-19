using System;
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

        public TaskListUI(TaskList taskList, Style buttonStyle, Style title, Style tStyle, Style imageStyle)
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

            AddTaskUI(buttonStyle, tStyle, imageStyle, 60, TaskUIs.Count);
        }

        public void AddTaskUI(Style buttonStyle, Style taskStyle, Style imageStyle, int margin, int count)
        {
            TaskUIs.Add(new TaskUI(new Task(), margin, buttonStyle, taskStyle, imageStyle, count));
        }

        public void RemoveTaskUI(TaskUI uiTask, Grid taskGrid)
        {
            taskGrid.Children.Remove(uiTask.IsCompletedUI);
            taskGrid.Children.Remove(uiTask.NameUI);
            taskGrid.Children.Remove(uiTask.Remove);
            taskGrid.Children.Remove(uiTask.AddImage);

            TaskList.Tasks.Remove(uiTask.Task);
            TaskUIs.Remove(uiTask);
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

        public void RecreateUIElements(Style title)
        {
            var name = $"List{TaskList.Id + 1}";
            NameTextBox = new TextBox
            {
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = name,
                Tag = name,
                VerticalAlignment = VerticalAlignment.Top,
                Style = title
            };

            var margin = 60;
            foreach (var taskUI in TaskUIs)
            {
                taskUI.RecreateUIElements(margin);
                margin += 30;
            }
        }

        public void ReorderTasks()
        {
            for (var i = 0; i < TaskUIs.Count; i++)
            {
                TaskUIs[i].Order = i;
            }
        }
    }
}
