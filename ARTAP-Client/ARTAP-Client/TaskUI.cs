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

        public TaskUI(Task task, int margin, Style buttonStyle, Style taskTextStyle, Style imageStyle)
        {
            Task = task;
            Id = Task.Id;
            //margin += 20;

           NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(40, margin, 100, 0),
                MinWidth = 450,
                MaxWidth = 450,
                Tag = Task.Name,
                VerticalAlignment = VerticalAlignment.Top,
                Style = taskTextStyle
            };

            IsCompletedUI = new CheckBox
            {
                IsChecked = Task.IsCompleted,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(530, margin + 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };

            Remove = new Button
            {
                Height = 20,
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, margin + 3, 570, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Style = buttonStyle
            };

            AddImage = new Button
            {
                //Content = "Add Image",
                Height = 30,
                Width = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(450, margin, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                ToolTip= "Add Image",
                Style = imageStyle
            };
        }

        public void SetMargins(int margin)
        {
            NameUI.Margin = new Thickness(40, margin, 100, 0);
            IsCompletedUI.Margin = new Thickness(530, margin + 4, 0, 0);
            Remove.Margin = new Thickness(0, margin + 3, 570, 0);
            AddImage.Margin = new Thickness(450, margin, 0, 0);
        }

        public void RecreateUIElements(int margin, Style buttonStyle, Style taskTextStyle, Style imageStyle)
        {
            NameUI = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(40, margin, 100, 0),
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
                Margin = new Thickness(530, margin + 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };

            Remove = new Button
            {
                Height = 20,
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, margin + 3, 570, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Style = buttonStyle
            };

            AddImage = new Button
            {
                //Content = "Add Image",
                Height = 30,
                Width = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(450, margin, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                ToolTip = "Add Image",
                Style = imageStyle
            };
        }
    }
}
