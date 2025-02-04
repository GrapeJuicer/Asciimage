using Asciimage.Brushes;
using OpenCvSharp;
using SkiaSharp;
using System.Diagnostics;

namespace AsciimageTester
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SKFont font = new()
            {
                Typeface = SKTypeface.FromFamilyName("Cascadia Code"),
                Size = 12,
            };

            List<string> ss = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".Select(x => x.ToString()).ToList();
            SegmentCount seg = new(14, 8);
            List<SegmentCount> segs = [SegmentCount.OneByOne, SegmentCount.TwoByTwo, SegmentCount.FourByTwo, SegmentCount.FourByFour, seg];

            GridSegmentedFontBrush brush = new(font, ss, segs);

            int pointSize = 10;
            int LineCount = 10;
            int margin = 50;

            var areaMap = brush.CharacterAreas[seg];

            int Bw = LineCount * (pointSize * areaMap.SegmentSize.Horizontal + margin) + margin;
            int Bh = (areaMap.RelativeAreaMap.Count / LineCount) * (pointSize * areaMap.SegmentSize.Vertical + margin) + margin;

            var bitmap = new SKBitmap(Bw, Bh);
            var canvas = new SKCanvas(bitmap);

            DrawDepth(canvas, areaMap, pointSize, LineCount, margin);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Desktop\asciimage-test1.png"));
            data.SaveTo(stream);
        }

        public static void DrawDepth(SKCanvas canvas, CharacterAreaMap areaMap, int pointSize = 10, int LineCount = 10, int margin = 50)
        {
            canvas.Clear(SKColors.Blue);

            var keys = areaMap.RelativeAreaMap.Keys.ToArray();

            for (int i = 0; i < areaMap.RelativeAreaMap.Count; i++)
            {
                for (int y = 0; y < areaMap.SegmentSize.Vertical; y++)
                {
                    for (int x = 0; x < areaMap.SegmentSize.Horizontal; x++)
                    {
                        canvas.DrawRect(
                        margin + (i % LineCount) * (areaMap.SegmentSize.Horizontal * pointSize + margin)/*(0,0)までのOffset)*/ + x * pointSize,
                        margin + (i / LineCount) * (areaMap.SegmentSize.Vertical * pointSize + margin)/*(0,0)までのOffset)*/ + y * pointSize,
                        pointSize,
                        pointSize,
                        new() { Color = SKColor.FromHsv(0, 0, float.Round((float)(100 * areaMap.RelativeAreaMap[keys[i]][y, x]))) }
                        );
                    }
                }
            }
        }
    }
}
