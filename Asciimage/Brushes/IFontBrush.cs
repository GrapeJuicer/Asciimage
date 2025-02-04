using Asciimage.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Brushes
{
    public interface IFontBrush
    {
        public SKFontInfo FontInfo { get; init; }
        //public SKSize CharacterRatio { get; init; }
        //public Dictionary<SegmentCount, CharacterAreaMap> CharacterAreas { get; init; }
        //public string GetLineString(SKBitmap bitmap, SKRect area, SegmentCount seg);
        public string[,] GetStringMap(SKBitmap bitmap, int stringWidth, int stringHeight, SegmentCount seg);
    }
}
