using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Fractals.Resources
{
    public static class DomainColor
    {
        static double s = 0.8;
        static double a = 0.3;

        public static Color Generate(Complex z)
        {
            double h = z.Phase * 180.0 / Math.PI;
            double v = 1.0 - Math.Pow(a, Math.Abs(z.Magnitude));
            return HsvToRgb(h, s, v);
        }

        public static Color Generate(double x)
        {
            double h = x * 120.0;
            double v = 1.0; //1.0 - Math.Pow(a, Math.Abs(x));
            return HsvToRgb(h, s, v);
        }

        public static Color HsvToRgb(double h, double S, double V)
        {
            double H = h;
            S = Math.Clamp(S, 0.0, 1.0);
            V = Math.Clamp(V, 0.0, 1.0);
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }

            Color color = new Color();

            color.R = (byte)Math.Clamp(R * 255.0, 0, 255);
            color.G = (byte)Math.Clamp(G * 255.0, 0, 255);
            color.B = (byte)Math.Clamp(B * 255.0, 0, 255);
            color.A = 255;

            return color;
        }
    }

    public static class Converters
    {
        public static int AbsoluteHue(int hue)
        {
            int h = hue % 360;
            if (h < 0)
                return 360 + h;
            else
                return h;
        }

        

    }

    public struct PixelRect
    {
        public PixelRect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public readonly int top;
        public readonly int bottom;
        public readonly int left;
        public readonly int right;
        public int Width { get => (right - left); }
        public int Height { get => (bottom - top); }

        public PixelRect GetFraction(int fraction, int x, int y)
        {
            if (fraction < 0)
                throw new ArgumentException("fraction must be bigger than 0.");
            if (x < 0 || x >= fraction)
                throw new ArgumentException("x must be smaller than the fraction and bigger than 0.");
            if (y < 0 || y >= fraction)
                throw new ArgumentException("y must be smaller than the fraction and bigger than 0.");

            int fx = (left + right) / fraction;
            int fy = (top + bottom) / fraction;

            return new PixelRect(x * fx, y * fy, (x + 1) * fx, (y + 1) * fy);
        }

        public override string ToString()
        {
            return $"({left}, {top}), ({right}, {bottom})";
        }
    }

    public struct ValueRect
    {
        public ValueRect(double left, double top, double right, double bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public readonly double top;
        public readonly double bottom;
        public readonly double left;
        public readonly double right;
        public double Width { get => (right - left); }
        public double Height { get => (bottom - top); }

        public ValueRect GetFraction(int fraction, int x, int y)
        {
            if (fraction < 0)
                throw new ArgumentException("fraction must be bigger than 0.");
            if (x < 0 || x >= fraction)
                throw new ArgumentException("x must be smaller than the fraction and bigger than 0.");
            if (y < 0 || y >= fraction)
                throw new ArgumentException("y must be smaller than the fraction and bigger than 0.");

            double fx = Width / fraction;
            double fy = Height / fraction;
            return new ValueRect(x * fx + left, y * fy + top, (x + 1) * fx + left, (y + 1) * fy + top);
        }

        public double CenterX => Width / 2 + left;

        public double CenterY => Height / 2 + top;

        public override string ToString()
        {
            return $"({left}, {top}), ({right}, {bottom})";
        }
    }

}
