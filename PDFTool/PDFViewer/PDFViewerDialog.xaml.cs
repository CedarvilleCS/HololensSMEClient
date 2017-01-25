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
using PDFToImage;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for PDFViewerDialog.xaml
    /// </summary>
    public partial class PDFViewerDialog : Window
    {
        public string pdfFile { get; private set; }

        public PDFViewerDialog(string pdfFile)
        {
            InitializeComponent();

            this.pdfFile = pdfFile;
            displayPage(1);
        }

        private void displayPage(int pageNumber)
        {
            byte[] bitmapData = PDFManager.getImage(pdfFile, pageNumber);

            //
            // Convert the byte array into a bitmap object
            //
            BitmapImage page;

            using (var data = new System.IO.MemoryStream(bitmapData))
            {
                page = new BitmapImage();
                page.BeginInit();
                page.CacheOption = BitmapCacheOption.OnLoad;
                page.StreamSource = data;
                page.EndInit();
            }

            imagePageDisplay.Source = page;
        }

        private void onTextChangedTextBoxPageNumber(object sender, TextChangedEventArgs e)
        {
            int pageNumber = -1;
            int totalPages = PDFManager.getNumPages(pdfFile);
            string text = textBoxPageNumber.Text;

            //
            // Do error checking
            //
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

            if (pageNumber < 0 || pageNumber > totalPages)
            {
                System.Windows.MessageBox.Show("Pages number must be between 0 and " + totalPages + ".");
            }

            //
            // Display the page
            //
            if (pageNumber != -1)
            {
                displayPage(pageNumber);
            }

        }

        private void onClickButtonSelect(object sender, RoutedEventArgs e)
        {

        }
    }
}
