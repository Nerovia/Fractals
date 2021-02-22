using Fractals.Generators;
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

        public FractalViewmodel[] Generators { get; } =
        {
            new MandelbrotViewmodel(),
            new DynamicViewmodel(),
        };

        public FractalViewmodel Generator
        {
            get => _generator;
            set
            {
                if (_generator == value)
                    return;
                if (_generator != null)
                    _generator.PropertyChanged -= OnViewmodelPropertyChanged;
                
                _generator = value;

                if (_generator != null)
                    _generator.PropertyChanged += OnViewmodelPropertyChanged;

                OnPropertyChanged(nameof(Generator));
                Update();
            }
        }

        private void OnViewmodelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }

        private FractalViewmodel _generator;




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



        public int ZoomLevel
        {
            get => (int)Math.Log(_Zoom,1.2);
            set 
            {
                if (_ZoomLevel != value)
                {
                    _ZoomLevel = value;
                    Zoom = Math.Pow(1.2, _ZoomLevel);
                }
            }
        }
        private int _ZoomLevel;


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


        public Viewbox Viewbox { get; private set; } = new Viewbox(new Complex(-2, -2), new Complex(2, 2));


        private void Update()
        {
            Generator?.Update(Bitmap, Viewbox);
            Bitmap?.Invalidate();
            OnPropertyChanged(nameof(OriginX));
            OnPropertyChanged(nameof(OriginY));
            OnPropertyChanged(nameof(Zoom));
            OnPropertyChanged(nameof(ZoomLevel));
        }

        public void CreateBitmap()
        {
            if (Bitmap != null && Bitmap.PixelWidth == SelectedResolution)
                return;

            Bitmap = new WriteableBitmap(SelectedResolution, SelectedResolution);
            Update();
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


        public Complex PointerFocus
        {
            get => _PointerFocus;
            set { Set(ref _PointerFocus, value); }
        }
        private Complex _PointerFocus;


        private Complex GetPointerFocus(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            var pointer = e.GetCurrentPoint(element);
            var pointerPosition = pointer.Properties.ContactRect;

            return new Complex(pointerPosition.X * Viewbox.Size.Real / element.ActualWidth + Viewbox.Left,
                                    pointerPosition.Y * (-Viewbox.Size.Imaginary) / element.ActualHeight + Viewbox.Top);
        }

        public void Move(Complex offset)
        {
            Viewbox.Move(offset);
            Update();
        }

        public void Reset()
        {
            Viewbox = new Resources.Viewbox(new Complex(-1, -1), new Complex(1, 1));
            Update();
        }

        public void OnFractalPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerFocus = GetPointerFocus(sender, e);
        }

        public void OnFractalPointerExited(object sender, PointerRoutedEventArgs e)
        {
            PointerFocus = Viewbox.Origin;
        }


        public void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            var pointer = e.GetCurrentPoint(element);
            var pointerPosition = pointer.Properties.ContactRect;

            var focus = new Complex(pointerPosition.X * Viewbox.Size.Real / element.ActualWidth + Viewbox.Left,
                                    pointerPosition.Y * (-Viewbox.Size.Imaginary) / element.ActualHeight + Viewbox.Top);
        }

    }
}
