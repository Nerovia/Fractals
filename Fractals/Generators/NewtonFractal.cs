using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Generators
{
    public class NewtonFractal : FractalGenerator
    {


        public override string ToString() => "Newton Fractal";

        public NewtonFractal()
        {
            Iterations = 100;
        }


        public override List<BindableParameter> Parameters { get; protected set; }

        protected double Tolerance { get; set; } = 0.001;

        public override ValueRect DefaultValues => new ValueRect(-1, -1, 1, 1);

        protected Complex[] Roots = new Complex[]
        {
            new Complex(1, 0),
            new Complex(-0.5, Math.Sqrt(3) / 2),
            new Complex(-0.5, -Math.Sqrt(3) / 2)
        };

        protected Complex Function(Complex z)
        {
            // z^3 - 1
            return Complex.Pow(z, 3) - 1;
        }

        protected Complex Derivative(Complex z)
        {
            // 3 * z^2
            return 3 * Complex.Pow(z, 2);
        }

        public RootResult FindRoot(Complex z)
        {
            if (Derivative(z) == 0)
                return new RootResult(0, -1);

            for (int i = 0; i <= Iterations; i++)
            {
                for (int n = 0; n < Roots.Length; n++)
                {
                    var difference = z - Roots[n];
                    if (Math.Abs(difference.Real) < Tolerance && Math.Abs(difference.Imaginary) < Tolerance)
                    {
                        return new RootResult(i, n);
                    }
                }
                z -= Function(z) / Derivative(z);
            }
            return new RootResult(Iterations, -1);
        }

        protected void Plot(ValueRect values, PixelRect pixels, RootResult[,] plot, ref int maxIterationCount)
        {
            double dotSizeX = values.Width / pixels.Width;
            double dotSizeY = values.Height / pixels.Height;

            for (int y = pixels.top; y < pixels.bottom; y++)
            {
                for (int x = pixels.left; x < pixels.right; x++)
                {
                    var result = FindRoot(new Complex((x - pixels.left) * dotSizeX + values.left, (y - pixels.top) * dotSizeY + values.top));
                    if (result.iterationCount > maxIterationCount)
                        maxIterationCount = result.iterationCount;
                    plot[x, y] = result;
                }
            }
        }

        public class PlotParameters
        {
            public PlotParameters(ValueRect values, PixelRect pixels, RootResult[,] plot)
            {
                this.values = values;
                this.pixels = pixels;
                this.plot = plot;
                this.maxIterationsCount = 0;
            }

            public ValueRect values;
            public PixelRect pixels;
            public int maxIterationsCount;
            public RootResult[,] plot;
        }

        public override void UpdateBitmap(WriteableBitmap bitmap, ValueRect values)
        {
            ValuesInDomain = bitmap.PixelHeight * bitmap.PixelWidth;
            ValuesInCondomain = 0;
            var pixels = new PixelRect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            var plot = new RootResult[pixels.Width, pixels.Height];

            List<PlotParameters> args = new System.Collections.Generic.List<PlotParameters>();
            List<Task> tasks = new List<Task>();

            int fraction = 2;
            for (int y = 0; y < fraction; y++)
            {
                for (int x = 0; x < fraction; x++)
                {
                    args.Add(new PlotParameters(values.GetFraction(fraction, x, y), pixels.GetFraction(fraction, x, y), plot));
                }
            }

            foreach(var arg in args)
                tasks.Add(Task.Run(() => Plot(arg.values, arg.pixels, arg.plot, ref arg.maxIterationsCount)));

            Task.WaitAll(tasks.ToArray());

            var maxIterationCount = 0;
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
                    Color color;
                    if (result.root == -1)
                        color = Colors.Transparent;
                    else
                        switch (result.root % 3)
                        {
                            case 0:
                                color = Color.FromArgb(255, result.Brightness(maxIterationCount), 0, 0);
                                ValuesInCondomain += 1;
                                break;

                            case 1:
                                color = Color.FromArgb(255, 0, result.Brightness(maxIterationCount), 0);
                                ValuesInCondomain += 1;
                                break;

                            case 2:
                                color = Color.FromArgb(255, 0, 0, result.Brightness(maxIterationCount));
                                ValuesInCondomain += 1;
                                break;

                            default:
                                color = Colors.Transparent;
                                break;
                        }

                    pixeldata[n++] = color.B;
                    pixeldata[n++] = color.G;
                    pixeldata[n++] = color.R;
                    pixeldata[n++] = color.A;
                }
            }

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixeldata, 0, pixeldata.Length);
            }
        }
    }
}
