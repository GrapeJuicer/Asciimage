using Asciimage.Brushes;
using SkiaSharp;
using System.Diagnostics;

namespace Asciimage.Core
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

            return new AsciiMat(brush.GetStringMap(localImage, config.Width, config.Height, seg));
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
    }
}
