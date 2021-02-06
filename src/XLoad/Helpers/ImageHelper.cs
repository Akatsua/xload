namespace XLoad.Helpers
{
    using Dto;
    using Image;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public static class ImageHelper
    {
        public static void WriteImage(
            Config config,
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
            int maxTimeToPrint = data.Length > 2 * 60 * 60 ? 2 * 60 * 60 : data.Length;

            const int padTop    = 10;
            const int padBottom = 10;
            const int tickSize  = 1;

            int maxValue        = data.Max();
            int maxCharNumber   = maxValue.ToString().Length;

            int padLeft = maxCharNumber * 5 + 1;

            Rectangle image = new Rectangle(0, 0, padLeft + data.Length, padTop + maxValue + padBottom);
            Rectangle graph = new Rectangle(padLeft, padBottom, data.Length, maxValue);

            var bitmap = new Bitmap((uint)image.Width, (uint)image.Height, true);

            // Background
            PlaceRectangle(bitmap, image, Color.White);

            // Axis
            PlaceLine(bitmap, new Point(graph.X, graph.Y), new Point(graph.X + graph.Width - 1, graph.Y), Color.Black);
            PlaceLine(bitmap, new Point(graph.X, graph.Y), new Point(graph.X, graph.Y + graph.Height - 1), Color.Black);

            PlaceNumber(bitmap, "0", graph.X - 6, graph.Y - 6);

            // Horizontal Ticks
            for (int i = resolution; i < maxTimeToPrint - 1; i+= resolution)
            {
                PlaceLine(bitmap, new Point(graph.X + i, graph.Y), new Point(graph.X + i, graph.Y + tickSize), Color.Black);
            }

            // Vertical Ticks
            int vTickSpacing = (int)Math.Floor(graph.Height / 10.0f);

            for (int i = 1; i <= 10; i++)
            {
                PlaceLine(bitmap, new Point(graph.X, graph.Y + vTickSpacing * i), new Point(graph.X + graph.Width - 1, graph.Y + vTickSpacing * i), Color.FromArgb(245, 245, 255));
                PlaceLine(bitmap, new Point(graph.X, graph.Y + vTickSpacing * i), new Point(graph.X + tickSize, graph.Y + vTickSpacing * i), Color.Black);
            }

            // Vertical Numbers

            for (int i = 1; i <= 10; i++)
            {
                var value = Math.Floor((maxValue / 10.0f) * i);
                PlaceNumber(bitmap, value.ToString(), (maxCharNumber - value.ToString().Length) * 5, graph.Y + vTickSpacing * i - 2);
            }

            // Data
            for (int i = 0; i < data.Length; i++)
            {
                if (i + 1 < data.Length)
                {
                    var toPaint = GetPointsOnLine(i, data[i], i + 1, data[i + 1]);

                    foreach (var item in toPaint)
                    {
                        bitmap.SetPixel(item.X + graph.X, item.Y + graph.Y, Color.Black);
                    }
                }
            }

            bitmap.Save(file);
        }

        private static void PlaceRectangle(Bitmap bitmap, Rectangle rectangle, Color color)
        {
            for (int x = 0; x < rectangle.Width; x++)
            {
                for (int y = 0; y < rectangle.Height; y++)
                {
                    bitmap.SetPixel(x + rectangle.X, y + rectangle.Y, Color.White);
                }
            }
        }

        private static void PlaceLine(Bitmap bitmap, Point p1, Point p2, Color color)
        {
            foreach (var point in GetPointsOnLine(p1.X, p1.Y, p2.X, p2.Y))
            {
                bitmap.SetPixel(point.X, point.Y, color);
            }
        }

        private static void PlaceNumber(Bitmap bitmap, string number, int cornerX, int cornerY)
        {
            for (int i = 0; i < number.Length; i++)
            {
                PlaceDigit(bitmap, number[i], cornerX + (i * 5), cornerY);
            }
        }

        private static void PlaceDigit(Bitmap bitmap, char letter, int cornerX, int cornerY)
        {
            byte[] rectangle = new byte[20];

            Font.Get4x5PointArray(letter, ref rectangle, true);

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if ((rectangle[(4 * y) + x] & 1) == 1)
                    {
                        bitmap.SetPixel(cornerX + x, cornerY + y, Color.Blue);
                    }
                }
            }
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
