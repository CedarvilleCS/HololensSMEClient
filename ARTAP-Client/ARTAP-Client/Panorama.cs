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
        public Polyline Drawing { private get; set; }
        public System.Windows.Point Location { private get; set; }
        public BitmapSource Image { get; set; }
        public int Id { get; set; }
        public List<ImagePosition> ImagePositions;
        public double MaxAngleLeft { get; set; }
        public double MaxAngleRight { get; set; }
        public double MaxHeight { get; set; }
        public double MinHeight { get; set; }
        public float[] SurfacePosition { get; set; }

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
            ImagePositions = new List<ImagePosition>();
            var convertedImages = new List<Bitmap>();
            foreach (var image in images)
            {
                ImagePositions.Add(ImagePosition.FromByteArray(image.position));
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
            InstantiatePositions();
            SavePositionToFile();
            return finalImages;
        }

        public void StitchImages(List<PanoImage> images)
        {
            var convertedImages = ConvertImages(images);
            foreach (PanoImage pi in images)
            {
                pi.image.Save("Image_" + DateTime.Now.Ticks + ".PNG", ImageFormat.Png);
            }
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                    result.Dispose();
                }
            }
        }

        public bool ContainsPoint(ImagePosition pos)
        {
            if (ImagePositions[0].IsHere(pos))
            {
                var forwardAngle = pos.GetForwardAngle();
                if (MaxAngleLeft > MaxAngleRight)
                {
                    return (forwardAngle < MaxAngleLeft && forwardAngle > MaxAngleRight);
                }
                else
                {
                    return (forwardAngle < MaxAngleLeft || forwardAngle > MaxAngleRight);
                }
            }
            return false;
        }

        public float[] GetPositionOnPano(ImagePosition pos)
        {
            var x = AngleBetween(pos.GetForwardAngle(), MaxAngleLeft) / AngleBetween(MaxAngleLeft, MaxAngleRight);
            var y = (float)((pos.Forward[1] - MinHeight) / (MaxHeight - MinHeight));
            return new float[] { x, y };
        }

        public float AngleBetween(double angle1, double angle2)
        {
            int angle1Deg = (int)(angle1 * (180.0 / Math.PI));
            int angle2Deg = (int)(angle2 * (180.0 / Math.PI));
            int phi = Math.Abs(angle2Deg - angle1Deg) % 360;       // This is either the distance or 360 - distance
            int distance = phi > 180 ? 360 - phi : phi;
            return distance;
        }

        public float DegreesToRads(float degrees)
        {
            return degrees * .0174533f;
        }

        private void InstantiatePositions()
        {
            //Measurements in radians
            double rightmostAngle = ImagePositions[0].GetForwardAngle();
            double leftmostAngle = ImagePositions[4].GetForwardAngle();
            double centerPoint = (leftmostAngle + rightmostAngle) / 2;
            double width = DegreesToRads(AngleBetween(leftmostAngle, rightmostAngle) * (1.215f));
            MaxAngleLeft = centerPoint + width / 2.0;
            MaxAngleRight = centerPoint - width / 2.0;
            if ((MaxAngleLeft > MaxAngleRight && MaxAngleLeft > (MaxAngleRight + 1.2)) || (MaxAngleRight > MaxAngleLeft && MaxAngleRight < (MaxAngleLeft + 1.2)))
            {
                double temp = MaxAngleRight;
                MaxAngleRight = MaxAngleLeft;
                MaxAngleLeft = temp;
            }
            float sumY = 0;
            foreach (ImagePosition ip in ImagePositions)
            {
                sumY += ip.Forward[1];
            }
            float avgY = sumY / 5;
            double height = width * .367;
            MaxHeight = avgY + height / 2;
            MinHeight = avgY - height / 2;
        }

        private void SavePositionToFile()
        {
            String printStr = "";
            for (int i = 0; i < 5; i++)
            {
                printStr += (i + 1) + ".\n";
                printStr += "Position:\n";
                printStr += "x = " + ImagePositions[i].Position[0];
                printStr += ",  y = " + ImagePositions[i].Position[1];
                printStr += ",  z = " + ImagePositions[i].Position[2];
                printStr += "\n";
                printStr += "Up:\n";
                printStr += "x = " + ImagePositions[i].Up[0];
                printStr += ",  y = " + ImagePositions[i].Up[1];
                printStr += ",  z = " + ImagePositions[i].Up[2];
                printStr += "\n";
                printStr += "Forward:\n";
                printStr += "x = " + ImagePositions[i].Forward[0];
                printStr += ",  y = " + ImagePositions[i].Forward[1];
                printStr += ",  z = " + ImagePositions[i].Forward[2] + "\n\n";
            }
            File.WriteAllText("positions.txt", printStr);
        }
    }
}
