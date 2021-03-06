﻿using System;
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
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        #region Constructors

        public MessageBox(MainWindow owner, string message, string title, MessageBoxButton button)
        {
            InitializeComponent();
            Margin = (owner.WindowState == WindowState.Maximized) ? new Thickness(0, 10, 0, 10) : new Thickness(10);

            this.Width = owner.ActualWidth;
            this.Height = 196;
            this.Left = owner.Location.X;
            this.Top = owner.Location.Y + (owner.ActualHeight - Height) / 2;

            Title.Text = title;
            Message.Text = message;
            Result = MessageBoxResult.None;

            switch (button)
            {
                case MessageBoxButton.OK:
                    ButtonGroup.Children.Remove(Yes);
                    ButtonGroup.Children.Remove(No);
                    ButtonGroup.Children.Remove(Cancel);
                    break;
                case MessageBoxButton.OKCancel:
                    ButtonGroup.Children.Remove(Yes);
                    ButtonGroup.Children.Remove(No);
                    break;
                case MessageBoxButton.YesNo:
                    ButtonGroup.Children.Remove(Okay);
                    ButtonGroup.Children.Remove(Cancel);
                    break;
                case MessageBoxButton.YesNoCancel:
                    ButtonGroup.Children.Remove(Okay);
                    break;
            }
        }

        public MessageBox(MainWindow owner, string message, MessageBoxButton button) : this (owner, message, "Information", button)
        {
        }

        public MessageBox(MainWindow owner, string message, string title) : this (owner, message, title, MessageBoxButton.OK)
        {
        }

        public MessageBox(MainWindow owner, string message) : this (owner, message, "Information", MessageBoxButton.OK)
        {
        }

        #endregion

        public MessageBoxResult Result { get; private set; }

        private void OKay_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            this.Close();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
