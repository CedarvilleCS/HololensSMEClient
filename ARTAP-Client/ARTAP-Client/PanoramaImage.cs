using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApplication1
{
    public class Panorama
    {
        public int Id { get; set; }
        public List<Polyline> CurrentView { get; set; }
        public byte[] CurrentPositionId { get; set; }
        public ImageSource Image { get; set; }
    }
}
