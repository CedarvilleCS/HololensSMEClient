using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace ARTAPclient
{
    public class LocatableImage : AnnotatedImage
    {
        /// <summary>
        /// Line thickness to be used when drawing the marker
        /// </summary>
        private const int MARKER_THICKNESS = 1;

        /// <summary>
        /// Scaling for marker size
        /// </summary>
        private const int SCALING = 3;

        /// <summary>
        /// Points used to draw an X
        /// </summary>
        private static readonly Point[] MARKER_POINTS = {new Point(-1, -1),
                                                         new Point(0, 0),
                                                         new Point(1, 1),
                                                         new Point(0, 0),
                                                         new Point(-1, 1),
                                                         new Point(0, 0),
                                                         new Point(1, -1)};

        /// <summary>
        /// List of indicators of where markers are placed on the LocatableImage
        /// </summary>
        private List<Polyline> _markers = new List<Polyline>(10);

        /// <summary>
        /// Adds line to list of annotations for this image.
        /// </summary>
        /// <param name="location">Point to add the marker at.</param>
        /// <param name="color">Color to draw the marker.</param>
        /// <returns>Returns the PolyLine to add to the canvas.</returns>
        public Polyline AddMarker(Point location, Color color)
        {
            PointCollection newMarkerPoints = new PointCollection(MARKER_POINTS.Length);

            //
            // Translate the shape of the marker so it will be placed where we want it
            //
            foreach (Point original in MARKER_POINTS)
            {
                newMarkerPoints.Add(new Point(location.X + original.X * SCALING, 
                                              location.Y + original.Y * SCALING));
            }

            Polyline marker = new Polyline();

            marker.StrokeThickness = MARKER_THICKNESS;
            marker.Stroke = new SolidColorBrush(color);

            marker.Points = newMarkerPoints;

            _markers.Add(marker);
            return marker;
        }

        /// <summary>
        /// Removes the last annotation added
        /// </summary>
        public void UndoMarker()
        {
            if (_markers.Count > 0)
            {
                _markers.RemoveAt(_markers.Count - 1);
            }
        }

        /// <summary>
        /// Gets the polylines that make up the annotations for this image
        /// </summary>
        /// <returns>Array of annotation polylines</returns>
        public Polyline[] GetMarkers()
        {
            return _markers.ToArray();
        }

        /// <summary>
        /// Clears all annotations
        /// </summary>
        public void ClearMarkers()
        {
            _markers.Clear();
        }

        /// <summary>
        /// Sets visibility of all markers drawn on this image
        /// </summary>
        /// <param name="visibility">Visibility to set to</param>
        public void SetMarkersVisibility(Visibility visibility)
        {
            foreach (UIElement line in _markers)
            {
                line.Visibility = visibility;
            }
        }

        #region Constructor

        /// <summary>
        /// Creates a new locatable image
        /// </summary>
        /// <param name="originalImage">Original image passed to base constructor</param>
        public LocatableImage(ImageSource originalImage) 
            : base(originalImage) {}

        #endregion

        #region Properties

        /// <summary>
        /// Position ID that tags the location of this image (retreived from HoloLens)
        /// </summary>
        public byte[] PositionID;

        /// <summary>
        /// Stores the place where the image was clicked
        /// </summary>
        public Point ArrowPosition { get; set; }

        /// <summary>
        /// Indicates how many annotations are in the image
        /// </summary>
        public int NumMarkers
        {
            get
            {
                return _markers.Count;
            }
        }

        #endregion
    }
}
