using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals.Generators
{
    public abstract class ParameterViewmodel : BindableBase
    {
        public event EventHandler ParameterChanged;
        protected void OnParameterChanged()
        {
            ParameterChanged?.Invoke(this, null);
        }
    }

    public class ValueParameterViewmodel : ParameterViewmodel
    {
        public ValueParameterViewmodel(double min, double max, double @default, double steps = 0.1)
        {
            Min = min;
            Max = max;
            Default = @default;
            Steps = steps;
        }

        public double Min { get; protected set; }
        public double Max { get; protected set; }
        public double Default { get; protected set; }
        public double Steps { get; set; }

        public double Value
        {
            get => _Value;
            set 
            {
                if (Set(ref _Value, value))
                    OnParameterChanged();
            }
        }
        private double _Value;
    }

    public class SelectionParameterViewmodel : ParameterViewmodel
    {
        public SelectionParameterViewmodel(object[] items)
        {
            Items = items;
            if (items.Length > 0)
                Selected = Items[0];
            else
                Selected = null;
        }

        public object[] Items { get; protected set; }

        public object Selected
        {
            get => _Selected;
            set 
            {
                if (Set(ref _Selected, value))
                    OnParameterChanged();
            }
        }
        private object _Selected;
    }
}
