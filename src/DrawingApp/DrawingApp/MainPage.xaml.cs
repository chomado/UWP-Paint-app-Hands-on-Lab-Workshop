using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace DrawingApp
{
    public sealed partial class MainPage : Page
    {
        private CanvasBitmap _image;
        public MainPage()
        {
            this.InitializeComponent();

            inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen |
                CoreInputDeviceTypes.Mouse |
                CoreInputDeviceTypes.Touch;
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);
                picture.Source = image;

                // 後で保存するための画像を保持
                var device = CanvasDevice.GetSharedDevice();
                _image = await CanvasBitmap.LoadAsync(device, stream);
            }
        }

        private void Picture_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // キャンバスサイズを画像と合わせる
            inkCanvas.Width = picture.ActualWidth;
            inkCanvas.Height = picture.ActualHeight;
        }

        private async void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeChoices.Add("png", new List<string> { ".png" });
            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            var device = CanvasDevice.GetSharedDevice();
            var renderTarget = new CanvasRenderTarget(device, (int)_image.Size.Width, (int)_image.Size.Height, 96);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawImage(_image, 0, 0);
                ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }

            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
            }
        }
    }
}
