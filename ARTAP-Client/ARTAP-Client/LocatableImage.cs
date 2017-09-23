using System.Collections.Generic;
using System.Linq;
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
        private static readonly Point[] MARKER_POINTS = {new Point(-2, -2),
                                                         new Point(0, 0),
                                                         new Point(1, 1),
                                                         new Point(0, 0),
                                                         new Point(-1, 1),
                                                         new Point(0, 0),
                                                         new Point(2, -2)};

        /// <summary>
        /// List of indicators of where markers are placed on the LocatableImage
        /// </summary>
        private List<Marker> _markers = new List<Marker>(10);

        /// <summary>
        /// Adds line to list of annotations for this image.
        /// </summary>
        /// <param name="relativeLocation">Point to add the marker at.</param>
        /// <param name="color">Color to draw the marker.</param>
        /// <returns>Returns the PolyLine to add to the canvas.</returns>
        public Polyline AddMarker(Point relativeLocation, Point absoluteLocation, Color color)
        {
            PointCollection newMarkerPoints = new PointCollection(MARKER_POINTS.Length);

            //
            // Translate the shape of the marker so it will be placed where we want it
            //
            foreach (Point original in MARKER_POINTS)
            {
                newMarkerPoints.Add(new Point(relativeLocation.X + original.X * SCALING, 
                                              relativeLocation.Y + original.Y * SCALING));
            }

            Polyline x = new Polyline();

            x.StrokeThickness = MARKER_THICKNESS;
            x.Stroke = new SolidColorBrush(color);

            x.Points = newMarkerPoints;

            _markers.Add(new Marker(x, relativeLocation, absoluteLocation, color));
            return x;
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
            foreach (Marker m in _markers)
            {
                m.Annotation.Visibility = visibility;
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
        /// Indicates how many annotations are in the image
        /// </summary>
        public int NumMarkers
        {
            get
            {
                return _markers.Count;
            }
        }

        public Marker GetLastMarker()
        {
            return _markers.Last();
        }

        /// <summary>
        /// Gets the polylines that make up the annotations for this image
        /// </summary>
        /// <returns>Array of annotation polylines</returns>
        public Marker[] Markers
        {
            get
            {
                Marker[] markers = new Marker[_markers.Count];
                for (int i = 0; i < _markers.Count; i++)
                {
                    markers[i] = _markers[i];
                }

                return markers;
            }
        }

        #endregion
    }
}
