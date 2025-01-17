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
        public SKSize CharacterRatio { get; init; }
        public Dictionary<SegmentCount, CharacterAreaMap> CharacterAreas { get; init; }
    }
}
