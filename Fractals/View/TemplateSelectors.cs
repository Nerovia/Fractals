using Fractals.Viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fractals.View
{
    public class FractalViewmodelSelector : DataTemplateSelector
    {
        public DataTemplate MandelbrotTemplate { get; set; }
        public DataTemplate DynamicTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is null)
                return null;
            else if (item.GetType() == typeof(MandelbrotViewmodel))
                return MandelbrotTemplate;
            else if (item.GetType() == typeof(DynamicViewmodel))
                return DynamicTemplate;
            else
                return null;
        }
    }
}
