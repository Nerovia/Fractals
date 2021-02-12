using Fractals.Generators2;
using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Viewmodel
{
    public class MainViewmodel : BindableBase
    {
        public MainViewmodel()
        {
            CreateBitmap();
            Generator = Generators.FirstOrDefault();
        }

        public FractalGenerator2[] Generators { get; set; } =
        {
            new DynamicSystemPlotter(),
            new Mandelbrot2(),
        };

        public FractalGenerator2 Generator
        {
            get => _Generator;
            set
            {
                if (Set(ref _Generator, value))
                {
                    Update();
                }
            }
        }
        private FractalGenerator2 _Generator;

        public int[] Resolutions { get; } = { 10, 50, 100, 200, 400, 600, 800, 1000, 2000, 3000 };

        public int SelectedResolution
        {
            get => _SelectedResolution;
            set
            {
                if (Set(ref _SelectedResolution, value))
                    CreateBitmap();
            }
        }
        private int _SelectedResolution = 200;


        public int Iterations
        {
            get => Generator.Iterations;
            set
            {
                if (Generator.Iterations != value)
                {
                    Generator.Iterations = value;
                    OnPropertyChanged(nameof(Iterations));
                    Update();
                }
            }
        }


        public double OriginX
        {
            get => Viewbox.Origin.Real;
            set
            {
                Viewbox.Origin = new Complex(value, Viewbox.Origin.Imaginary);
                Update();
            }
        }

        public double OriginY
        {
            get => Viewbox.Origin.Imaginary;
            set
            {
                Viewbox.Origin = new Complex(Viewbox.Origin.Real, value);
                Update();
            }
        }


        public double Zoom
        {
            get => _Zoom;
            set 
            { 
                Viewbox.Zoom(value / _Zoom);
                _Zoom = value;
                Update();
            }
        }
        private double _Zoom = 1;


        public WriteableBitmap Bitmap
        {
            get => _Bitmap;
            set 
            { 
                if (Set(ref _Bitmap, value))
                    Update();
            }
        }
        private WriteableBitmap _Bitmap;


        public Viewbox Viewbox { get; private set; } = new Viewbox(new Complex(-1, -1), new Complex(1, 1));


        private void Update()
        {
            Generator?.Update(Bitmap, Viewbox);
            Bitmap?.Invalidate();
            OnPropertyChanged(nameof(OriginX));
            OnPropertyChanged(nameof(OriginY));
            OnPropertyChanged(nameof(Zoom));
        }

        public void CreateBitmap()
        {
            if (Bitmap != null && Bitmap.PixelWidth == SelectedResolution)
                return;

            Bitmap = new WriteableBitmap(SelectedResolution, SelectedResolution);
            Update();
        }

        public double Map(double x, double xmin, double xmax, double ymin, double ymax)
        {
            return (x - xmin) * (ymax - ymin) / (xmax - xmin) + xmax;
        }


        public void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            var pointer = e.GetCurrentPoint(element);
            var pointerPosition = pointer.Properties.ContactRect;

            var focus = new Complex(pointerPosition.X * Viewbox.Size.Real / element.ActualWidth + Viewbox.Left, 
                                    pointerPosition.Y * (-Viewbox.Size.Imaginary) / element.ActualHeight + Viewbox.Top);

           
            if (pointer.Properties.MouseWheelDelta < 0)
            {
                Viewbox.Zoom(0.8, focus);
                _Zoom /= 0.8;
            }
            else
            {
                Viewbox.Zoom(1.2, focus);
                _Zoom /= 1.2;
            }
            Update();
        }

        public void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            var pointer = e.GetCurrentPoint(element);
            var pointerPosition = pointer.Properties.ContactRect;

            var focus = new Complex(pointerPosition.X * Viewbox.Size.Real / element.ActualWidth + Viewbox.Left,
                                    pointerPosition.Y * (-Viewbox.Size.Imaginary) / element.ActualHeight + Viewbox.Top);

            if (pointer.Properties.IsMiddleButtonPressed)
            {
                Viewbox.Origin = focus;
            }
            else if (pointer.Properties.IsRightButtonPressed)
            {
                Viewbox.Zoom(0.8, focus);
                _Zoom /= 0.8;
            }
            else if (pointer.Properties.IsLeftButtonPressed)
            {
                Viewbox.Zoom(1.2, focus);
                _Zoom /= 1.2;
            }
            Update();

            e.Handled = true;
        }

    }
}
