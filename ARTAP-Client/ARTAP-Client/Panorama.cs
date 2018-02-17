using ARTAPclient;
using Emgu.CV;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApplication1
{
    public class Panorama
    {
        public int Id { get; set; }
        public Polyline Drawing { private get; set; }
        public Point Location { private get; set; }
        public Bitmap Image { get; set; }

        public Panorama() { }

        public Panorama(List<PanoImage> images)
        {
            StitchImages(images);
            AddViewRectangle();
        }

        public void AddViewRectangle()
        {
            Drawing = new Polyline()
            {
                Points = new System.Windows.Media.PointCollection
                {
                    new System.Windows.Point(Location.X - 4, Location.Y + 2),
                    new System.Windows.Point(Location.X + 4, Location.Y + 2),
                    new System.Windows.Point(Location.X - 4, Location.Y - 2),
                    new System.Windows.Point(Location.X - 4, Location.Y - 2),
                }
            };
        }

        public Image<Bgr, byte>[] ConvertImages(List<PanoImage> images)
        {
            var convertedImages = new List<Bitmap>();
            foreach (var image in images)
            {
                using (var ms = new MemoryStream(image.image))
                {
                    convertedImages.Add(new Bitmap(ms));
                }
            }

            var size = convertedImages.Count;
            var finalImages = new Image<Bgr, byte>[size];
            for (var i = 0; i < size; i++)
            {
                finalImages[i] = new Image<Bgr, byte>(convertedImages[i]);
            }

            return finalImages;
        }

        public void StitchImages(List<PanoImage> images)
        {
            var convertedImages = ConvertImages(images);
            using (var stitcher = new Stitcher(false))
            {
                using (var vectorMat = new VectorOfMat())
                {
                    var result = new Mat();
                    vectorMat.Push(convertedImages);
                    stitcher.Stitch(vectorMat, result);

                    Image = result.Bitmap;
                    result.Dispose();
                }
            }
        }
    }
}
