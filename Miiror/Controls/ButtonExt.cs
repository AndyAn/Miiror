using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Miiror.Controls
{
    public class ButtonExt : Button
    {
        static ButtonExt()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonExt), new FrameworkPropertyMetadata(typeof(ButtonExt)));
        }

        public string HighLightBackground
        {
            get { return (string)GetValue(HighLightBackgroundProperty); }
            set { SetValue(HighLightBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HighLightBackgroundProperty =
            DependencyProperty.Register("HighLightBackground", typeof(string), typeof(ButtonExt), new PropertyMetadata(null));

        //public static readonly DependencyProperty HighLightBackgroundProperty =
        //    DependencyProperty.Register("HighLightBackground", typeof(string), typeof(ButtonExt), new PropertyMetadata(string.Empty,
        //        (o, e) =>
        //        {
        //            try
        //            {
        //                ButtonExt button = o as ButtonExt;
        //                string color = e.NewValue.ToString().Trim('#');
        //                byte a, r, g, b;

        //                if (color.Length == 8)
        //                {
        //                    a = byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        //                    r = byte.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        //                    g = byte.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //                    b = byte.Parse(color.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        //                }
        //                else
        //                {
        //                    a = 255;
        //                    r = byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        //                    g = byte.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        //                    b = byte.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //                }

        //                button.SetValue(HoverBackgroundProperty, new SolidColorBrush(Color.FromArgb(a, r, g, b)) as Brush);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }));

        public Brush HoverBackground
        {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(ButtonExt), new UIPropertyMetadata(null));

        public string ImageUri
        {
            get { return (string)GetValue(ImageUriProperty); }
            set { SetValue(ImageUriProperty, value); }
        }

        public static readonly DependencyProperty ImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(string), typeof(ButtonExt), new UIPropertyMetadata(string.Empty,
              (o, e) =>
              {
                  try
                  {
                      Uri uriSource = new Uri((string)e.NewValue, UriKind.RelativeOrAbsolute);
                      if (uriSource != null)
                      {
                          ButtonExt button = o as ButtonExt;
                          BitmapImage img = new BitmapImage(uriSource);
                          button.SetValue(ImageSourceProperty, img);
                      }
                  }
                  catch (Exception ex)
                  {
                      throw ex;
                  }
              }));

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(ButtonExt), new UIPropertyMetadata(null));
    }
}