using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace WpfApplication1
{
    public class TaskUI
    {
        public int Id { get; set; }
        public Task Task { get; set; }
        public TextBox NameUI { get; set; }
        public CheckBox IsCompletedUI { get; set; }
        public Button AddImage { get; set; }
        public Button Remove { get; set; }

        public TaskUI(Task task, int margin, Style buttonStyle, Style taskTextStyle)
        {
            Task = task;
            Id = Task.Id;

            NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(50, margin, 70, 0),
                MinWidth = 450,
                VerticalAlignment = VerticalAlignment.Top,
                Style = taskTextStyle
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
                Style = buttonStyle
            };

            AddImage = new Button
            {
                Content = "Add Image",
                Height = 20,
                Width = 50
            };
        }

        public void SetMargins(int margin)
        {
            NameUI.Margin = new Thickness(50, margin, 70, 0);
            IsCompletedUI.Margin = new Thickness(500, margin + 4, 0, 0);
            Remove.Margin = new Thickness(0, margin + 3, 530, 0);
        }

        public void RecreateUIElements(int margin, Style buttonStyle, Style taskTextStyle)
        {
            NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(50, margin, 70, 0),
                MinWidth = 450,
                MaxWidth = 450,
                Tag = Task.Name,
                VerticalAlignment = VerticalAlignment.Top,
                Style = taskTextStyle
            };

            if (!Task.IsNew) NameUI.Text = Task.Name;
            else NameUI.SetValue(TextBoxHelper.WatermarkProperty, Task.Name);

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
                Style = buttonStyle
            };

            AddImage = new Button
            {
                Content = "Add Image",
                Height = 20,
                Width = 50
            };
        }
    }
}
