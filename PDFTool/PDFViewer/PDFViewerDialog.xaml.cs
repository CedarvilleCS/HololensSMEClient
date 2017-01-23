using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PDFViewer;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for PDFViewerDialog.xaml
    /// </summary>
    public partial class PDFViewerDialog : Window
    {

        private PDFViewer pdf = new PDFViewer();

        public PDFViewerDialog(string pdfFile)
        {
            InitializeComponent();
            
            pdf.
        }

        private void onTextChangedTextBoxPageNumber(object sender, TextChangedEventArgs e)
        {
            int pageNumber = -1;
            string text = textBoxPageNumber.Text;

            try
            {
                pageNumber = Convert.ToInt32(text);
            }
            catch (FormatException)
            {
                System.Windows.MessageBox.Show("Not a valid page number.");
            }
            catch (OverflowException)
            {
                System.Windows.MessageBox.Show("Number overflows Int32 range.");
            }

            if (pageNumber < 0 || )
        }
    }
}
