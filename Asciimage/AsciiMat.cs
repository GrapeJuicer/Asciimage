using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Asciimage
{
    public class AsciiMat
    {
        /// <summary>
        /// Get the ASCII art character width.
        /// </summary>
        public int Width { get { return Ascii.GetLength(1); } }
        /// <summary>
        /// Get the ASCII art character height.
        /// </summary>
        public int Height { get { return Ascii.GetLength(0); } }
        /// <summary>
        /// Get the characters used in the ASCII art.
        /// </summary>
        public HashSet<char> Characters
        {
            get
            {
                HashSet<char> result = [];
                foreach (char c in Ascii)
                {
                    result.Add(c);
                }
                return result;
            }
        }
        /// <summary>
        /// Get the ASCII art.
        /// </summary>
        public char[,] Ascii { get; }
        /// <summary>
        /// Color information.
        /// null: AsciiMat does not have color information. Binary or Grayscale only.
        /// </summary>
        public ConsoleColor[,]? ForegroundColorMap { get; }
        public ConsoleColor[,]? BackgroundColorMap { get; }

        /// <summary>
        /// Create ASCII art image instance.
        /// `foregroundColorMap` must be specify if you specified `backgourndColorMap`.
        /// </summary>
        /// <param name="mat">Character map</param>
        /// <param name="foregroundColorMap">Foreground color map to display on commandline</param>
        /// <param name="backgroundColorMap">Background color map to display on commandline</param>
        /// <exception cref="ArgumentException"></exception>
        public AsciiMat(char[,] mat, ConsoleColor[,]? foregroundColorMap = null, ConsoleColor[,]? backgroundColorMap = null)
        {
            int matHeight = mat.GetLength(0);
            int matWidth = mat.GetLength(1);

            if (matHeight == 0 || matWidth == 0)
            {
                throw new ArgumentException("'mat' must have at least one character.");
            }

            if (foregroundColorMap != null)
            {
                if (matHeight != foregroundColorMap.GetLength(0) || matWidth != foregroundColorMap.GetLength(1))
                {
                    throw new ArgumentException("'mat' and 'foregroundColorMap' must be the same size.");
                }

                if (backgroundColorMap != null && (matHeight != backgroundColorMap.GetLength(0) || matWidth != backgroundColorMap.GetLength(1)))
                {
                    throw new ArgumentException("'mat' and 'backgroundColorMap' must be the same size.");
                }
            }
            else if (backgroundColorMap != null)
            {
                throw new ArgumentException("Foreground must be set to use background color");
            }

            Ascii = mat;
            ForegroundColorMap = foregroundColorMap;
        }

        public string ToString(bool asColor = false)
        {
            int width = Height;
            int height = Width;
            string result = string.Empty;

            if (asColor && ForegroundColorMap == null)
            {
                throw new NotSupportedException("This image does not have color map.");
            }

            for (int row = 0; row < width; row++)
            {
                for (int col = 0; col < height; col++)
                {
                    if (asColor && ForegroundColorMap != null)
                    {
                        // add foreground ANSI escape sequence
                        result += ForegroundColorMap[row, col].ToANSIForegroundColor();

                        if (BackgroundColorMap != null)
                        {
                            // add background ANSI escape sequence
                            result += BackgroundColorMap[row, col].ToANSIForegroundColor();
                        }
                    }

                    result += Ascii[row, col];
                }

                if (row < width - 1)
                {
                    result += '\n';
                }
            }

            if (asColor && ForegroundColorMap != null)
            {
                result += ((ConsoleColor)(-1)).ToANSIBackgroundColor() + ((ConsoleColor)(-1)).ToANSIBackgroundColor();
            }

            return result;
        }
    }
}
