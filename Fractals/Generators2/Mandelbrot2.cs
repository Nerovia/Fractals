using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Fractals.Generators2
{
    public class Mandelbrot2 : FractalGenerator2
    {
        public override string ToString() => "Mandelbrot";

        protected override Windows.UI.Color[,] Generate(Rectangle drawbox, Viewbox viewbox)
        {
            var plot = Plot(drawbox, viewbox, 2, Iterate2);

            var maxIterationCount = 1;
            foreach (var p in plot)
                if ((int)p > maxIterationCount)
                    maxIterationCount = (int)p;

            int totalNumIterations = 0;
            int[] numIterations = new int[Iterations + 1];

            for (int y = 0; y < drawbox.Height; y++)
                for (int x = 0; x < drawbox.Width; x++)
                    numIterations[(int)plot[x, y]]++;

            foreach (var n in numIterations)
                totalNumIterations += n;

            return Color(plot, (object element) =>
            {
                var iterations = (int)element;

                double hue = 0;
                for (int i = 0; i <= iterations; i++)
                    hue += (double)numIterations[i] / (double)totalNumIterations;

                hue *= 360;

                Windows.UI.Color color = DomainColor.HsvToRgb(hue, 0.6, 1.0);
                if (iterations == Iterations)
                    color = Colors.Black;

                return color;
            });
        }

        protected object Iterate2(Complex z)
        {
            int i = 0;
            int escapeBoundary = 8;
            var c = new Complex(-0.5, 0);

            while (z.Real * z.Real + z.Imaginary * z.Imaginary < escapeBoundary && i < Iterations)
            {
                z = new Complex(z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real, 2 * z.Real * z.Imaginary + c.Imaginary);
                i += 1;
            }
            return i;
        }

        protected object Iterate1(Complex c)
        {
            int i = 0;
            int escapeBoundary = 8;
            var z = Complex.Zero;

            while (z.Real * z.Real + z.Imaginary * z.Imaginary <= escapeBoundary && i < Iterations)
            {
                var temp = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z = new Complex(temp, 2 * z.Real * z.Imaginary + c.Imaginary);
                i += 1;
            }

            return i;
        }
    }
}
