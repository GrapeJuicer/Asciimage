using Asciimage.Brushes;
using SkiaSharp;
using System.Diagnostics;

namespace Asciimage
{
    /// <summary>
    /// Provides methods for generating ASCII art from images.
    /// </summary>
    public static class Asciimage
    {
        /// <summary>
        /// Generates an ASCII art representation of the given image.
        /// </summary>
        /// <param name="image">The input image to be converted to ASCII art.</param>
        /// <param name="brush">The font brush used to determine character ratios and areas.</param>
        /// <param name="seg">The segment count defining the granularity of the ASCII art.</param>
        /// <param name="config">The configuration settings for the ASCII art generation.</param>
        /// <returns>An AsciiMat object containing the generated ASCII art.</returns>
        /// <exception cref="ArgumentException">Thrown when the configuration is invalid.</exception>
        /// <exception cref="NotSupportedException">Thrown when the color mode is not supported.</exception>
        public static AsciiMat Generate(SKBitmap image, IFontBrush brush, SegmentCount seg, AsciimageConfig config)
        {
            // Define a local image for processing
            using SKBitmap localImage = image.Copy();

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

            // Calculate the number of segments
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

            // Divide the image by config.Width and config.Height, and repeat the following process for each region
            double cellWidth = (double)localImage.Width / horizontalSegmentCount;
            double cellHeight = (double)localImage.Height / verticalSegmentCount;

            string[,] asciiArt = new string[verticalSegmentCount, horizontalSegmentCount];

            // Iterate over each region
            for (int y = 0; y < verticalSegmentCount; y++)
            {
                for (int x = 0; x < horizontalSegmentCount; x++)
                {
                    int xMin = (int)Math.Floor(cellWidth * x);
                    int xMax = (int)Math.Floor(cellWidth * (x + 1));
                    int yMin = (int)Math.Floor(cellHeight * y);
                    int yMax = (int)Math.Floor(cellHeight * (y + 1));

                    // Define the region of interest (ROI)
                    SKRectI roi = new(xMin, yMin, xMax - 1, yMax - 1);

                    // Get the character that best represents the region
                    asciiArt[y, x] = GetSimilarCharacter(localImage, roi, brush, seg);
                }
            }

            return new AsciiMat(asciiArt);
        }

        /// <summary>
        /// Converts the given image to a binary image using the specified threshold.
        /// </summary>
        /// <param name="image">The image to be binarized.</param>
        /// <param name="threshold">The threshold value used to binarize the image.</param>
        private static void BinarizeImage(SKBitmap image, byte threshold)
        {
            int width = image.Width;
            int height = image.Height;
            SKColor[] pixels = new SKColor[width * height];
            image.Pixels.CopyTo(pixels, 0);
            
            Parallel.For(0, height, y =>
            {
                int offset = y * width;
                for (int x = 0; x < width; x++)
                {
                    SKColor color = pixels[offset + x];
                    byte brightness = (byte)(0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
                    byte binaryColor = brightness > threshold ? (byte)255 : (byte)0;
                    pixels[offset + x] = new SKColor(binaryColor, binaryColor, binaryColor);
                }
            });
            image.Pixels = pixels;
        }

        /// <summary>
        /// Converts the given image to a grayscale image.
        /// </summary>
        /// <param name="image">The image to be converted to grayscale.</param>
        private static void GrayscaleImage(SKBitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            SKColor[] pixels = new SKColor[width * height];
            image.Pixels.CopyTo(pixels, 0);

            Parallel.For(0, height, y =>
            {
                int offset = y * width;
                for (int x = 0; x < width; x++)
                {
                    SKColor color = pixels[offset + x];
                    byte brightness = (byte)(0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
                    pixels[offset + x] = new SKColor(brightness, brightness, brightness);
                }
            });

            image.Pixels = pixels;
        }

        /// <summary>
        /// Gets the character that best represents the given region of the image.
        /// </summary>
        /// <param name="segmentBitmap">The image segment to be analyzed.</param>
        /// <param name="rect">The region of interest (ROI) in the image segment.</param>
        /// <param name="brush">The font brush used to determine character ratios and areas.</param>
        /// <param name="seg">The segment count defining the granularity of the ASCII art.</param>
        private static string GetSimilarCharacter(SKBitmap segmentBitmap, SKRectI rect, IFontBrush brush, SegmentCount seg)
        {
            var areaMap = brush.CharacterAreas[seg].RelativeAreaMap;

            double[,] depthMap = new double[seg.Vertical, seg.Horizontal];

            double w = (double)rect.Width / seg.Horizontal;
            double h = (double)rect.Height / seg.Vertical;

            int iMin, jMin = rect.Top;
            int iMax, jMax;

            for (int y = 0; y < seg.Vertical; y++)
            {
                jMax = (int)Math.Floor(h * (y + 1)) + rect.Top;
                iMin = rect.Left;

                for (int x = 0; x < seg.Horizontal; x++)
                {
                    double totalBrightness = 0;
                    int pixelCount = 0;

                    iMax = (int)Math.Floor(w * (x + 1)) + rect.Left;

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
