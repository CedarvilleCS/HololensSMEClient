using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ARTAPclient
{
    public class LocatableImage : AnnotatedImage
    {
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
        public byte[] PositionID { get; set; }

        /// <summary>
        /// Stores the place where the image was clicked
        /// </summary>
        public Point ArrowPosition { get; set; }

        #endregion
    }
}
