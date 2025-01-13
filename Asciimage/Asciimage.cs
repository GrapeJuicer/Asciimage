using OpenCvSharp;

namespace Asciimage
{
    public static class Asciimage
    {
        public static AsciiMat Generate(Mat mat, AsciimageConfig config)
        {
            if (config.ColorMode != ColorMode.Binary)
            {
                throw new NotSupportedException("ColorMode only supports Binary for now.");
            }

            // Define a local mat for processing
            Mat localMat = mat.Clone();

            // If config.ColorMode is Binary, binarize localMat. The threshold is 127.
            if (config.ColorMode == ColorMode.Binary)
            {
                Cv2.CvtColor(localMat, localMat, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(localMat, localMat, 127, 255, ThresholdTypes.Binary);
            }

            // Divide the Mat by config.Width and config.Height, and repeat the following process for each region
            int cellWidth = (int)Math.Ceiling((double)localMat.Width / config.Width);
            int cellHeight = (int)Math.Ceiling((double)localMat.Height / config.Height);
            char[,] asciiArt = new char[config.Height, config.Width];

            for (int y = 0; y < config.Height; y++)
            {
                for (int x = 0; x < config.Width; x++)
                {
                    // Define the region of interest (ROI)
                    int roiWidth = Math.Min(cellWidth, localMat.Width - x * cellWidth);
                    int roiHeight = Math.Min(cellHeight, localMat.Height - y * cellHeight);
                    Rect roi = new(x * cellWidth, y * cellHeight, roiWidth, roiHeight);
                    //Console.WriteLine($"({x}, {y}) --> ({roi.X}, {roi.Y}, {roi.Width}, {roi.Height})");
                    Mat cell = new(localMat, roi);

                    // Calculate the average brightness of the cell
                    Scalar mean = Cv2.Mean(cell);
                    double brightness = mean.Val0;

                    // Map the brightness to a character
                    // tempprary implementation
                    asciiArt[y, x] = brightness >= 0.5 ? '#' : ' ';
                }
            }

            return new AsciiMat(asciiArt);
        }
    }
}
