using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fractals.Resources
{    
    public class Viewbox
    {
        public Viewbox(Complex bottomLeft, Complex topRight)
        {
            Frame(bottomLeft, topRight);
        }
        
        public Viewbox(double left, double top, double width, double height)
        {
            Frame(new Complex(left, top - height), new Complex(left + width, top));
        }

        public double Top { get => TopRight.Imaginary; }

        public double Bottom { get => BottomLeft.Imaginary; }

        public double Right { get => TopRight.Real; }

        public double Left { get => BottomLeft.Real; }

        public Complex BottomLeft { get; set; }

        public Complex TopRight { get; set; }

        public Complex Origin
        {
            get => (TopRight + BottomLeft) / 2;
            set
            {
                var offset = value - Origin;
                BottomLeft += offset;
                TopRight += offset;
            }
        }

        public Complex Size
        {
            get => TopRight - BottomLeft;
            set
            {
                var size = value / 2;
                var origin = Origin;
                BottomLeft = origin - size;
                TopRight = origin + size;
            }
        }

        public void Zoom(double factor)
        {
            Zoom(factor, Origin);
        }

        public void Zoom(double factor, Complex focus)
        {
            Size /= factor;
            Origin = focus - (focus - Origin) / factor;
        }

        public void Frame(Complex bottomLeft, Complex topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public void Move(Complex offset)
        {
            Origin += offset;
        }

        public override string ToString()
        {
            return $"{BottomLeft} {TopRight} : {Origin}";
        }
    }
}
