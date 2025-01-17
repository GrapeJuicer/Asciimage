using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Brushes
{
    public readonly struct SKFontInfo(SKFont font)
    {
        public string FamilyName { get; init; } = font.Typeface.FamilyName;
        public float Size { get; init; } = font.Size;
        public SKFontStyleSlant Slant { get; init; } = font.Typeface.FontSlant;
        public int Weight { get; init; } = font.Typeface.FontWeight;
        public int Width { get; init; } = font.Typeface.FontWidth;
        public float ScaleX { get; init; } = font.ScaleX;
        public float SkewX { get; init; } = font.SkewX;
    }
}
