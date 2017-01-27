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
        public const int INITIAL_PAGE_TO_DISPLAY = 1;

        public string pdfFile { get; private set; }
        public BitmapImage selectedImage { get; private set; } = null;

        private int totalPages;

        public PDFViewerDialog(string pdfFile)
        {
            InitializeComponent();

            this.pdfFile = pdfFile;
            textBoxPageNumber.Text = Convert.ToString(INITIAL_PAGE_TO_DISPLAY);

            totalPages = PDFManager.getNumPages(pdfFile);
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

        private bool changePageBy(int delta)
        {
            int currentPage = Convert.ToInt32(textBoxPageNumber.Text);
            int newPage = currentPage + delta;

            if(newPage > 0 && newPage < totalPages)
            {
                textBoxPageNumber.Text = Convert.ToString(newPage);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void onTextChangedTextBoxPageNumber(object sender, TextChangedEventArgs e)
        {
            int pageNumber = -1;
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
            selectedImage = (BitmapImage)imagePageDisplay.Source;
            this.Close();
        }

        private void onClickButtonBack(object sender, RoutedEventArgs e)
        {
            changePageBy(-1);
        }

        private void onClickButtonForward(object sender, RoutedEventArgs e)
        {
            changePageBy(1);
        }
    }
}
