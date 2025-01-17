using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Brushes
{
    public class CusomFontBrush : IFontBrush
    {
        public SKFontInfo FontInfo { get; init; }
        public SKSize CharacterRatio { get; init; }
        public Dictionary<SegmentCount, CharacterAreaMap> CharacterAreas { get; init; }

        public CusomFontBrush(string fontFamily, Dictionary<SegmentCount, CharacterAreaMap> characterAreas, SKSize? characterRatio = null) :
            this(SKTypeface.FromFamilyName(fontFamily), characterAreas, characterRatio) { }
        public CusomFontBrush(SKTypeface fontFace, Dictionary<SegmentCount, CharacterAreaMap> characterAreas, SKSize? characterRatio = null) :
            this(new SKFont(fontFace), characterAreas, characterRatio) { }
        public CusomFontBrush(SKFont font, Dictionary<SegmentCount, CharacterAreaMap> characterAreas, SKSize? characterRatio = null)
        {
            if (!font.Typeface.IsFixedPitch)
            {
                throw new ArgumentException("The specified font must be monospaced.");
            }

            FontInfo = new(font);
            CharacterAreas = characterAreas;
            CharacterRatio = characterRatio ?? new SKSize(font.MeasureText("a"), font.Metrics.Descent - font.Metrics.Ascent);
        }

        public CusomFontBrush(string fontFamily, IEnumerable<char> characters, IEnumerable<SegmentCount>? segmentSizes = null) :
            this(fontFamily, characters.Select(c => c.ToString()), segmentSizes) { }
        public CusomFontBrush(string fontFamily, IEnumerable<string> characters, IEnumerable<SegmentCount>? segmentSizes = null) :
            this(SKTypeface.FromFamilyName(fontFamily), characters, segmentSizes) { }
        public CusomFontBrush(SKTypeface fontFace, IEnumerable<string> characters, IEnumerable<SegmentCount>? segmentSizes = null) :
            this(new SKFont(fontFace), characters, segmentSizes) { }

        /// <summary>
        /// This class uses SkiaSharp to calcurate character segment area.
        /// And SkiaSharp doesn't support some charcters.
        /// If you want to use that characters,  calcurate their area and create instance using following constructor:
        ///     public CusomFontBrush(string fontFamily, Size characterRatio, Dictionary<SegmentCount, CharacterAreaMap> characterAreas)
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="characters"></param>
        /// <param name="segmentSizes">1x1 is used if this parameter is `null`.</param>
        public CusomFontBrush(SKFont font, IEnumerable<string> characters, IEnumerable<SegmentCount>? segmentSizes = null)
        {
            FontInfo = new(font);

            // use 1x1 if not specified
            segmentSizes ??= [SegmentCount.OneByOne];

            if (!font.Typeface.IsFixedPitch)
            {
                throw new ArgumentException("The specified font must be monospaced.");
            }

            CharacterRatio = new SKSize(font.MeasureText("a"), font.Metrics.Descent - font.Metrics.Ascent);

            segmentSizes = segmentSizes.Distinct();

            int width = (int)float.Ceiling(CharacterRatio.Width);
            int height = (int)float.Ceiling(CharacterRatio.Height);

            if (segmentSizes.Any(x => x.Horizontal > width || x.Vertical > height))
            {
                throw new ArgumentException("segmentSize is larger than actual character size. Specify smaller segmentSize or larger font size.");
            }

            CharacterAreas = [];

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
                    // 元画像がグレースケールという前提なので、HSVに変換してVを取得することで0-255での表現に変化できる
                    color.ToHsv(out _, out _, out float v);
                    values.Add(v / 100);
                }
            }

            return values.Average();
        }
    }
}
