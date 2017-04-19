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
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SizePicker : Window
    {
        private double _brushSize;

        public double BrushSize
        {
            get
            {
                return _brushSize;
            }

            set
            {
                _brushSize = value;
            }
        }

        public SizePicker()
        {
            InitializeComponent();
        }

        public SizePicker(double brushSize)
        {
            InitializeComponent();
            BrushSize = brushSize;
            sizeTextBox.Text = BrushSize.ToString();
            sizeSlider.Value = brushSize;
        }
        #region Event Handlers

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrushSize = sizeSlider.Value;
            if (sizeTextBox != null)
            {
                sizeTextBox.Text = BrushSize.ToString();
            }
        }

        #endregion

    }
}
