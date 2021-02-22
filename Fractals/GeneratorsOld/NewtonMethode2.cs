using ColorHelper;
using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Generators
{
    public class NewtonMethode2 : FractalGenerator
    {
        public class Result
        {
            public static bool Reset { get; set; }
            public static int MinIterations { get; private set; } = 0;
            public static int PeekIterations { get; private set; } = 0;
            public int Iterations { get; private set; }
            public bool IsRoot { get; private set; }
            public double X { get; private set; }

            public override string ToString()
            {
                return X.ToString();
            }

            public Result(int iterations, double x, bool isRoot)
            {
                Iterations = iterations;
                X = x;
                IsRoot = isRoot;

                if (Reset)
                {
                    MinIterations = Iterations;
                    PeekIterations = Iterations;
                    Reset = false;
                }

                if (Iterations > PeekIterations)
                    PeekIterations = Iterations;
                else if (Iterations < MinIterations)
                    MinIterations = Iterations;
            }
        }

        public override int Iterations { get; set; } = 0;

        public override ValueRect DefaultValues => new ValueRect(-1, -1, 1, 1);

        public double Tolerance { get; set; } = 0.0000001;

        public override int MaxIterations { get; protected set; } = 1000;

        public override int MinIterations { get; protected set; } = 0;

        public override string ToString() => "Newton Methode 2";

        public double Function(double x)
        {
            //return Math.Sin(x);
            //return Math.Pow(x, 3) - 2 * x + 2;
            return Math.Pow(x, 3) - x;
        }

        public double Derivative(double x)
        {
            //return Math.Cos(x);
            //return 3 * Math.Pow(x, 2) - 2;
            return 3 * Math.Pow(x, 2) - 1;
        }

        public Result IterationsToRoot(double x)
        {
            int i = 0;
            for (i = 0; i < Iterations; i++)
            {
                if (Math.Abs(Function(x)) < Tolerance)
                    return new Result(i, x, true);
                else
                    x -= Function(x) / Derivative(x);
            }
            return new Result(i, x, false);
        }



        public Result[] Plot(double valueWidth, double valueOffset, int pixelWidth, int pixelOffset)
        {
            double dotSize = valueWidth / pixelWidth;
            Result[] plot = new Result[pixelWidth];

            for (int x = 0; x < pixelWidth; x++)
            {
                var v = (x - pixelOffset) * dotSize + valueOffset;
                plot[x] = IterationsToRoot(v);
            }

            return plot;
        }

        


        public override void UpdateBitmap(WriteableBitmap bitmap, ValueRect valueRange)
        {
            Result.Reset = true;

            var plot = Plot(valueRange.Width, valueRange.left, bitmap.PixelWidth, 0);

            //ValueRect values = new ValueRect(valueRange.left, 26, valueRange.right, 0);
            ValueRect values = new ValueRect(valueRange.left, Result.PeekIterations + 2, valueRange.right, Result.MinIterations - 1);
            PixelRect pixels = new PixelRect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);

            double dotSizeX = values.Width / pixels.Width;
            double dotSizeY = values.Height / pixels.Height;

            var pixeldata = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            int h = bitmap.PixelHeight / 10;
            int n = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    var result = plot[x];

                    RGB rootColor = new RGB(0, 0, 0);
                    //if (result.IsRoot)
                        rootColor = ColorConverter.HsvToRgb(new HSV(Converters.AbsoluteHue((int)(plot[x].X * 120)), 60, 100));

                    RGB iterationColor = ColorConverter.HsvToRgb(new HSV(result.Iterations * 20, 60, 100));


                    Color color = DomainColor.Generate(result.X);
                    //if (y < h)
                    //{
                    //    color = iterationColor;
                    //}
                    //else if (y > bitmap.PixelHeight * 0.9)
                    //{
                    //    color = rootColor;
                    //}
                    //else
                    //{
                    //    var j = (y - pixels.top) * dotSizeY + values.top;

                    //    if (result.Iterations < j && result.Iterations > j - 1)
                    //        color = rootColor;
                    //}
                    

                    pixeldata[n++] = color.B;
                    pixeldata[n++] = color.G;
                    pixeldata[n++] = color.R;
                    pixeldata[n++] = 255;
                }
            }

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixeldata, 0, pixeldata.Length);
            }
        }
    }
}
