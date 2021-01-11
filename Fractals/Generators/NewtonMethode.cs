using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Generators
{
    public class NewtonMethode : FractalGenerator
    {
        public override string ToString()
        {
            return "Netwon Methode";
        }

        public override List<BindableParameter> Parameters { get; protected set; } = new List<BindableParameter>()
        {

        };

        public override ValueRect DefaultValues => new ValueRect(-1, -1, 1, 1);

        public override int MaxIterations { get; protected set; } = 50;

        public override int Iterations
        {
            get => _Iterations;
            set
            {
                if (_Iterations != value)
                {
                    value = Math.Clamp(value, MinIterations, MaxIterations);
                    _Iterations = value;
                }
            }
        }
        private int _Iterations = 10;

        public double Tolerance
        {
            get => _Tolerance;
            set
            {
                if (_Tolerance != value)
                {
                    _Tolerance = Math.Clamp(value, 0.1, 0.0000001);
                }
            }
        }
        private double _Tolerance = 0.000001;


        private double Function(double x)
        {
            return Math.Pow(x, 3) - x;
            //return Math.Sin(x);
        }

        private double Derivative(double x)
        {
            return 3 * Math.Pow(x, 2) - 1;
            //return Math.Cos(x);
        }


        private double[] Roots { get; set; } = new double[]
        {
            -1,
            0,
            1
        };

        private double RootTrig { get; set; } = Math.PI;


        private int FindRootTrig(double x)
        {
            if (Derivative(x) == 0)
                return -1;
            var r = x % RootTrig;
            for (int i = 0; i < Iterations; i++)
            {
                if (Math.Abs(r) < Tolerance)
                {
                    if (r < 0)
                        return Math.Abs((int)((x - Math.PI / 2) / Math.PI));
                    else
                        return Math.Abs((int)((x + Math.PI / 2) / Math.PI));
                }

                r -= Function(r) / Derivative(r);
            }

            return -1;
        }

        private int FindRoot(double x)
        {
            if (Derivative(x) == 0)
                return -1;

            for (int i = 0; i < Iterations; i++)
            {
                for (byte n = 0; n < Roots.Length; n++)
                {
                    var difference = x - Roots[n];
                    if (Math.Abs(difference) < Tolerance)
                    {
                        return n;
                    }
                }
                x -= Function(x) / Derivative(x);
            }

            return -1;
        }

        

        private Color[,] Overlay(double xmin, double xmax, double ymin, double ymax, int pixelWidth, int pixelHight)
        {
            var overlay = new Color[pixelWidth, pixelHight];
            

            double dotSize = (xmax - xmin) / pixelWidth / 100;

            for (int xn = 0; xn < pixelWidth * 100; xn++)
            {
                int y = (int)((Function(xn * dotSize + xmin) - ymin) * pixelHight / (ymax - ymin));

                int x = xn / 100;

                if (x > 0 && x < pixelWidth - 1 && y > 0 && y < pixelHight - 1)
                {

                    overlay[x, y] = Colors.White;
                    //overlay[x - 1, y] = Colors.White;
                    //overlay[x + 1, y] = Colors.White;
                    //overlay[x, y - 1] = Colors.White;
                    //overlay[x, y + 1] = Colors.White;
                }
            }

            return overlay;
        }

        protected Color[,] Plot(ValueRect values, int width, int height)
        {
            double dotSize = values.Width / width;

            var plot = new Color[width, 1];

            for (int x = 0; x < width; x++)
            {
                Color color;
                var root = FindRoot(x * dotSize + values.left);
                if (root == -1)
                    color = Colors.Transparent;
                else
                    switch (root % 3)
                    {
                        case 0:
                            ValuesInCondomain += 1;
                            color = Colors.Red;
                            break;

                        case 1:
                            ValuesInCondomain += 1;
                            color = Colors.Green;
                            break;

                        case 2:
                            ValuesInCondomain += 1;
                            color = Colors.Blue;
                            break;

                        default:
                            color = Colors.Transparent;
                            break;
                    }
                plot[x, 0] = color;
            }
            return plot;
        }

        public override void UpdateBitmap(WriteableBitmap bitmap, ValueRect values)
        {
            ValuesInDomain = bitmap.PixelWidth;
            ValuesInCondomain = 0;
            var plot = Plot(values, bitmap.PixelWidth, bitmap.PixelHeight);
            var overlay = Overlay(values.left, values.right, -1, 1, bitmap.PixelWidth, bitmap.PixelHeight);
            var pixeldata = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            int n = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    pixeldata[n++] = (byte)Math.Abs(plot[x, 0].B - overlay[x, y].B);
                    pixeldata[n++] = (byte)Math.Abs(plot[x, 0].G - overlay[x, y].G);
                    pixeldata[n++] = (byte)Math.Abs(plot[x, 0].R - overlay[x, y].R);
                    pixeldata[n++] = (byte)Math.Abs(plot[x, 0].A - overlay[x, y].A);
                }
            }

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixeldata, 0, pixeldata.Length);
            }
        }
    }
}
