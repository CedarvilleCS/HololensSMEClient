using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET;
using System.Drawing;

namespace PDFToImage
{
    public class PDFManager
    {

        /// <summary>
        /// Stores the version of the Ghostscript DLL to use with Ghostscript.NET
        /// </summary>
        private static GhostscriptVersionInfo version = Ghostscript.NET.GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.GPL | GhostscriptLicense.AFPL, 
                                                                                                                       GhostscriptLicense.GPL);

        /// <summary>
        /// DPI to render PDF images
        /// </summary>
        private const int DPI = 200;

        /// <summary>
        /// Caches PDF files that have been loaded from disk
        /// </summary>
        private static Dictionary<string, GhostscriptRasterizer> cache = new Dictionary<string, GhostscriptRasterizer>();

        /// <summary>
        /// Keeps track of the order items arrive into the cache and their sizes
        /// </summary>
        private static Queue<Tuple<string, long>> fifo = new Queue<Tuple<string, long>>();

        /// <summary>
        /// Max cache size in bytes
        /// </summary>
        public static long maxCacheSize { get; set; } = 100000000;

        /// <summary>
        /// Current cache size
        /// </summary>
        private static long cacheSize;

        /// <summary>
        /// Keeps track of when the first rasterizer is created
        /// </summary>
        private static bool firstRasterizer = true;

        /// <summary>
        /// Retrive a page of a PDF as an image
        /// </summary>
        /// <param name="filename">Path to the PDF document file</param>
        /// <param name="page">Page number to retrive as an image</param>
        /// <returns>Byte array containing bitmap image data</returns>
        public static Image getImage(string filename, int page)
        {
            GhostscriptRasterizer rasterizer = loadPdf(filename);

            return rasterizer.GetPage(DPI, DPI, page);
        }

        public static int getNumPages(string filename)
        {
            GhostscriptRasterizer rasterizer = loadPdf(filename);

            return rasterizer.PageCount;
        }

        private static GhostscriptRasterizer loadPdf(string filename)
        {
            GhostscriptRasterizer pdf;

            bool cached = cache.TryGetValue(filename, out pdf);

            //
            // If the file is not in the cache add it to the cache
            //
            if (!cached)
            {
                FileInfo info = new FileInfo(filename);

                //
                // Keep the cache from getting to big
                //
                long overflow = cacheSize + info.Length - maxCacheSize;
                while (overflow > 0)
                {
                    Tuple<string, long> cacheItem = fifo.Dequeue();
                    cache.Remove(cacheItem.Item1);
                    overflow -= cacheItem.Item2;
                }

                pdf = new GhostscriptRasterizer();
                pdf.Open(filename, version, true);

                //
                // Keep track of the cache size and the
                // size of each item.
                //
                cacheSize += info.Length;
                fifo.Enqueue(new Tuple<string, long>(filename, info.Length));
            }

            return pdf;
        }
    }
}
