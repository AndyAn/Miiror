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
using System.Windows.Shapes;
using System.Globalization;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using System.Runtime.InteropServices;
using Miiror.Utils;
using System.Windows.Interop;
using System.Timers;
using System.Windows.Media.Animation;

namespace Miiror
{
    /// <summary>
    /// Interaction logic for BalloonBox.xaml
    /// </summary>
    public partial class BalloonBox : Window
    {
        int MAX_WIDTH = 360;
        int DEFAULT_WIDTH = 274;
        int DEFAULT_HEIGHT = 80;
        Timer timer;

        Storyboard BoxStart;
        Storyboard BoxEnd;

        public BalloonBox(MainWindow owner, string message)
        {
            InitializeComponent();

            //Margin = new Thickness(10);
            double msgLength = MeasureString(message);

            Message.Text = message;
            Message.TextWrapping = TextWrapping.Wrap;

            Width = GetWidth(msgLength);
            Height = GetHeight(msgLength);
            Point p = GetPosition();
            Left = p.X;//Forms.Screen.PrimaryScreen.WorkingArea.Width - Width;
            Top = p.Y;

            timer = new Timer(10 * 1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            timer.Start();

            BoxStart = this.Resources["PopupShow"] as Storyboard;
            BoxEnd = this.Resources["PopupHide"] as Storyboard;
            BoxEnd.Completed += new EventHandler(BoxEnd_Completed);
        }

        void BoxEnd_Completed(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Close();
            }), null);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            timer.Dispose();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                BoxEnd.Begin();
            }), null);
        }

        private Point GetPosition()
        {
            Point pos = new Point();

            Drawing.Rectangle workingArea = Forms.Screen.PrimaryScreen.WorkingArea;
            Drawing.Rectangle monitorArea = Forms.Screen.PrimaryScreen.Bounds;

            if (workingArea.Top == 0 && workingArea.Left == 0 && workingArea.Height < monitorArea.Height)
            {   // Taskbar at the bottom
                pos.X = workingArea.Width - Width;
                pos.Y = workingArea.Height - Height;
                Margin = new Thickness(10, 10, 0, 0);
            }
            else if (workingArea.Top == 0 && workingArea.Left == 0 && workingArea.Width < monitorArea.Width)
            {   // Taskbar on the right
                pos.X = workingArea.Width - Width;
                pos.Y = workingArea.Height - Height;
                Margin = new Thickness(10, 10, 0, 0);
            }
            else if (workingArea.Left == monitorArea.Left && workingArea.Bottom == monitorArea.Bottom && workingArea.Top > monitorArea.Top)
            {   // Taskbar on the top
                pos.X = workingArea.Width - Width;
                pos.Y = workingArea.Top - monitorArea.Top;
                Margin = new Thickness(10, 0, 0, 10);
            }
            else if (workingArea.Top == monitorArea.Top && workingArea.Bottom == monitorArea.Bottom && workingArea.Left > monitorArea.Left)
            {   // Taskbar on the left
                pos.X = workingArea.Left - monitorArea.Left;
                pos.Y = workingArea.Height - Height;
                Margin = new Thickness(0, 10, 10, 0);
            }

            return pos;
        }

        private double GetWidth(double length)
        {
            double width = Math.Max(DEFAULT_WIDTH, Math.Min(length, MAX_WIDTH));

            Message.Width += (width - DEFAULT_WIDTH);

            return width;
        }

        private double GetHeight(double length)
        {
            double height = DEFAULT_HEIGHT;

            if (length > Message.Width)
            {
                height += ((length / MAX_WIDTH) - 1 + (length % MAX_WIDTH == 0 ? 0 : 1)) * Message.FontSize;

                
            }

            return height;
        }

        private double MeasureString(string text)
        {
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(Message.FontFamily, Message.FontStyle, Message.FontWeight, Message.FontStretch),
                Message.FontSize,
                new SolidColorBrush(Colors.Black));

            if (text == "")
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            else if (text.Substring(0, 1) == "\t")
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            else
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BoxEnd.Begin();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BoxStart.Begin();
        }
    }
}
