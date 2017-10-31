using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApplication1;

namespace ARTAPclient
{
    public class Marker
    {
        public Marker(Polyline annotation, Point relativeLocation, Point absoluteLocation, Color color, Direction direction = Direction.MiddleMiddle)
        {
            Annotation = annotation;
            RelativeLocation = relativeLocation;
            AbsoluteLocation = absoluteLocation;
            Color = color;
            Direction = direction;
        }

        public Polyline Annotation { get; private set; }
        public Point RelativeLocation { get; private set; }
        public Point AbsoluteLocation { get; private set; }
        public Color Color { get; private set; }
        public Direction Direction { get; set; }
        public bool Sent { get; set; } = false;
    }
}