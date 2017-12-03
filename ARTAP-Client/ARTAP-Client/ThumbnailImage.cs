using System.Windows.Controls;

namespace WpfApplication1
{
    public class ThumbnailImage
    {
        public ThumbnailImage(Image image, bool isSelected, bool isPdf = false)
        {
            Image = image;
            IsSelected = isSelected;
        }

        public Image Image { get; set; }
        public bool IsSelected { get; set; }
        public bool IsPdf { get; set; }
    }
}