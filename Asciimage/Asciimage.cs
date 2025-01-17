using Asciimage.Brushes;
using SkiaSharp;
using System.Diagnostics;

namespace Asciimage
{
    public static class Asciimage
    {
        public static AsciiMat Generate(SKBitmap image, IFontBrush brush, SegmentCount seg, AsciimageConfig config)
        {
            // Define a local image for processing
            using SKBitmap localImage = image.Copy();

            Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Copied");


            // Process the image based on the color mode
            switch (config.ColorMode)
            {
                case ColorMode.Binary:
                    BinarizeImage(localImage, 127);
                    break;
                case ColorMode.Grayscale:
                    GrayscaleImage(localImage);
                    break;
                default:
                    throw new NotSupportedException("Color mode not supported.");
            }

            Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} ColorMode Applied");

            int horizontalSegmentCount, verticalSegmentCount;

            if ((config.Width == 0 && config.Height == 0) || config.Width < 0 || config.Height < 0)
            {
                throw new ArgumentException("Invalid config.");
            }
            else if (config.Height == 0)
            {
                double segmentWidth = (double)localImage.Width / config.Width;
                double segmentHeight = segmentWidth * brush.CharacterRatio.Height / brush.CharacterRatio.Width;
                verticalSegmentCount = (int)Math.Round(localImage.Height / segmentHeight);
                horizontalSegmentCount = config.Width;
            }
            else if (config.Width == 0)
            {
                double segmentHeight = (double)localImage.Height / config.Height;
                double segmentWidth = segmentHeight * brush.CharacterRatio.Width / brush.CharacterRatio.Height;
                horizontalSegmentCount = (int)Math.Round(localImage.Width / segmentWidth);
                verticalSegmentCount = config.Height;
            }
            else
            {
                horizontalSegmentCount = config.Width;
                verticalSegmentCount = config.Height;
            }

            Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Segment Counted");

            // Divide the image by config.Width and config.Height, and repeat the following process for each region
            double cellWidth = (double)localImage.Width / horizontalSegmentCount;
            double cellHeight = (double)localImage.Height / verticalSegmentCount;

            string[,] asciiArt = new string[verticalSegmentCount, horizontalSegmentCount];

            Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Loop Start");
            bool first = true;

            for (int y = 0; y < verticalSegmentCount; y++)
            {
                for (int x = 0; x < horizontalSegmentCount; x++)
                {
                    if (first) Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} For Item Start");
                    int xMin = (int)Math.Floor(cellWidth * x);
                    int xMax = (int)Math.Floor(cellWidth * (x + 1));
                    int yMin = (int)Math.Floor(cellHeight * y);
                    int yMax = (int)Math.Floor(cellHeight * (y + 1));
                    if (first) Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} For Item Rect");

                    // Define the region of interest (ROI)
                    SKRectI roi = new(xMin, yMin, xMax - 1, yMax - 1);
                    if (first) Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} For Item Bitmap & Canvas");

                    // Extract the cell from the localImage
                    using SKBitmap cell = new(roi.Width, roi.Height);
                    using SKCanvas canvas = new(cell);
                    if (first) Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} For Item Draw");
                    canvas.DrawBitmap(localImage, roi, new SKRect(0, 0, roi.Width, roi.Height));

                    if (first) Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} For Similar Start");

                    asciiArt[y, x] = GetSimilarCharacter(cell, brush, seg);

                    if (first) Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} For Item End");
                    first = false;
                }
            }

            Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Loop End");

            return new AsciiMat(asciiArt);
        }

        private static void BinarizeImage(SKBitmap image, byte threshold)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {

                    SKColor color = image.GetPixel(x, y);
                    byte brightness = (byte)(0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
                    byte binaryColor = brightness > threshold ? (byte)255 : (byte)0;
                    image.SetPixel(x, y, new SKColor(binaryColor, binaryColor, binaryColor));
                }
            }
        }

        private static void GrayscaleImage(SKBitmap image)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    SKColor color = image.GetPixel(x, y);
                    byte brightness = (byte)(0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
                    image.SetPixel(x, y, new SKColor(brightness, brightness, brightness));
                }
            }
        }

        private static string GetSimilarCharacter(SKBitmap segmentBitmap, IFontBrush brush, SegmentCount seg)
        {
            var areaMap = brush.CharacterAreas[seg].RelativeAreaMap;

            double[,] depthMap = new double[seg.Vertical, seg.Horizontal];

            double w = (double)segmentBitmap.Width / seg.Horizontal;
            double h = (double)segmentBitmap.Height / seg.Vertical;

            int iMin = 0, jMin = 0;
            int iMax, jMax;

            for (int y = 0; y < seg.Vertical; y++)
            {
                jMax = (int)Math.Floor(h * (y + 1));
                iMin = 0;

                for (int x = 0; x < seg.Horizontal; x++)
                {
                    double totalBrightness = 0;
                    int pixelCount = 0;

                    iMax = (int)Math.Floor(w * (x + 1));

                    for (int j = jMin; j < jMax; j++)
                    {
                        for (int i = iMin; i < iMax; i++)
                        {
                            SKColor color = segmentBitmap.GetPixel(i, j);
                            color.ToHsv(out _, out _, out float v);
                            totalBrightness += v / 100;
                            pixelCount++;
                        }
                    }

                    depthMap[y, x] = totalBrightness / pixelCount;

                    iMin = iMax;
                }

                jMin = jMax;
            }

            double minDiff = double.MaxValue;
            string bestMatch = string.Empty;

            foreach (var kvp in areaMap)
            {
                double[,] area = kvp.Value;
                double diff = 0;

                for (int y = 0; y < seg.Vertical; y++)
                {
                    for (int x = 0; x < seg.Horizontal; x++)
                    {
                        diff += Math.Pow(area[y, x] - depthMap[y, x], 2);
                    }
                }

                diff /= (seg.Vertical * seg.Horizontal);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestMatch = kvp.Key;
                }
            }

            if (bestMatch == string.Empty)
            {
                throw new InvalidOperationException("No match found.");
            }

            return bestMatch;
        }
    }
}
