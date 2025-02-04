using Asciimage.Brushes;
using Asciimage.Core;
using SkiaSharp;
using System.Diagnostics;

namespace AsciimageTester;

[TestClass]
public class Test3
{
    [TestMethod]
    public void TestMethod1()
    {
        SKFont font = new()
        {
            Typeface = SKTypeface.FromFamilyName("Cascadia Code"),
            Size = 12,
        };

        List<string> ss = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~ ".Select(x => x.ToString()).ToList();
        List<SegmentCount> segs = [SegmentCount.FourByTwo];

        GridSegmentedFontBrush brush = new(font, ss, segs);

        using var stream = File.OpenRead("star.png");
        SKBitmap bitmap = SKBitmap.Decode(stream);

        AsciiMat mat = Asciimage.Core.Asciimage.Generate(bitmap, brush, segs[0], new AsciimageConfig(height: 100, colorMode: ColorMode.Binary));

        DateTimeOffset startTime = DateTimeOffset.Now;

        for (int i = 0; i < 10; i++)
        {
            mat.ToString();
        }

        DateTimeOffset endTime = DateTimeOffset.Now;

        Debug.WriteLine($"{startTime:HH:mm:ss.fff} Start x1000");
        Debug.WriteLine($"{endTime:HH:mm:ss.fff} End ({(endTime - startTime).TotalMilliseconds} ms)");
        Debug.WriteLine(mat.ToString());

    }
}
