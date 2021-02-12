using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ColorHelper;

namespace Fractals.Generators
{
    public class MandelPlotParameters
    {
        public MandelPlotParameters(ValueRect values, PixelRect pixels, int[,] plot)
        {
            this.values = values;
            this.pixels = pixels;
            this.plot = plot;
            this.maxIterationsCount = 0;
        }

        public ValueRect values;
        public PixelRect pixels;
        public int maxIterationsCount;
        public int[,] plot;
    }

    public class Mandelbrot : FractalGenerator
    {
        double maxBetrag = 100;

        public override int Iterations { get; set; } = 100;

        public override int MaxIterations { get; protected set; } = 5000;

        public override ValueRect DefaultValues => new ValueRect(-3.5, -3, 2.5, 3);

        public override string ToString() => "Mandelbrot";

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



            int totalNumIterations = 0;
            int[] numIterations = new int[Iterations + 1];

            for (int y = 0; y < pixels.Height; y++)
                for (int x = 0; x < pixels.Width; x++)
                    numIterations[plot[x, y]]++;


            foreach (var n in numIterations)
                totalNumIterations += n;


            int m = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    var iterations = plot[x, y];

                    if (iterations >= maxIterationCount)
                        ValuesInCondomain += 1;

                    


                    double hue = 0;
                    for (int i = 0; i <= iterations; i++)
                        hue += (double)numIterations[i] / (double)totalNumIterations;

                    hue *= 360;

                    Color color = DomainColor.HsvToRgb(hue, 0.6, 1.0);
                    if (iterations == Iterations)
                        color = Colors.Black;

                    pixeldata[m++] = color.B;
                    pixeldata[m++] = color.G;
                    pixeldata[m++] = color.R;
                    pixeldata[m++] = 255;
                }
            }


            //int n = 0;
            //for (int y = 0; y < bitmap.PixelHeight; y++)
            //{
            //    for (int x = 0; x < bitmap.PixelWidth; x++)
            //    {
            //        var result = plot[x, y];

            //        if (result >= maxIterationCount)
            //            ValuesInCondomain += 1;

            //        RGB color = ColorConverter.HsvToRgb(new HSV(result * 15, 60, 100));

            //        pixeldata[n++] = color.B;
            //        pixeldata[n++] = color.G;
            //        pixeldata[n++] = color.R;
            //        pixeldata[n++] = 255;
            //    }
            //}

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
                    var iterationCount = Iterate2(reC, imC);
                    if (iterationCount > maxIterationCount)
                        maxIterationCount = iterationCount;
                    plot[x, y] = iterationCount;
                }
            }
        }

        private int Iterate2(double reC, double imC)
        {
            int iteration = 0;
            double reZ = 0;
            double imZ = 0;
            int escapeBoundary = 80;

            while (reZ * reZ + imZ * imZ <= escapeBoundary && iteration < Iterations)
            {
                var temp = reZ * reZ - imZ * imZ + reC;
                imZ = 2 * reZ * imZ + imC;
                reZ = temp;
                iteration += 1;
            }

            //if (iteration < Iterations)
            //{
            //    var log_zn = Math.Log(reZ * reZ + imZ * imZ) / 2;
            //    var nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            //    // Rearranging the potential function.
            //    // Dividing log_zn by log(2) instead of log(N = 1<<8)
            //    // because we want the entire palette to range from the
            //    // center to radius 2, NOT our bailout radius.
            //    iteration = (int)(iteration + 1 - nu);
            //}



            return iteration;
        }


        private int Iterate(double x, double y, double xadd, double yadd, double maxBetrag, int maxIter)
        {
            var remainIter = maxIter;
            var xx = x * x;
            var yy = y * y;
            var xy = x * y;
            var betrag = xx + xy;

            while (betrag <= maxBetrag && remainIter > 0)
            {
                remainIter--;
                x = xx - yy + xadd;
                y = xy + xy + yadd;
                xx = x * x;
                yy = y * y;
                xy = x * y;
                betrag = xx + yy;
            }
            return maxIter - remainIter;
        }
    }
}
