using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Generators
{
    public class SierpinskiCarpet : FractalGenerator
    {
        public override ValueRect DefaultValues => new ValueRect(-1, -1, 1, 1);

        public override int Iterations { get; set; } = 1;

        public override int MaxIterations { get; protected set; } = 20;

        public override string ToString()
        {
            return "Sierpinski Carpet";
        }

        public override void UpdateBitmap(WriteableBitmap bitmap, ValueRect valueRange)
        {
            PixelRect pixels = new PixelRect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            ValuesInDomain = pixels.Width * pixels.Height;
            ValuesInCondomain = 0;

            bool[,] plot =
            {
                { true, true, true, },
                { true, false, true, },
                { true, true, true, }
            };

            for (int m = 0; m < Iterations; m++)
            {
                if (plot.GetLength(0) >= pixels.Width || plot.GetLength(1) >= pixels.Height)
                    break;
                plot = AppendCarpet(plot);
            }

            var pixeldata = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            int n = 0;
            for (int y = 0; y < pixels.Height; y++)
            {
                if (y >= plot.GetLength(1))
                {
                    pixeldata[n++] = 0;
                    pixeldata[n++] = 0;
                    pixeldata[n++] = 0;
                    pixeldata[n++] = 255;
                }
                else
                {
                    for (int x = 0; x < pixels.Width; x++)
                    {
                        if (x >= plot.GetLength(0))
                        {
                            pixeldata[n++] = 0;
                            pixeldata[n++] = 0;
                            pixeldata[n++] = 0;
                            pixeldata[n++] = 255;
                        }
                        else
                        {
                            var result = plot[x, y];

                            if (result)
                            {
                                ValuesInCondomain += 1;
                                pixeldata[n++] = 255;
                            }
                            else
                            {
                                pixeldata[n++] = 0;
                            }

                            pixeldata[n++] = 0;
                            pixeldata[n++] = 0;
                            pixeldata[n++] = 255;
                        }
                    }
                }
            }

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixeldata, 0, pixeldata.Length);
            }
        }

        public bool[,] AppendCarpet(bool[,] oldCarpet)
        {
            var widthThird = oldCarpet.GetLength(0);
            var heightThird = oldCarpet.GetLength(1);

            var width = oldCarpet.GetLength(0) * 3;
            var height = oldCarpet.GetLength(1) * 3;

            var carpet = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y >= heightThird && y < 2 * heightThird && x >= widthThird && x < 2 * widthThird)
                        carpet[x, y] = false;
                    else
                        carpet[x, y] = oldCarpet[x % widthThird, y % heightThird];
                }
            }

            return carpet;
        }
    }
}
