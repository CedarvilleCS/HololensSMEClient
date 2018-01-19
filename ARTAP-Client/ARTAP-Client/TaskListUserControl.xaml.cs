using ARTAPclient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            var list = _ui.CurrentTaskList;
            var currentId = list.Tasks.Last().Id;

            _ui.MakeNewTask(null, null);
            ScrollView.ScrollToEnd();
        }

        private void SendTaskClick(object sender, RoutedEventArgs e)
        {
            _ui.SendTaskList();
        }
    }
}
