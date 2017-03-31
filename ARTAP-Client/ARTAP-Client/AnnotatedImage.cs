using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ARTAPclient
{
    public class AnnotatedImage
    {
        #region Private Fields

        /// <summary>
        /// Current index in annotation array
        /// </summary>
        private int _annotationIndex = 0;

        /// <summary>
        /// List of annotations drawn on the AnnotatedImage
        /// </summary>
        private List<Polyline> _annotations = new List<Polyline>(10);

        /// <summary>
        /// Was there an arrow placed?
        /// </summary>
        private Boolean arrowPlaced;

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
            _annotationIndex++;
        }

        /// <summary>
        /// Removes the last annotation added
        /// </summary>
        public void UndoAnnotation()
        {
            if (_annotationIndex > 0)
            {
                _annotations.RemoveAt(--_annotationIndex);
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
            _annotationIndex = 0;
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
        /// Indicates which polyline we are currently on 
        /// </summary>
        public int CurrentAnnotationIndex
        {
            get
            {
                return _annotationIndex - 1;
            }
        }


        /// <summary>
        /// Getter and Setter for ArrowPlaced
        /// </summary>
        public Boolean ArrowPlaced
        {
            get
            {
                return arrowPlaced;
            }
            set
            {
                arrowPlaced = value;
            }
        }

        #endregion

    }
}
