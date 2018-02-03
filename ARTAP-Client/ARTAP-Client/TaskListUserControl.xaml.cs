using ARTAPclient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for TaskListUserControl.xaml
    /// </summary>
    public partial class TaskListUserControl : UserControl
    {
        private ScreenshotAnnotationsWindow _ui;

        public TaskListUserControl(ScreenshotAnnotationsWindow window)
        {
            _ui = window;
            InitializeComponent();
        }

        private void AddTaskClick(object sender, RoutedEventArgs e)
        {
            _ui.MakeNewTask(null, null);
            ScrollView.ScrollToEnd();
        }

        private void SendTaskClick(object sender, RoutedEventArgs e)
        {
            _ui.SendTaskList();
        }
    }
}
