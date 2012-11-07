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
using System.Windows.Interop;
using Miiror.Utils;
using Miiror.Controls;

namespace Miiror
{
    /// <summary>
    /// Interaction logic for MinimizingBox.xaml
    /// </summary>
    public partial class MinimizingBox : Window
    {
        #region Constructors

        public MinimizingBox(MainWindow owner)
        {
            InitializeComponent();
            Margin = (owner.WindowState == WindowState.Maximized) ? new Thickness(0, 10, 0, 10) : new Thickness(10);

            this.Width = owner.Width;
            this.Height = 274;
            this.Left = (owner.WindowState == WindowState.Maximized) ? 0 : owner.Left;
            this.Top = ((owner.WindowState == WindowState.Maximized) ? 0 : owner.Top) + (owner.Height - this.Height) / 2;

            if (owner.MiSettings.MinimizedClose)
            {
                MinToTray.IsChecked = true;
            }
            else
            {
                ExitApp.IsChecked = true;
            }
        }

        #endregion

        private void OKay_Click(object sender, RoutedEventArgs e)
        {
            if (MinToTray.IsChecked.Value)
            {
                (Owner as MainWindow).MiSettings.MinimizedClose = true;
            }
            else
            {
                (Owner as MainWindow).MiSettings.MinimizedClose = false;
            }

            (Owner as MainWindow).MiSettings.ShowMinimizingBox = !NoAsk.IsChecked.Value;

            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
