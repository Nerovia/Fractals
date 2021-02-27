using Fractals.Generators;
using Fractals.Resources;
using Fractals.Viewmodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Fractals
{
    public sealed partial class MainPage : Page
    {
        public MainViewmodel Viewmodel { get; } = new MainViewmodel();

        public MainPage()
        {
            this.InitializeComponent();
            //SharedShadow.Receivers.Add(ShadowReceiver);

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(30, 0, 0, 0);

        }

        private void image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Viewmodel.OnPointerWheelChanged(sender, e);
        }

        

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".png" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = Viewmodel.Generator.ToString();

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file == null)
                return;
            
            Windows.Storage.CachedFileManager.DeferUpdates(file);
            // write to file

            await SaveWriteableBitmapToFile(Viewmodel.Bitmap, file);

            Debug.WriteLine($"Exported {file.DisplayName}");
            Debug.WriteLine($"Horizontal Min/Max");
            Debug.WriteLine($"[{Viewmodel.Viewbox.Left}, {Viewmodel.Viewbox.Right}]");
            Debug.WriteLine($"Vertical Min/Max");
            Debug.WriteLine($"[{Viewmodel.Viewbox.Bottom}, {Viewmodel.Viewbox.Top}]");
            Debug.WriteLine("");

            Windows.Storage.Provider.FileUpdateStatus status =
                await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
        }

        private async Task SaveWriteableBitmapToFile(WriteableBitmap writeableBitmap, StorageFile outputFile)
        {
            SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                writeableBitmap.PixelBuffer,
                BitmapPixelFormat.Bgra8,
                writeableBitmap.PixelWidth,
                writeableBitmap.PixelHeight
            );

            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.NearestNeighbor;
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Reset();
        }

        private void image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Viewmodel.OnFractalPointerMoved(sender, e);
        }

        private void image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Viewmodel.OnPointerPressed(sender, e);
        }

        private void image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Viewmodel.OnFractalPointerMoved(sender, e);
        }

        private void image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Viewmodel.OnFractalPointerExited(sender, e);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        private void image_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
                return;

            var element = sender as Windows.UI.Xaml.Controls.Image;
            var viewbox = Viewmodel.Viewbox;
            var translation = e.Delta.Translation;
            var scale = e.Delta.Scale;
            var position = e.Position;
            var size = GetImageSize();

            var offset = new Complex(-translation.X * viewbox.Width / size ,
                                     -translation.Y * (-viewbox.Height) / size );

            var focus = new Complex(position.X * viewbox.Width / element.ActualWidth + viewbox.Left,
                                    position.Y * (-viewbox.Height) / element.ActualHeight + viewbox.Top);



            Viewmodel.Move(offset);
            Viewmodel.Viewbox.Zoom(scale, focus);
        }

        public double GetImageSize()
        {
            var width = Viewbox.ActualWidth;
            var height = Viewbox.ActualHeight;
            var size = Math.Min(width, height);
            return size;
        }

    }
}
