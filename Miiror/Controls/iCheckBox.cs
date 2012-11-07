using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;

namespace AjaxMiniLite.controls.checkbox
{
    public class iCheckBox : CheckBox
    {
        #region Dependency properties
        public static DependencyProperty CheckImageProperty = DependencyProperty.Register("CheckImage", typeof(ImageSource), typeof(iCheckBox),
            new FrameworkPropertyMetadata(new BitmapImage(), FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty UnCheckImageProperty = DependencyProperty.Register("UnCheckImage", typeof(ImageSource), typeof(iCheckBox),
            new FrameworkPropertyMetadata(new BitmapImage(), FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Construction
        static iCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(iCheckBox), new FrameworkPropertyMetadata(typeof(iCheckBox)));
        }
        #endregion

        #region Public properties
        public BitmapImage MarkImage
        {
            set
            {
                SetValue(CheckImageProperty, value as ImageSource);
                SetValue(UnCheckImageProperty, ToGrayScale(value));

                (VisualTreeHelper.GetChild(this, 0) as Image).Source = CheckImage;
            }
        }

        private ImageSource CheckImage
        {
            get
            {
                if (!IsChecked.HasValue)
                    return (ImageSource)GetValue(UnCheckImageProperty);
                else
                {
                    if (IsChecked.Value)
                        return (ImageSource)GetValue(CheckImageProperty);
                    else
                        return (ImageSource)GetValue(UnCheckImageProperty);
                }
            }
        }
        #endregion

        #region Protected overrides
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == CheckBox.IsCheckedProperty)
            {
                (VisualTreeHelper.GetChild(this, 0) as Image).Source = CheckImage;
            }
        }
        #endregion

        private unsafe static BitmapSource ToGrayScale(BitmapSource source)
        {
            const int PIXEL_SIZE = 4;
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            var bitmap = new WriteableBitmap(source);

            bitmap.Lock();
            var backBuffer = (byte*)bitmap.BackBuffer.ToPointer();
            for (int y = 0; y < height; y++)
            {
                var row = backBuffer + (y * bitmap.BackBufferStride);
                for (int x = 0; x < width; x++)
                {
                    var grayScale = (byte)(((row[x * PIXEL_SIZE + 1]) + (row[x * PIXEL_SIZE + 2]) + (row[x * PIXEL_SIZE + 3])) / 3);
                    for (int i = 0; i < PIXEL_SIZE; i++)
                        row[x * PIXEL_SIZE + i] = grayScale;
                }
            }

            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            return bitmap;
        }
    }
}
