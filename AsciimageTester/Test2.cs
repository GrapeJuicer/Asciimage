using Asciimage.Brushes;
using Asciimage.Core;
using SkiaSharp;
using System.Diagnostics;

namespace AsciimageTester;

[TestClass]
public class Test2
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

        Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Start Brush Creating");
        
        CusomFontBrush brush = new(font, ss, segs);

        Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Brush created");

        using var stream = File.OpenRead("star.png");
        SKBitmap bitmap = SKBitmap.Decode(stream);

        Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} Start Generating");

        AsciiMat mat = Asciimage.Core.Asciimage.Generate(bitmap, brush, segs[0], new AsciimageConfig(height: 20, colorMode: ColorMode.Binary));
        
        Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} End");

        Debug.WriteLine(mat.ToString());
    }
}
