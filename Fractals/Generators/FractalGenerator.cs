using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Color = Windows.UI.Color;

namespace Fractals.Generators
{
    public abstract class FractalGenerator : IFractal
    {
        protected class PlotArgs
        {

            public IViewbox viewbox;
            public Rectangle drawbox;
            public object[,] plot;
        }

        public int Iterations { get; set; }

        public virtual void Update(WriteableBitmap bitmap, IViewbox viewbox)
        {
            Rectangle drawbox = new Rectangle(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            var plot = Generate(drawbox, viewbox);
            var pixeldata = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            //int w = bitmap.PixelWidth / drawbox.Width;
            //int h = bitmap.PixelHeight / drawbox.Height;

            int n = 0;
            for (int y = 0; y < drawbox.Height; y++)
            {
                //for (int a = 0; a < h; a++)
                //{
                    for (int x = 0; x < drawbox.Width; x++)
                    {
                        //for (int b = 0; b < w; b++)
                        //{
                            var color = plot[x, y];
                            pixeldata[n++] = color.B;
                            pixeldata[n++] = color.G;
                            pixeldata[n++] = color.R;
                            pixeldata[n++] = 255;
                        //}
                    }
                //}
            }

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixeldata, 0, pixeldata.Length);
            }
        }

        protected abstract Color[,] Generate(Rectangle drawbox, IViewbox viewbox);

        public abstract override string ToString();

        protected virtual object[,] Plot(Rectangle drawbox, IViewbox viewbox, PlotMethode methode, int iterations)
        {
            var steps = new Complex(viewbox.Width / drawbox.Width, viewbox.Height / drawbox.Height);
            var plot = new object[drawbox.Width, drawbox.Height];


            for (int y = drawbox.Top; y < drawbox.Bottom; y++)
            {
                for (int x = drawbox.Left; x < drawbox.Right; x++)
                {
                    Complex z = new Complex((x - drawbox.Left) * steps.Real + viewbox.Left, (y - drawbox.Top) * steps.Imaginary - viewbox.Top);
                    plot[x, y] = methode(z, iterations);
                }
            }


            return plot;
        }

        protected virtual object[,] Plot(Rectangle drawbox, IViewbox viewbox, int workslpit, PlotMethode methode, int iterations)
        {
            var plot = new object[drawbox.Width, drawbox.Height];
            List<PlotArgs> args = new List<PlotArgs>();
            List<Task> tasks = new List<Task>();

            int dfx = drawbox.Width / workslpit;
            int dfy = drawbox.Height / workslpit;
            double vfx = viewbox.Width / workslpit;
            double vfy = viewbox.Height / workslpit;

            for (int y = 0; y < workslpit; y++)
            {
                for (int x = 0; x < workslpit; x++)
                {
                    args.Add(new PlotArgs()
                    {
                        viewbox = new SimpleViewbox(x * vfx + viewbox.Left, -y * vfy + viewbox.Top, vfx, vfy),
                        drawbox = new Rectangle(x * dfx, y * dfy, dfx, dfy),
                        plot = plot,
                    });
                }
            }

            foreach (var arg in args)
            {
                tasks.Add(Task.Run(() =>
                {
                    var steps = new Complex(arg.viewbox.Width/ arg.drawbox.Width, arg.viewbox.Height / arg.drawbox.Height);

                    for (int y = arg.drawbox.Top; y < arg.drawbox.Bottom; y++)
                    {
                        for (int x = arg.drawbox.Left; x < arg.drawbox.Right; x++)
                        {
                            Complex z = new Complex((x - arg.drawbox.Left) * steps.Real + arg.viewbox.Left, (y - arg.drawbox.Top) * steps.Imaginary - arg.viewbox.Top);
                            arg.plot[x, y] = methode(z, iterations);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return plot;
        }

        protected virtual Color[,] Color(object[,] plot, ColorMethode methode)
        {
            var width = plot.GetLength(0);
            var height = plot.GetLength(1);
            var colors = new Color[width, height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    colors[x, y] = methode(plot[x, y]);

            return colors;
        }

        
    }

    public delegate Color ColorMethode(object element);
    public delegate object PlotMethode(Complex z, int iterations);


    public interface IFractal
    {
        int Iterations { get; set; }
        void Update(WriteableBitmap bitmap, IViewbox viewbox);
    }

    public interface IJuliaSet : IFractal
    {
        Complex JuliaConstant { get; set; }
        bool JuliaMode { get; set; }
    }
}
