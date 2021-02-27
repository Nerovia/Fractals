using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Fractals.View
{
    [ContentProperty(Name = "Children")]
    public sealed partial class PanelGroup : UserControl
    {
        public PanelGroup()
        {
            //Children = new List<UIElement>();
            //Spacing = 10;
            this.InitializeComponent();
        }


     

        public List<UIElement> Children
        {
            get { return (List<UIElement>)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Children.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register("Children", typeof(List<UIElement>), typeof(PanelGroup), new PropertyMetadata(new List<UIElement>()));





    }
}
