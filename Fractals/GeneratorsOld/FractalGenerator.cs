using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace Fractals.Generators
{
    public struct RootResult
    {
        public RootResult(int iterations, int root)
        {
            this.iterationCount = iterations;
            this.root = root;
        }

        public byte Brightness(int maxIterationCount)
        {
            return (byte)(255 - (iterationCount * 255 / maxIterationCount));
        }

        public int iterationCount;
        public int root;
    }

    public class BindableParameter : BindableBase
    {
        public bool IsVisible
        {
            get => _IsVisible;
            set { Set(ref _IsVisible, value, nameof(IsVisible)); }
        }
        public bool _IsVisible;

        public object Value
        {
            get => _Value;
            set { Set(ref _Value, value, nameof(Value)); }
        }
        public object _Value;
    }

    public abstract class FractalGenerator
    {
        public virtual int Iterations { get; set; } = 1;

        public virtual int MaxIterations { get; protected set; } = 100;

        public virtual int MinIterations { get; protected set; } = 1;

        public int ValuesInCondomain { get; protected set; } = 0;

        public int ValuesInDomain { get; protected set; } = 0;

        public abstract ValueRect DefaultValues { get; }

        public virtual List<BindableParameter> Parameters { get; protected set; }

        public abstract override string ToString();

        public abstract void UpdateBitmap(WriteableBitmap bitmap, ValueRect valueRange);   
    }
}
