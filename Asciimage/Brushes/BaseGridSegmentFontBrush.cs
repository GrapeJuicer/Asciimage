using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Asciimage.Brushes
{
    public abstract class BaseGridSegmentedFontBrush(SKFont font, Dictionary<SegmentCount, CharacterAreaMap> characterAreas, SKSize? characterRatio = null) : IFontBrush
    {
        public SKFontInfo FontInfo { get; init; } = new SKFontInfo(font);
        public Dictionary<SegmentCount, CharacterAreaMap> CharacterAreas { get; init; } = characterAreas;
        public SKSize CharacterRatio { get; init; } = characterRatio ?? new(font.MeasureText("A"), font.Metrics.Descent - font.Metrics.Ascent);

        public BaseGridSegmentedFontBrush(string fontFamily, Dictionary<SegmentCount, CharacterAreaMap> characterAreas, SKSize? characterRatio = null) :
            this(SKTypeface.FromFamilyName(fontFamily), characterAreas, characterRatio)
        { }
        public BaseGridSegmentedFontBrush(SKTypeface fontFace, Dictionary<SegmentCount, CharacterAreaMap> characterAreas, SKSize? characterRatio = null) :
            this(new SKFont(fontFace), characterAreas, characterRatio)
        { }

        protected abstract string GetSegmentCharacter(SKBitmap bitmap, SKRectI rect, SegmentCount seg);
        
        protected void ConvertToGridSegmentedSize(SKBitmap bitmap, int width, int height, out int gridSegmentWidth, out int gridSegmentHeight)
        {
            // Calculate the number of segments
            int gsw, gsh;
            if (width == -1 && height == -1 || width == 0 || height == 0)
            {
                throw new ArgumentException("Invalid config.");
            }
            else if (height == -1)
            {
                double segmentWidth = (double)bitmap.Width / width;
                double segmentHeight = segmentWidth * CharacterRatio.Height / CharacterRatio.Width;
                gsh = (int)Math.Round(bitmap.Height / segmentHeight);
                gsw = width;
            }
            else if (width == -1)
            {
                double segmentHeight = (double)bitmap.Height / height;
                double segmentWidth = segmentHeight * CharacterRatio.Width / CharacterRatio.Height;
                gsw = (int)Math.Round(bitmap.Width / segmentWidth);
                gsh = height;
            }
            else
            {
                gsw = width;
                gsh = height;
            }

            gridSegmentWidth = gsw;
            gridSegmentHeight = gsh;
        }

        public string[,] GetStringMap(SKBitmap bitmap, int stringWidth, int stringHeight, SegmentCount seg)
        {
            ConvertToGridSegmentedSize(bitmap, stringWidth, stringHeight, out int gridSegmentWidth, out int gridSegmentHeight);

            // Divide the image by config.Width and config.Height, and repeat the following process for each region
            double cellWidth = (double)bitmap.Width / gridSegmentWidth;
            double cellHeight = (double)bitmap.Height / gridSegmentHeight;

            string[,] asciiArt = new string[gridSegmentHeight, gridSegmentWidth];

            // Iterate over each region
            for (int y = 0; y < gridSegmentHeight; y++)
            {
                int yMin = (int)Math.Floor(cellHeight * y);
                int yMax = (int)Math.Floor(cellHeight * (y + 1));

                for (int x = 0; x < gridSegmentWidth; x++)
                {
                    int xMin = (int)Math.Floor(cellWidth * x);
                    int xMax = (int)Math.Floor(cellWidth * (x + 1));

                    // Define the region of interest (ROI)
                    SKRectI roi = new(xMin, yMin, xMax - 1 <= xMin ? xMin + 1 : xMax - 1, yMax - 1 <= yMin ? yMin + 1 : yMax - 1);

                    // Get the character that best represents the region
                    asciiArt[y, x] = GetSegmentCharacter(bitmap, roi, seg);
                }
            }

            return asciiArt;
        }
    }
}
