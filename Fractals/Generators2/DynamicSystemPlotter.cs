using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Color = Windows.UI.Color;

namespace Fractals.Generators2
{
    public class DynamicSystemPlotter : FractalGenerator2
    {

        public class Result
        {
            public static int maxIterations;
            public int iterations = 0;
            public bool isJulia;
            public Complex z;
        }


        public Complex Function(Complex z)
        {
            //return Complex.Tan(z);
            //return Complex.Sin(z);
            //return Complex.Pow(z, 8) + 15 * Complex.Pow(z, 4) - 16;
            //return Complex.Pow(z, 3) - 2 * z + 2;
            //return new Complex(Math.Pow(z.Real, 3) - z.Real, 0);
            return Complex.Pow(z, 3) - 1;
        }

        public Complex Derivative(Complex z)
        {
            //return Complex.Pow(Complex.Tan(z), 2) + 1;
            //return Complex.Cos(z);
            //return 8 * Complex.Pow(z, 7) + 15 * 4 * Complex.Pow(z, 3);
            //return 3 * Complex.Pow(z, 2) - 2;
            //return new Complex(3 * Math.Pow(z.Real, 2) - 1, 0);
            return 3 * Complex.Pow(z, 2);
        }

        private object Iterate(Complex z)
        {
            double tolerance = 0.01;
            Complex last;
            for (int i = 0; i < Iterations; i++)
            {
                z = z - Function(z) / Derivative(z);
                if (Math.Abs(last.Real - z.Real) < tolerance && Math.Abs(last.Imaginary - z.Imaginary) < tolerance)
                    break;
                last = z;
            }
            return z;
        }

        private Color Color0(object element)
        {
            var z = (Complex)element;
            return DomainColor.Generate(z);
        }

        private Color Color1(object element)
        {
            var result = element as Result;
            if (result.isJulia)
                return Colors.White;
            else
                return DomainColor.Generate(result.z);
        }

        private Color Color2(object element)
        { 
            return DomainColor.Generate(((Complex)element).Real);
        }

        protected override Color[,] Generate(Rectangle drawbox, Viewbox viewbox)
        {
            Result.maxIterations = 1;
            var plot = Plot(drawbox, viewbox, 2, Iterate);
            var color = Color(plot, Color0);
            return color;

            int f = 10;
            var steps = new Complex(viewbox.Size.Real / drawbox.Width / f, viewbox.Size.Imaginary / drawbox.Height / f);

            for (int xn = 0; xn < drawbox.Width * f; xn++)
            {
                double x = (xn - drawbox.Left) * steps.Real + viewbox.Left;
                double y = Function(x).Real;

                int p = (int)xn / f;
                int q = (int)((-y + viewbox.Top) / steps.Imaginary + drawbox.Top) / f;

                if (p > 0 && p < drawbox.Width - 1 && q > 0 && q < drawbox.Height - 1)
                { 
                    color[p, q] = Colors.White;
                    color[p + 1, q] = Colors.White;
                    color[p - 1, q] = Colors.White;
                    color[p, q + 1] = Colors.White;
                    color[p, q - 1] = Colors.White;
                }
            }

            return color;
        }

        public override string ToString() => "Dynamic System";

    }
}
