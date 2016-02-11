using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MediaCapture _mediaCapture = new MediaCapture();
        private Result _result;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void but1_Click_1(object sender, RoutedEventArgs e)
        {

        }

       
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                if (cameras.Count < 1)
                {
                    Error.Text = "No camera found, decoding static image";
                    await DecodeStaticResource();
                    return;
                }
                MediaCaptureInitializationSettings settings;
                if (cameras.Count == 1)
                {
                    settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[0].Id }; // 0 => front, 1 => back
                }
                else
                {
                    settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[1].Id }; // 0 => front, 1 => back
                }

                await _mediaCapture.InitializeAsync(settings);
                VideoCapture.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();

                while (_result == null)
                {
                    var photoStorageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("scan.jpg", CreationCollisionOption.GenerateUniqueName);
                    await _mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), photoStorageFile);

                    var stream = await photoStorageFile.OpenReadAsync();
                    // initialize with 1,1 to get the current size of the image
                    var writeableBmp = new WriteableBitmap(1, 1);
                    writeableBmp.SetSource(stream);
                    // and create it again because otherwise the WB isn't fully initialized and decoding
                    // results in a IndexOutOfRange
                    writeableBmp = new WriteableBitmap(writeableBmp.PixelWidth, writeableBmp.PixelHeight);
                    stream.Seek(0);
                    writeableBmp.SetSource(stream);

                    _result = ScanBitmap(writeableBmp);

                    await photoStorageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                await _mediaCapture.StopPreviewAsync();
                VideoCapture.Visibility = Visibility.Collapsed;
                CaptureImage.Visibility = Visibility.Visible;
                ScanResult.Text = _result.Text;
            }
            catch (Exception ex)
            {
                Error.Text = ex.Message;
            }
        }
        private async System.Threading.Tasks.Task DecodeStaticResource()
        {
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\1.jpg");
            var stream = await file.OpenReadAsync();
            // initialize with 1,1 to get the current size of the image
            var writeableBmp = new WriteableBitmap(1, 1);
            writeableBmp.SetSource(stream);
            // and create it again because otherwise the WB isn't fully initialized and decoding
            // results in a IndexOutOfRange
            writeableBmp = new WriteableBitmap(writeableBmp.PixelWidth, writeableBmp.PixelHeight);
            stream.Seek(0);
            writeableBmp.SetSource(stream);
            CaptureImage.Source = writeableBmp;
            VideoCapture.Visibility = Visibility.Collapsed;
            CaptureImage.Visibility = Visibility.Visible;

            _result = ScanBitmap(writeableBmp);
            if (_result != null)
            {
                ScanResult.Text += _result.Text;
            }
            return;
        }

        private Result ScanBitmap(WriteableBitmap writeableBmp)
        {
            var barcodeReader = new BarcodeReader
            {
                TryHarder = true,
                AutoRotate = true
            };
            var result = barcodeReader.Decode(writeableBmp);

            if (result != null)
            {
                CaptureImage.Source = writeableBmp;
            }

            return result;
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await _mediaCapture.StopPreviewAsync();
        }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            // Whatever byte[] you're trying to convert.
            byte[] imageBytes  = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }; ; //= (value as FileAttachment).ContentBytes;

            BitmapImage image = new BitmapImage();
            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            ms.AsStreamForWrite().Write(imageBytes, 0, imageBytes.Length);
            ms.Seek(0);

            image.SetSource(ms);
            ImageSource src = image;
            image1.Source = src;
            return src;
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            //            //byte[] imageBytes = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }; ; //= (value as FileAttachment).ContentBytes;

            var imageBytes = new byte[7000];
            for (int i = 0; i < imageBytes.Length; i++)
            {
                imageBytes[i] = 0x2;
            }

            //            BitmapImage image = new BitmapImage();
            //            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            //            ms.AsStreamForWrite().Write(imageBytes, 0, imageBytes.Length);
            ////            ms.Seek(0);

            //            image.SetSource(ms);
            //            ImageSource src = image;
            //            image1.Source = src;


            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            BitmapImage image = new BitmapImage();
            await stream.WriteAsync(imageBytes.AsBuffer());
            stream.Seek(0);
            image.SetSource(stream);
            ImageSource m = image;
            image1.Source = m;

        }
    }
}