using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ARTAPclient
{
    public class Marker
    {
        public Marker(Polyline annotation, Point relativeLocation, Point absoluteLocation, Color color)
        {
            Annotation = annotation;
            RelativeLocation = relativeLocation;
            AbsoluteLocation = absoluteLocation;
            Color = color;
        }

        public Polyline Annotation { get; private set; }
        public Point RelativeLocation { get; private set; }
        public Point AbsoluteLocation { get; private set; }
        public Color Color { get; private set; }
        public bool Sent { get; set; } = false;
    }
}