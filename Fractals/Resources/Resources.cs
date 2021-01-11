using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals.Resources
{
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
        public int Width { get => Math.Abs(right - left); }
        public int Height { get => Math.Abs(bottom - top); }

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
        public double Width { get => Math.Abs(right - left); }
        public double Height { get => Math.Abs(bottom - top); }

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

        public override string ToString()
        {
            return $"({left}, {top}), ({right}, {bottom})";
        }
    }

}
