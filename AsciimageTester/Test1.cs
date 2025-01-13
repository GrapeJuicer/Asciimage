using OpenCvSharp;
using System.Diagnostics;

namespace AsciimageTester
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // print current directory
            Console.WriteLine(Directory.GetCurrentDirectory());

            Debug.WriteLine("This is debug");
            // start.pngを読み込む
            Mat img = Cv2.ImRead("star.png", ImreadModes.Color);

            Console.WriteLine(img.Width + " " + img.Height);

            Asciimage.AsciimageConfig config = new(200, 50, colorMode: Asciimage.ColorMode.Binary);

            var am = Asciimage.Asciimage.Generate(img, config);
            Console.WriteLine(am.ToString());
        }
    }
}
