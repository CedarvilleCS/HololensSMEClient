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
        /// <summary>
        /// Number of the page to display when the GUI is loaded.
        /// </summary>
        public const int INITIAL_PAGE_TO_DISPLAY = 1;

        /// <summary>
        /// Filename of the PDF file being displayed.
        /// </summary>
        public string pdfFile { get; private set; }

        /// <summary>
        /// Page selected by the user to be displayed, if the window
        /// is closed without a page being selected, this will
        /// remain null.
        /// </summary>
        public List<BitmapImage> selectedImages { get; private set; } = null;

        /// <summary>
        /// Keeps track of the total number of pages in the current PDF.
        /// </summary>
        private int totalPages;

        /// <summary>
        /// Constructs a PDFViewerDialog.
        /// </summary>
        /// <param name="pdfFile">Path to the PDF file to display</param>
        /// <remarks>When textBoxPageNumber.text is changed the event handler: <see cref="onTextChangedTextBoxPageNumber(object, TextChangedEventArgs)"/> will be called.</remarks>
        /// <seealso cref="onTextChangedTextBoxPageNumber(object, TextChangedEventArgs)"/>
        public PDFViewerDialog(string pdfFile)
        {
            totalPages = PDFManager.getNumPages(pdfFile);
            this.pdfFile = pdfFile;

            InitializeComponent();

            textBoxPageNumber.Text = Convert.ToString(INITIAL_PAGE_TO_DISPLAY);

            selectedImages = new List<BitmapImage>();
        }

        /// <summary>
        /// Displays the specified page to <see cref="imagePageDisplay"/>.
        /// </summary>
        /// <param name="pageNumber">Number denoting the desired page to display.</param>
        private void displayPage(int pageNumber)
        {
            buttonBack.IsEnabled = pageNumber != 1;
            buttonForward.IsEnabled = pageNumber != totalPages;

            System.Drawing.Image image = PDFManager.getImage(pdfFile, pageNumber);

            //
            // Convert the image into a bitmap object
            //
            // NOTE: You can't force garbage collection on the 
            //       memory stream after this done, it will 
            //       cause a NullReferenceException when trying
            //       to display the page in the annotation window.
            //
            BitmapImage page;

            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            page = new BitmapImage();
            page.BeginInit();
            page.StreamSource = ms;
            page.EndInit();

            imagePageDisplay.Source = page;
        }

        /// <summary>
        /// Changes the currently displayed page by a specified delta.
        /// </summary>
        /// <param name="delta"> Number of pages to change by, positve for forward, negative for back.</param>
        /// <returns>True if the page was changed False if there was an error.</returns>
        /// <seealso cref="setPage(int)"/>
        private bool changePageBy(int delta)
        {
            int currentPage = Convert.ToInt32(textBoxPageNumber.Text);
            int newPage = currentPage + delta;

            return setPage(newPage);
        }

        /// <summary>
        /// Sets the currently displayed page to a specified value.
        /// </summary>
        /// <param name="page">Page number of the page to display.</param>
        /// <returns>True if the page number is valid, False if the page number is invalid.</returns>
        /// <remarks>When textBoxPageNumber.text is changed the event handler: <see cref="onTextChangedTextBoxPageNumber(object, TextChangedEventArgs)"/> will be called.</remarks>
        /// <seealso cref="onTextChangedTextBoxPageNumber(object, TextChangedEventArgs)"/>
        private bool setPage(int page)
        {
            if (page > 0 && page <= totalPages)
            {
                textBoxPageNumber.Text = Convert.ToString(page);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// When the <see cref="textBoxPageNumber"/>'s text is changed this handler fires to load
        /// and display the new page from the PDF.
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">Event args (none)</param>
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

                if (pageNumber < 0 || pageNumber > totalPages)
                {
                    System.Windows.MessageBox.Show("Pages number must be between 0 and " + totalPages + ".\nGot " + pageNumber);
                }
                else
                {
                    //
                    // Display the page
                    //
                    if (pageNumber != -1)
                    {
                        displayPage(pageNumber);
                    }
                }
            }
            catch (FormatException)
            {
                System.Windows.MessageBox.Show("Not a valid page number.");
            }
            catch (OverflowException)
            {
                System.Windows.MessageBox.Show("Number overflows Int32 range.");
            }
        }

        /// <summary>
        /// When the select button is clicked, this event fires, closing the
        /// window and setting the <see cref="selectedImages"/> to the currently
        /// displayed image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onClickButtonSelect(object sender, RoutedEventArgs e)
        {
            selectedImages.Add((BitmapImage)imagePageDisplay.Source);
        }

        private void onCloseButtonSelect(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Sets the dialog result when the window is closed.
        /// </summary>
        private new void Close()
        {
            DialogResult = selectedImages.Any();
            base.Close();
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
