namespace XLoad.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    public static class ImageHelper
    {
        public static void WriteImage(
            Dto.Config config,
            List<int> data)
        {
            if (!string.IsNullOrWhiteSpace(config.Diagnostic.Image))
            {
                try
                {
                    SaveBitmap(config.Diagnostic.Image, data.ToArray(), config.Noise.Resolution.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] A unexpected error occured while generating the sample image ({ex}).");
                }
            }
        }

        private static void SaveBitmap(
            string file,
            int[] data,
            int resolution)
        {
            int height = data.Max() + 1;
            int height2 = data.Max();

            Bitmap bmp = new Bitmap(data.Length, height, PixelFormat.Format24bppRgb);

            // Background
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(x, y, Color.White);
                }
            }

            // Hour marks
            for (int i = 0; i < data.Length; i += (60 * 60 / resolution))
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(i, y, Color.OrangeRed);
                }
            }

            // Message marks
            for (int i = 0; i < height; i += height2 / 20)
            {
                for (int x = 0; x < data.Length; x++)
                {
                    bmp.SetPixel(x, height2 - i, Color.DarkRed);
                }
            }

            // Data
            for (int i = 0; i < data.Length; i++)
            {
                if (i + 1 < data.Length)
                {
                    var toPaint = GetPointsOnLine(i, height2 - data[i], i + 1, height2 - data[i + 1]);

                    foreach (var item in toPaint)
                    {
                        bmp.SetPixel(item.X, item.Y, Color.Black);
                    }
                }

                bmp.SetPixel(i, height2 - data[i], Color.Green);
            }

            bmp.Save(file);
        }

        // From Eric Woroshow
        // http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
        // Under MIT License http://ericw.ca/license.html
        // Edited to swap using tuples, Akatsua, 2021
        private static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            
            if (steep)
            {
                (x0, y0) = (y0, x0);
                (x1, y1) = (y1, x1);
            }
            
            if (x0 > x1)
            {
                (x0, x1) = (x1, x0);
                (y0, y1) = (y1, y0);
            }
            
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;

            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return new Point((steep ? y : x), (steep ? x : y));
                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }
    }
}
