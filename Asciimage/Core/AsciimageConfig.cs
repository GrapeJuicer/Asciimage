using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Core
{
    /// <summary>
    /// Config that specifies width, height, characters, color mode.
    /// </summary>
    public struct AsciimageConfig
    {
        /// <summary>
        /// Width of the image.
        /// If set to 0, it will be auto-adjusted to the original image width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the image.
        /// If set to 0, it will be auto-adjusted to the original image height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Color mode of the image.
        /// </summary>
        public ColorMode ColorMode;

        /// <summary>
        /// Initializes a new instance of the AsciimageConfig struct.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="colorMode">Color mode of the image.</param>
        /// <exception cref="ArgumentException">Thrown when both width and height are zero.</exception>
        public AsciimageConfig(int width = 0, int height = 0, ColorMode colorMode = ColorMode.Binary)
        {
            if (width == 0 && height == 0)
            {
                throw new ArgumentException("Either width or height must be specified.");
            }

            Width = width;
            Height = height;
            ColorMode = colorMode;
        }
    }
}
