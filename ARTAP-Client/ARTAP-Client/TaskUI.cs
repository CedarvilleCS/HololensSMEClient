using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System;

namespace WpfApplication1
{
    public class TaskUI
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public int TaskId { get; set; }
        public TextBox NameUI { get; set; }
        public CheckBox IsCompletedUI { get; set; }
        public Button AddImage { get; set; }
        public Button Remove { get; set; }
        public Style RemoveStyle { get; set; }
        public Style TextStyle { get; set; }
        public Style ImageStyle { get; set; }

        public TaskUI(Task task, int margin, Style buttonStyle, Style taskTextStyle, Style imageStyle, int order)
        {
            TaskId = task.Id;
            Id = Guid.NewGuid();
            RemoveStyle = buttonStyle;
            TextStyle = taskTextStyle;
            ImageStyle = imageStyle;
            Order = order;

           NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(40, margin, 100, 0),
                MinWidth = 450,
                MaxWidth = 450,
                Tag = Id,
                VerticalAlignment = VerticalAlignment.Top,
                Style = TextStyle
            };

            IsCompletedUI = new CheckBox
            {
                IsChecked = task.IsCompleted,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(530, margin + 4, 0, 0),
                Tag = Id,
                VerticalAlignment = VerticalAlignment.Top,
            };

            Remove = new Button
            {
                Height = 20,
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, margin + 3, 570, 0),
                Style = RemoveStyle,
                Tag = Id,
                VerticalAlignment = VerticalAlignment.Top
            };

            AddImage = new Button
            {
                Height = 30,
                Width = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(450, margin, 0, 0),
                Style = ImageStyle,
                Tag = Id,
                ToolTip= "Add Image",
                VerticalAlignment = VerticalAlignment.Top
            };
        }

        public void SetMargins(int margin)
        {
            NameUI.Margin = new Thickness(40, margin, 100, 0);
            IsCompletedUI.Margin = new Thickness(530, margin + 4, 0, 0);
            Remove.Margin = new Thickness(0, margin + 3, 570, 0);
            AddImage.Margin = new Thickness(450, margin, 0, 0);
        }

        public void RecreateUIElements(int margin, Task task)
        {
            NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                IsEnabled = !task.IsCompleted,
                Margin = new Thickness(40, margin, 100, 0),
                MinWidth = 450,
                MaxWidth = 450,
                Tag = Id,
                VerticalAlignment = VerticalAlignment.Top,
                Style = TextStyle
            };

            if (!task.IsNew) NameUI.Text = task.Name;
            else NameUI.SetValue(TextBoxHelper.WatermarkProperty, task.Name);

            IsCompletedUI = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                IsChecked = task.IsCompleted,
                Margin = new Thickness(530, margin + 4, 0, 0),
                Tag = Id,
                VerticalAlignment = VerticalAlignment.Top,
            };

            Remove = new Button
            {
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, margin + 3, 570, 0),
                Style = RemoveStyle,
                Tag = Id,
                Width = 20,
                VerticalAlignment = VerticalAlignment.Top,
            };

            AddImage = new Button
            {
                Height = 30,
                Width = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsEnabled = !task.IsCompleted,
                Margin = new Thickness(450, margin, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Style = ImageStyle,
                Tag = Id,
                ToolTip = "Add Image"
            };
        }
    }
}
