using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Fractals.Generators
{
    public class MandelbrotGenerator : FractalGenerator
    {
        public override string ToString() => "Mandelbrot";

        public Complex JuliaConstant { get; set; }

        public bool JuliaMode { get; set; } = false;

        protected override Windows.UI.Color[,] Generate(Rectangle drawbox, IViewbox viewbox)
        {
            object[,] plot;
            if (JuliaMode)
                plot = Plot(drawbox, viewbox, 2, IterateJulia, Iterations);
            else
                plot = Plot(drawbox, viewbox, 2, IterateMandelbrot, Iterations);

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

        protected object IterateJulia(Complex z, int iterations)
        {
            int i = 0;
            int escapeBoundary = 8;
            var c = JuliaConstant;

            while (z.Real * z.Real + z.Imaginary * z.Imaginary < escapeBoundary && i < iterations)
            {
                z = new Complex(z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real, 2 * z.Real * z.Imaginary + c.Imaginary);
                i += 1;
            }
            return i;
        }

        protected object IterateMandelbrot(Complex c, int iterations)
        {
            int i = 0;
            int escapeBoundary = 8;
            var z = Complex.Zero;

            while (z.Real * z.Real + z.Imaginary * z.Imaginary <= escapeBoundary && i < iterations)
            {
                var temp = z.Real * z.Real - z.Imaginary * z.Imaginary + c.Real;
                z = new Complex(temp, 2 * z.Real * z.Imaginary + c.Imaginary);
                i += 1;
            }

            return i;
        }
    }
}
