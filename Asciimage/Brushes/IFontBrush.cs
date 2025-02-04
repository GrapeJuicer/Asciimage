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
        public string[,] GetStringMap(SKBitmap bitmap, int stringWidth, int stringHeight, SegmentCount seg);
    }
}
