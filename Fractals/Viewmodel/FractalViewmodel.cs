using Fractals.Generators;
using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Viewmodel
{
    public class FractalViewmodel : BindableBase
    {
        public FractalViewmodel(IFractal generator)
        {
            Model = generator;
        }

        protected IFractal Model { get; private set; }

        public int Iterations
        {
            get => Model.Iterations;
            set
            {
                value = Math.Clamp(value, IterationsMin, IterationsMax);
                if (Model.Iterations != value)
                {
                    Model.Iterations = value;
                    OnPropertyChanged(nameof(Iterations));
                }
            }
        }

        public virtual int IterationsMin { get; } = 0;

        public virtual int IterationsMax { get; } = 1000;

        public virtual IViewbox DefaultViewbox { get; } = new SimpleViewbox(-1, 1, 2, 2);

        public override string ToString()
        {
            return Model.ToString();
        }

        public void Update(WriteableBitmap bitmap, IViewbox viewbox)
        {
            Model.Update(bitmap, viewbox);
        }

        public virtual void OnFractalPressed(Complex origin, PointerPointProperties e) { }

        public virtual void OnFractalReleased(Complex origin, PointerPointProperties e) { }
    }

    public class JuliaViewmodel : FractalViewmodel
    {
        public JuliaViewmodel(IJuliaSet generator) : base(generator)
        {
        }

        protected new IJuliaSet Model => base.Model as IJuliaSet;

        public override IViewbox DefaultViewbox { get; } = new SimpleViewbox(-1, 1, 2, 2);

        public double JuliaConstantReal
        {
            get => Model.JuliaConstant.Real;
            set
            {
                if (Model.JuliaConstant.Real != value)
                {
                    Model.JuliaConstant = new Complex(value, Model.JuliaConstant.Imaginary);
                    OnPropertyChanged(nameof(JuliaConstantReal));
                }
            }
        }

        public double JuliaConstantImaginary
        {
            get => Model.JuliaConstant.Imaginary;
            set
            {
                if (Model.JuliaConstant.Imaginary != value)
                {
                    Model.JuliaConstant = new Complex(Model.JuliaConstant.Real, value);
                    OnPropertyChanged(nameof(JuliaConstantReal));
                }
            }
        }


        public bool JuliaMode
        {
            get => Model.JuliaMode;
            set
            {
                if (Model.JuliaMode != value)
                {
                    Model.JuliaMode = value;
                    OnPropertyChanged(nameof(JuliaMode));
                }
            }
        }

        public bool juliaPreview;


        public override void OnFractalPressed(Complex origin, PointerPointProperties e)
        {
            if (e.IsLeftButtonPressed)
            {
                juliaPreview = JuliaMode;
                Model.JuliaMode = true;

                Model.JuliaConstant = origin;
                OnPropertyChanged("");
            }
        }

        public override void OnFractalReleased(Complex origin, PointerPointProperties e)
        {
            if (e.IsLeftButtonPressed)
            {
                JuliaMode = juliaPreview;
            }
        }
    }

    public class DynamicViewmodel : FractalViewmodel
    {
        public DynamicViewmodel() : base(new DynamicGenerator())
        {
        }

        protected new DynamicGenerator Model => base.Model as DynamicGenerator;

        public IDynamicSystem[] Systems { get; } =
        {
            DynamicSystem.Default,
            new NewtonMethodeSystem("x³ - x", (Complex z) => new Complex(Math.Pow(z.Real, 3) - z.Real, 0), (Complex z) => new Complex(3 * Math.Pow(z.Real, 2) - 1, 0)) { Color = (object e) => DomainColor.Generate(((Complex)e).Real) },
            new NewtonMethodeSystem("z³ - 1", (Complex z) => Complex.Pow(z, 3) - 1, (Complex z) => 3 * Complex.Pow(z, 2)),
            new NewtonMethodeSystem("sin(z)", (Complex z) => Complex.Sin(z), (Complex z) => Complex.Cos(z)),
            new NewtonMethodeSystem("sin(z) - 1", (Complex z) => Complex.Sin(z) - 1, (Complex z) => Complex.Cos(z)),
            new NewtonMethodeSystem("tan(z)", (Complex z) => Complex.Tan(z), (Complex z) => Complex.Pow(Complex.Tan(z), 2) - 1),
            new NewtonMethodeSystem("z⁸ + 15z⁴ - 16", (Complex z) => Complex.Pow(z, 8) + 15 * Complex.Pow(z, 4) - 16, (Complex z) =>  8 * Complex.Pow(z, 7) + 15 * 4 * Complex.Pow(z, 3)),
            new DynamicSystem("z² + c") { Plot = (Complex z, int iterations) =>
            {
                Complex c = z;
                for (int i = 0; i < iterations; i++)
                    z = Complex.Pow(z, 2) + c;
                return z;
            }},
        };


        
        public IDynamicSystem System
        {
            get => Model.System;
            set
            {
                if (Model.System != value)
                {
                    Model.System = value;
                    OnPropertyChanged(nameof(System));
                }
            }
        }

    }

   
}
