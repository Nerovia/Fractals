using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals.Viewmodel
{
    public class ExportViewmodel : BindableBase
    {
        public ExportViewmodel(int width, int height)
        {
            Width = width;
            Height = height;
        }


        public string Name
        {
            get => _Name;
            set
            {
                if (Set(ref _Name, value))
                {
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(Height));
                }
            }
        }
        private string _Name;


        public int Scale
        {
            get => _Scale;
            set { Set(ref _Scale, value); }
        }
        private int _Scale;


        public int Width
        {
            get => _Width * Scale;
            private set => _Width = value;
        }
        private int _Width;


        public int Height
        {
            get => _Height * Scale;
            private set  => _Height = value;
        }
        private int _Height;
    }
}
