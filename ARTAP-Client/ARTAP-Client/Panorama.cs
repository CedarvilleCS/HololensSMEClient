using Emgu.CV;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    public class Panorama
    {
        public int Id { get; set; }
        public Polyline Drawing { private get; set; }
        public System.Windows.Point Location { private get; set; }
        public BitmapSource Image { get; set; }

        public Panorama() { }

        public Panorama(List<PanoImage> images)
        {
            StitchImages(images);
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
                using (var ms = new MemoryStream(image.imageData))
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

                    var bitmap = result.Bitmap;
                    bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + "pano.png", ImageFormat.Png);
                    var handle = bitmap.GetHbitmap();
                    try
                    {
                        Image = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                    result.Dispose();
                }
            }
        }
    }
}
