using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1
{
    public class TaskUI
    {
        public int Id { get; set; }
        public Task Task { get; set; }
        public TextBox NameUI { get; set; }
        public CheckBox IsCompletedUI { get; set; }
        public Button Remove { get; set; }

        public TaskUI(Task task, int margin, Style style)
        {
            Task = task;
            Id = Task.Id;

            NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(50, margin, 70, 0),
                Name = Task.Name,
                MinWidth = 450,
                VerticalAlignment = VerticalAlignment.Top,
            };

            IsCompletedUI = new CheckBox
            {
                IsChecked = Task.IsCompleted,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(500, margin + 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };

            Remove = new Button
            {
                Height = 20,
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, margin + 3, 530, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Name = "remove" + task.Id.ToString(),
                Style = style
            };
        }
    }
}
