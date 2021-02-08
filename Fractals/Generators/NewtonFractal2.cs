using ColorHelper;
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

namespace Fractals.Generators
{
    public class NewtonFractal2 : FractalGenerator, IJulia
    {
        public class PlotParameters
        {
            public PlotParameters(ValueRect values, PixelRect pixels, Complex[,] plot)
            {
                this.values = values;
                this.pixels = pixels;
                this.plot = plot;
            }

            public ValueRect values;
            public PixelRect pixels;
            public Complex[,] plot;
        }

        public Complex C { get; set; } = new Complex(0, 0);

        public override ValueRect DefaultValues => new ValueRect(-1, -1, 1, 1);

        public override string ToString() => "Newton Fractal 2";


        public override int MinIterations { get; protected set; } = 0;

        public override int Iterations { get; set; } = 3;

        public Complex Function(Complex z)
        {
            //return Complex.Tan(z);
            //return Complex.Sin(z);
            return Complex.Pow(z, 8) + 15 * Complex.Pow(z, 4) - 16;
            return Complex.Pow(z, 3) - 2 * z + 2;
            return Complex.Pow(z, 3) - 1;
        }

        public Complex Derivative(Complex z)
        {
            //return Complex.Pow(Complex.Tan(z), 2) + 1;
            //return Complex.Cos(z);
            return 8 * Complex.Pow(z, 7) + 15 * 4 * Complex.Pow(z, 3);
            return 3 * Complex.Pow(z, 2) - 2;
            return 3 * Complex.Pow(z, 2);
        }

        public Complex Step(Complex z)
        {
            return z - (Function(z) / Derivative(z));
        }

        protected void Plot(ValueRect values, PixelRect pixels, Complex[,] plot)
        {
            double dotSizeX = values.Width / pixels.Width;
            double dotSizeY = values.Height / pixels.Height;

            for (int y = pixels.top; y < pixels.bottom; y++)
            {
                for (int x = pixels.left; x < pixels.right; x++)
                {
                    Complex z = new Complex((x - pixels.left) * dotSizeX + values.left, (y - pixels.top) * dotSizeY + values.top);
                    //var c = z;
                    for (int i = 0; i < Iterations; i++)
                    {
                        //z = Complex.Pow(z, 2) + C;
                        //z = Complex.Pow(z, 2) + c;
                        z = Step(z);
                    }
                        
                    plot[x, y] = z;
                }
            }
        }

        public byte GetBrightness(double value)
        {
            double a = 0.5;
            if (double.IsNaN(value))
                value = 0;
            return (byte)Math.Clamp((1.1 - Math.Pow(a, Math.Abs(value))) * 100, 0, 100);
        }
        
        public override void UpdateBitmap(WriteableBitmap bitmap, ValueRect values)
        {
            ValuesInDomain = bitmap.PixelHeight * bitmap.PixelWidth;
            ValuesInCondomain = 0;
            var pixels = new PixelRect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            var plot = new Complex[pixels.Width, pixels.Height];

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

            foreach (var arg in args)
                tasks.Add(Task.Run(() => Plot(arg.values, arg.pixels, arg.plot)));

            Task.WaitAll(tasks.ToArray());

      
            var pixeldata = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            int n = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    
                    var z = plot[x, y];
                    // color = ColorConverter.HsvToRgb(new HSV(Converters.AbsoluteHue((int)((double)z.Phase * 180.0 / Math.PI)), 80, GetBrightness(z.Magnitude))); // Black if NaN
                    Color color = DomainColor.Generate(z);
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
