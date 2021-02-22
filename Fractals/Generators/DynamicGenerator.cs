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

namespace Fractals.Generators
{
    public interface IDynamicSystem
    {
        ColorMethode Color { get; }
        PlotMethode Plot { get; }
        string Name { get; }
    }

    public class DynamicSystem : IDynamicSystem
    {
        public DynamicSystem(string name)
        {
            Name = name;
        }
        public ColorMethode Color { get; set; } = DefaultColor;
        public PlotMethode Plot { get; set; } = DefaultPlot;
        public static DynamicSystem Default { get; } = new DynamicSystem("z");

        public string Name { get; }

        private static object DefaultPlot(Complex z, int iterations)
        {
            return z;
        }

        private static Color DefaultColor(object e)
        {
            return DomainColor.Generate((Complex)e);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class NewtonMethodeSystem : IDynamicSystem
    {
        public NewtonMethodeSystem(string name, Func<Complex, Complex> function, Func<Complex, Complex> derivative)
        {
            Name = name;
            this.function = function;
            this.derivative = derivative;
        }

        public Func<Complex, Complex> function;
        public Func<Complex, Complex> derivative;

        public PlotMethode Plot
        {
            get => (Complex z, int iterations) =>
            {
                for (int i = 0; i < iterations; i++)
                    z = z - function(z) / derivative(z);
                return z;
            };
        }

        public ColorMethode Color { get; set; } = DynamicSystem.Default.Color;

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }



    public class DynamicGenerator : FractalGenerator
    {

        public class Result
        {
            public static int maxIterations;
            public int iterations = 0;
            public bool isJulia;
            public Complex z;
        }


        public IDynamicSystem System
        {
            get
            {
                if (_System is null)
                    return DynamicSystem.Default;
                else
                    return _System;
            }
            set
            {
                _System = value;
            }
        }
        private IDynamicSystem _System = null;


        protected override Color[,] Generate(Rectangle drawbox, IViewbox viewbox)
        {
            Result.maxIterations = 1;
            var plot = Plot(drawbox, viewbox, 2, System.Plot, Iterations);
            var color = Color(plot, System.Color);
            return color;

            //int f = 10;
            //var steps = new Complex(viewbox.Size.Real / drawbox.Width / f, viewbox.Size.Imaginary / drawbox.Height / f);

            //for (int xn = 0; xn < drawbox.Width * f; xn++)
            //{
            //    double x = (xn - drawbox.Left) * steps.Real + viewbox.Left;
            //    double y = Function(x).Real;

            //    int p = (int)xn / f;
            //    int q = (int)((-y + viewbox.Top) / steps.Imaginary + drawbox.Top) / f;

            //    if (p > 0 && p < drawbox.Width - 1 && q > 0 && q < drawbox.Height - 1)
            //    { 
            //        color[p, q] = Colors.White;
            //        color[p + 1, q] = Colors.White;
            //        color[p - 1, q] = Colors.White;
            //        color[p, q + 1] = Colors.White;
            //        color[p, q - 1] = Colors.White;
            //    }
            //}

            //return color;
        }

        public override string ToString() => "Dynamic System";

    }
}
