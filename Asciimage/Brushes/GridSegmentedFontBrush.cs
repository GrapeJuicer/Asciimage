using Asciimage.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Asciimage.Brushes
{
    public class GridSegmentedFontBrush : BaseGridSegmentedFontBrush
    {
        public GridSegmentedFontBrush(string fontFamily, IEnumerable<char> characters, IEnumerable<SegmentCount>? segmentSizes = null) :
            this(fontFamily, characters.Select(c => c.ToString()), segmentSizes)
        { }
        public GridSegmentedFontBrush(string fontFamily, IEnumerable<string> characters, IEnumerable<SegmentCount>? segmentSizes = null) :
            this(SKTypeface.FromFamilyName(fontFamily), characters, segmentSizes)
        { }
        public GridSegmentedFontBrush(SKTypeface fontFace, IEnumerable<string> characters, IEnumerable<SegmentCount>? segmentSizes = null) :
            this(new SKFont(fontFace), characters, segmentSizes)
        { }

        /// <summary>
        /// This class uses SkiaSharp to calcurate character segment area.
        /// And SkiaSharp doesn't support some charcters.
        /// If you want to use that characters,  calcurate their area and create instance using following constructor:
        ///     public CusomFontBrush(string fontFamily, Size characterRatio, Dictionary<SegmentCount, CharacterAreaMap> characterAreas)
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="characters"></param>
        /// <param name="segmentSizes">1x1 is used if this parameter is `null`.</param>
        public GridSegmentedFontBrush(SKFont font, IEnumerable<string> characters, IEnumerable<SegmentCount>? segmentSizes = null) : base(font, [], null)
        {
            // use 1x1 if not specified
            segmentSizes ??= [SegmentCount.OneByOne];

            if (!font.Typeface.IsFixedPitch)
            {
                throw new ArgumentException("The specified font must be monospaced.");
            }

            segmentSizes = segmentSizes.Distinct();

            int width = (int)float.Ceiling(CharacterRatio.Width);
            int height = (int)float.Ceiling(CharacterRatio.Height);

            if (segmentSizes.Any(x => x.Horizontal > width || x.Vertical > height))
            {
                throw new ArgumentException("segmentSize is larger than actual character size. Specify smaller segmentSize or larger font size.");
            }

            var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true
            };

            Dictionary<string, Dictionary<SegmentCount, double[,]>> segMap = [];

            foreach (string s in characters)
            {
                using SKBitmap workBitmap = new(width, height);
                using SKCanvas workCanvas = new(workBitmap);

                workCanvas.Clear(SKColors.Black);
                workCanvas.DrawText(s, 0f, -font.Metrics.Ascent, font, paint);

                Dictionary<SegmentCount, double[,]> area = segmentSizes.ToDictionary(seg => seg, seg => CalculateSegmentArea(workBitmap, seg));

                segMap.Add(s, area);
            }

            foreach (var seg in segmentSizes)
            {
                CharacterAreas.Add(seg, new(seg, segMap.ToDictionary(x => x.Key, x => x.Value[seg])));
            }
        }

        private static double[,] CalculateSegmentArea(SKBitmap bitmap, SegmentCount segmentSize)
        {
            float w = (float)bitmap.Width / segmentSize.Horizontal;
            float h = (float)bitmap.Height / segmentSize.Vertical;

            double[,] segmentArea = new double[segmentSize.Vertical, segmentSize.Horizontal];

            int yMin = 0;
            for (int j = 0; j < segmentSize.Vertical; j++)
            {
                int xMin = 0;
                int yMax = (int)float.Floor((j + 1) * h);

                for (int i = 0; i < segmentSize.Horizontal; i++)
                {
                    // each segment
                    int xMax = (int)float.Floor((i + 1) * w);

                    // calculate mean depth
                    segmentArea[j, i] = GetSegmentMeanColorDepth(bitmap, xMin, xMax, yMin, yMax);

                    // set next xMin
                    xMin = xMax;
                }

                // set next yMin
                yMin = yMax;
            }

            return segmentArea;
        }

        /// <summary>
        /// xMin <= x < xMax, yMin <= y < yMax is segment range
        /// </summary>
        /// <param name="bitmap">Source bitmap</param>
        /// <param name="xMin">Segment x min</param>
        /// <param name="xMax">Segment x max (not includes xMax cell)</param>
        /// <param name="yMin">Segment y min</param>
        /// <param name="yMax">Segment y max (not includes yMax cell)</param>
        /// <returns>Mean segment color depth</returns>
        private static double GetSegmentMeanColorDepth(SKBitmap bitmap, int xMin, int xMax, int yMin, int yMax)
        {
            List<double> values = [];

            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    SKColor color = bitmap.GetPixel(x, y);
                    // Assuming the original image is grayscale, convert to HSV and get V to represent it in the range of 0-255
                    color.ToHsv(out _, out _, out float v);
                    values.Add(v / 100);
                }
            }

            return values.Average();
        }

        protected override string GetSegmentCharacter(SKBitmap bitmap, SKRectI rect, SegmentCount seg)
        {
            var areaMap = CharacterAreas[seg].RelativeAreaMap;

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
                            SKColor color = bitmap.GetPixel(i, j);
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
                        double d = area[y, x] - depthMap[y, x];
                        diff += d == 0 ? 0 : d * d;
                    }
                }

                diff /= seg.Vertical * seg.Horizontal;

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
