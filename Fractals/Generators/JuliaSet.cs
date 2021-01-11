using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Generators
{
    public class JuliaSet : FractalGenerator
    {
        public override ValueRect DefaultValues => new ValueRect(-2, -2, 2, 2);

        public override string ToString() => "Julia Set";

        public Complex C = new Complex(-0.5, 0);

        public override int Iterations { get; set; } = 100;

        public override int MaxIterations { get; protected set; } = 1000;

        public override void UpdateBitmap(WriteableBitmap bitmap, ValueRect values)
        {
            PixelRect pixels = new PixelRect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);

            ValuesInDomain = pixels.Width * pixels.Height;
            ValuesInCondomain = 0;

            int[,] plot = new int[pixels.Width, pixels.Height];

            List<MandelPlotParameters> args = new List<MandelPlotParameters>();
            List<Task> tasks = new List<Task>();

            int fraction = 2;
            for (int y = 0; y < fraction; y++)
            {
                for (int x = 0; x < fraction; x++)
                {
                    args.Add(new MandelPlotParameters(values.GetFraction(fraction, x, y), pixels.GetFraction(fraction, x, y), plot));
                }
            }

            foreach (var arg in args)
                tasks.Add(Task.Run(() => Plot(arg.values, arg.pixels, arg.plot, ref arg.maxIterationsCount)));

            Task.WaitAll(tasks.ToArray());


            var maxIterationCount = 1;
            foreach (var arg in args)
                if (arg.maxIterationsCount > maxIterationCount)
                    maxIterationCount = arg.maxIterationsCount;


            var pixeldata = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            int n = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    var result = plot[x, y];

                    if (result >= maxIterationCount)
                        ValuesInCondomain += 1;

                    pixeldata[n++] = (byte)(result * 255 / maxIterationCount);
                    pixeldata[n++] = 0;
                    pixeldata[n++] = 0;
                    pixeldata[n++] = 255;
                }
            }

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixeldata, 0, pixeldata.Length);
            }
        }


        protected void Plot(ValueRect values, PixelRect pixels, int[,] plot, ref int maxIterationCount)
        {
            double dotSizeX = values.Width / pixels.Width;
            double dotSizeY = values.Height / pixels.Height;

            for (int y = pixels.top; y < pixels.bottom; y++)
            {
                double imC = (y - pixels.top) * dotSizeY + values.top;

                for (int x = pixels.left; x < pixels.right; x++)
                {
                    double reC = (x - pixels.left) * dotSizeX + values.left;

                    //var iterationCount = Iterate(reC, imC, reC, imC, maxBetrag, Iterations);
                    var iterationCount = Iterate(reC, imC);
                    if (iterationCount > maxIterationCount)
                        maxIterationCount = iterationCount;
                    plot[x, y] = iterationCount;
                }
            }
        }

        public double R { get; set; } = 2;

        public int Iterate(double reZ, double imZ)
        {
            int iteration = 0;

            while (reZ * reZ + imZ * imZ < Math.Pow(R, 2) && iteration < Iterations)
            {
                var temp = reZ * reZ - imZ * imZ;
                imZ = 2 * reZ * imZ + C.Imaginary;
                reZ = temp + C.Real;

                iteration += 1;
            }
            return iteration;
        }

        public int Iterate2(double zx, double zy)
        {
            double n = 10;
            int iteration = 0;
            while (zx * zx + zy * zy < Math.Pow(R, 2) && iteration < Iterations)
            {
                var temp = Math.Pow((zx * zx + zy * zy), (n / 2)) * Math.Cos(n * Math.Atan2(zy, zx)) + C.Real;
                zy = Math.Pow((zx * zx + zy * zy), (n / 2)) * Math.Sin(n * Math.Atan2(zy, zx)) + C.Imaginary;
                zx = temp;

                iteration = iteration + 1;
            }
            return iteration;
        }
    }
}
