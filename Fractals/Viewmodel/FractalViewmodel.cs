using Fractals.Generators;
using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Viewmodel
{
    public class FractalViewmodel : BindableBase
    {
        #region Constructor



        public FractalViewmodel()
        {
            CreateBitmap();
            Generator = Generators.FirstOrDefault();
        }



        #endregion

        #region Properties


        public int ValuesInDomain { get => Generator.ValuesInDomain; }


        public int ValuesInCondomain { get => Generator.ValuesInCondomain; }


        public int Iterations
        {
            get => Generator.Iterations;
            set
            {
                if (Generator.Iterations != value)
                {
                    Generator.Iterations = value;
                    OnPropertyChanged(nameof(Iterations));
                    Refresh();
                }
            }
        }

        public int MaxIterations { get => Generator.MaxIterations; }

        public int MinIterations { get => Generator.MinIterations; }




        public double R
        {
            get
            {
                if (Generator is JuliaSet)
                    return (Generator as JuliaSet).R;
                else
                    return 0;
            }
            set
            {
                if (Generator is JuliaSet)
                {
                    var g = Generator as JuliaSet;
                    if (g.R != value)
                    {
                        g.R = value;
                        OnPropertyChanged(nameof(R));
                        Refresh();
                    }
                }
            }
        }


        public double ImC
        {
            get
            {
                if (Generator is JuliaSet)
                    return (Generator as JuliaSet).C.Imaginary;
                else
                    return 0;
            }
            set
            {
                if (Generator is JuliaSet)
                {
                    var g = Generator as JuliaSet;
                    if (g.C.Imaginary != value)
                    {
                        g.C = new Complex(g.C.Real, value);
                        OnPropertyChanged(nameof(ImC));
                        Refresh();
                    }
                }
            }
        }


        public double ReC
        {
            get
            {
                if (Generator is JuliaSet)
                    return (Generator as JuliaSet).C.Real;
                else
                    return 0;
            }
            set
            {
                if (Generator is JuliaSet)
                {
                    var g = Generator as JuliaSet;
                    if (g.C.Real != value)
                    {
                        g.C = new Complex(value, g.C.Imaginary); ;
                        OnPropertyChanged(nameof(ReC));
                        Refresh();
                    }
                }
            }
        }



        public FractalGenerator[] Generators { get; } =
        {
            new NewtonMethode(),
            new NewtonFractal(),
            new Mandelbrot(),
            new SierpinskiCarpet(),
            new JuliaSet(),
        };


        public int[] Resolutions { get; } = { 10, 50, 100, 200, 400, 600, 800, 1000, 2000 };



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


      

        public FractalGenerator Generator
        {
            get => _Generator;
            set
            {
                if (Set(ref _Generator, value))
                {
                    Values = _Generator.DefaultValues;
                    OnPropertyChanged(nameof(Iterations));
                    OnPropertyChanged(nameof(MinIterations));
                    OnPropertyChanged(nameof(MaxIterations));
                    Refresh();
                }
            }
        }
        private FractalGenerator _Generator;


        //public double HorizontalOffset { get => Values.left * Zoom + 1; }
        //public double VerticalOffset { get => Values.top * Zoom + 1; }

        public double HorizontalOffset
        {
            get => _HorizontalOffset;
            set 
            {
                if (Set(ref _HorizontalOffset, value))
                    UpdateFrame();
            }
        }
        private double _HorizontalOffset;

        public double VerticalOffset
        {
            get => _VerticalOffset;
            set 
            {
                if (Set(ref _VerticalOffset, value))
                    UpdateFrame();
            }
        }
        private double _VerticalOffset;

        public double Zoom
        {
            get => _Zoom;
            set 
            {
                if (Set(ref _Zoom, value))
                    UpdateFrame();
            }
        }
        private double _Zoom = 1;


        public WriteableBitmap Bitmap
        {
            get => _Bitmap;
            set { Set(ref _Bitmap, value); }
        }
        private WriteableBitmap _Bitmap;



        #endregion

        #region Private Fields



   
        public ValueRect Values
        {
            get => _Values;
            set 
            {
                if (Set(ref _Values, value))
                {
                    OnPropertyChanged(nameof(HorizontalOffset));
                    OnPropertyChanged(nameof(VerticalOffset));
                }
            }
        }
        private ValueRect _Values = new ValueRect(-1, -1, 1, 1);


        public double ZoomSpeed
        {
            get => _ZoomSpeed;
            set { Set(ref _ZoomSpeed, value); }
        }
        private double _ZoomSpeed = 1.2;




        #endregion

        #region Public Fields



        // No Public Fields



        #endregion

        #region Private Methodes



        private void Refresh()
        {
            Generator?.UpdateBitmap(Bitmap, Values);
            Bitmap?.Invalidate();
            OnPropertyChanged(nameof(ValuesInCondomain));
            OnPropertyChanged(nameof(ValuesInDomain));
        }

        public void image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            var point = e.GetCurrentPoint(element);
            var position = point.Properties.ContactRect;

            //double xPos = (position.X / element.ActualWidth * 2) - 1;
            //double yPos = (position.Y / element.ActualHeight * 2) - 1;



            double width = Values.Width;
            double height = Values.Height;
            double offsetX = Values.left;
            double offsetY = Values.top;

            double xPos = position.X * (width) / (element.ActualWidth) + offsetX;
            double yPos = position.Y * (height) / (element.ActualHeight) + offsetY;

            

            if (point.Properties.MouseWheelDelta < 0)
            {
                _Zoom /= ZoomSpeed;

                // Change the size of the picturebox, multiply it by the ZOOMFACTOR
                width = width * ZoomSpeed;
                height = height * ZoomSpeed;

                // Formula to move the picturebox, to zoom in the point selected by the mouse cursor
                offsetY = yPos - (yPos - offsetY) * ZoomSpeed;
                offsetX = xPos - (xPos - offsetX) * ZoomSpeed;

            }
            else
            {
                _Zoom *= ZoomSpeed;

                // Change the size of the picturebox, divide it by the ZOOMFACTOR
                width = width / ZoomSpeed;
                height = height / ZoomSpeed;

                // Formula to move the picturebox, to zoom in the point selected by the mouse cursor
                offsetY = yPos - (yPos - offsetY) / ZoomSpeed;
                offsetX = xPos - (xPos - offsetX) / ZoomSpeed;
            }

            _HorizontalOffset = offsetX - (-1 / Zoom);
            _VerticalOffset = offsetY - (-1 / Zoom);

            OnPropertyChanged(nameof(HorizontalOffset));
            OnPropertyChanged(nameof(VerticalOffset));
            OnPropertyChanged(nameof(Zoom));

            Values = new ValueRect(offsetX, offsetY, width + offsetX, height + offsetY);
            Refresh();
        }

        public void ResetFrame()
        {
            Values = Generator.DefaultValues;
            Refresh();
        }

        public void UpdateFrame()
        {
            double width = 2 / Zoom;
            double height = 2 / Zoom;
            double offsetX = -1 / Zoom + HorizontalOffset;
            double offsetY = -1 / Zoom + VerticalOffset;

            Values = new ValueRect(offsetX, offsetY, width + offsetX, height + offsetY);
            Refresh();
        }

        #endregion

        #region Public Methodes



        public void CreateBitmap()
        {
            if (Bitmap != null && Bitmap.PixelWidth == SelectedResolution)
                return;

            Bitmap = new WriteableBitmap(SelectedResolution, SelectedResolution);
            Refresh();
        }


        private WriteableBitmap ScaleBitmap(WriteableBitmap original, int width, int height)
        {
            return original.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
        }

        public Complex Complexi { get; } = new Complex(1, 1);

        #endregion

        #region Events



        // No Events



        #endregion

    }
}
