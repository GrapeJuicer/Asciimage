using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Brushes
{
    public readonly struct SegmentCount(int vertical, int horizontal)
    {
        public readonly int Vertical = vertical;
        public readonly int Horizontal = horizontal;

        public static readonly SegmentCount OneByOne = new(1, 1);
        public static readonly SegmentCount TwoByOne = new(2, 1);
        public static readonly SegmentCount TwoByTwo = new(2, 2);
        public static readonly SegmentCount FourByTwo = new(4, 2);
        public static readonly SegmentCount FourByFour = new(4, 4);

        public string ToString(bool withLabel = true)
        {
            return withLabel ? $"(Vertical: {Vertical}, Horizontal: {Horizontal})" : $"({Vertical}, {Horizontal})";
        }
    }
}
