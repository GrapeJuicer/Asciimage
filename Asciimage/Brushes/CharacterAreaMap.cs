using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Brushes
{
    public class CharacterAreaMap
    {
        public SegmentCount SegmentSize { get; init; }
        /// <summary>
        /// Dictionary<character, areamap[vSegmentCnt, hSegmentCnt]>
        ///     character: 1 character. multi-byte character is supported.
        /// {
        ///     "A": [  [  0, 0.7,   0]
        ///             [0.3, 0.2, 0.3]
        ///             [0.7, 0.6, 0.7]
        ///             [0.9, 0.2, 0.9]  ],
        ///     ...
        /// }
        /// </summary>
        public Dictionary<string, double[,]> AbsoluteAreaMap { get; init; }
        public Dictionary<string, double[,]> RelativeAreaMap { get; init; }

        public CharacterAreaMap(SegmentCount segmentSize, Dictionary<string, double[,]> absoluteAreaMap) : this(segmentSize, absoluteAreaMap, CalculateRelativeAreaMap(absoluteAreaMap)) { }
        public CharacterAreaMap(SegmentCount segmentSize, Dictionary<string, double[,]> absoluteAreaMap, Dictionary<string, double[,]> relativeAreaMap)
        {
            SegmentSize = segmentSize;

            // segment count とサイズが異なるものがある場合
            if (absoluteAreaMap.Values.Any(x => x.GetLength(0) != SegmentSize.Vertical || x.GetLength(1) != SegmentSize.Horizontal) ||
                relativeAreaMap.Values.Any(x => x.GetLength(0) != SegmentSize.Vertical || x.GetLength(1) != SegmentSize.Horizontal))
            {
                throw new ArgumentException("All `absoluteAreaMap` values must have [verticalSegmentCount, horizontalSegmentCount] array.");
            }

            AbsoluteAreaMap = absoluteAreaMap;
            RelativeAreaMap = relativeAreaMap;
        }

        public static Dictionary<string, double[,]> CalculateRelativeAreaMap(Dictionary<string, double[,]> absoluteAreaMap)
        {
            double maxValue = absoluteAreaMap.Values.SelectMany(array => array.Cast<double>()).Max();

            return absoluteAreaMap.ToDictionary(
                x => x.Key,
                x =>
                {
                    double[,] relativeValues = (double[,])x.Value.Clone();
                    for (int i = 0; i < relativeValues.GetLength(0); i++)
                    {
                        for (int j = 0; j < relativeValues.GetLength(1); j++)
                        {
                            relativeValues[i, j] /= maxValue;
                        }
                    }
                    return relativeValues;
                }
            );
        }
    }
}
