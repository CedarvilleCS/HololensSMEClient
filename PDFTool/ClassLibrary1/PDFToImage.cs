using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using libpdf;

namespace PDFToImage
{
    public class PDFManager
    {
        /// <summary>
        /// Caches PDF files that have been loaded from disk
        /// </summary>
        private static Dictionary<string, LibPdf> cache = new Dictionary<string, LibPdf>();

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
        /// Retrive a page of a PDF as an image
        /// </summary>
        /// <param name="filename">Path to the PDF document file</param>
        /// <param name="page">Page number to retrive as an image</param>
        /// <returns>Byte array containing bitmap image data</returns>
        public static byte[] getImage(string filename, int page)
        {
            LibPdf pdf; 

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
                while(overflow > 0)
                {
                    Tuple<string, long> cacheItem = fifo.Dequeue();
                    cache.Remove(cacheItem.Item1);
                    overflow -= cacheItem.Item2;
                }

                byte[] b = File.ReadAllBytes(filename);
                pdf = new LibPdf(b);
                cache.Add(filename, pdf);

                //
                // Keep track of the cache size and the
                // size of each item.
                //
                cacheSize += b.Length;
                fifo.Enqueue(new Tuple<string, long>(filename, info.Length));
            }

            return pdf.GetImage(page, ImageType.BMP);
        }
    }
}
