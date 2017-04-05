using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ARTAPclient
{
    public class AnnotatedImage
    {
        #region Private Fields

        /// <summary>
        /// List of annotations drawn on the AnnotatedImage
        /// </summary>
        private List<Polyline> _annotations = new List<Polyline>(10);

        #endregion


        #region Constructor
        
        /// <summary>
        /// Creates a new annotated image
        /// </summary>
        /// <param name="originalImage">Original image to create with</param>
        public AnnotatedImage(ImageSource originalImage)
        {
            OriginalImage = originalImage;
            LatestImage = originalImage;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds line to list of annotations for this image
        /// </summary>
        /// <param name="line">Line to add</param>
        public void AddAnnotation(Polyline line)
        {
            _annotations.Add(line);
        }

        /// <summary>
        /// Removes the last annotation added
        /// </summary>
        public void UndoAnnotation()
        {
            if (_annotations.Count > 0)
            {
                _annotations.RemoveAt(_annotations.Count - 1);
            }
        }

        /// <summary>
        /// Gets the polylines that make up the annotations for this image
        /// </summary>
        /// <returns>Array of annotation polylines</returns>
        public Polyline[] GetAnnotations()
        {
            return _annotations.ToArray();
        }

        /// <summary>
        /// Clears all annotations
        /// </summary>
        public void ClearAnnotations()
        {
            _annotations.Clear();
        }

        /// <summary>
        /// Sets visibility of all Annotations drawn on this image
        /// </summary>
        /// <param name="visibility">Visibility to set to</param>
        public void SetAnnotationsVisibility(Visibility visibility)
        {
            foreach (UIElement line in _annotations)
            {
                line.Visibility = visibility;
            }
        }

        public UIElement GetLastAnnotation()
        {
            return _annotations.Last();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The original image associated with this annotated image
        /// </summary>
        public ImageSource OriginalImage { get; private set; }

        /// <summary>
        /// Latest staved variation of the image with annotations
        /// </summary>
        public ImageSource LatestImage { get; set; }

        /// <summary>
        /// Indicates how many annotations are in the image
        /// </summary>
        public int NumAnnotations
        {
            get
            {
                return _annotations.Count;
            }
        }

        #endregion

    }
}
