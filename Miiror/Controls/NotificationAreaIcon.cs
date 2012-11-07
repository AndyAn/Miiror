using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Drawing;

namespace Miiror.Controls
{
    [ContentProperty("Text")]
    [DefaultEvent("MouseDoubleClick")]
    public class NotificationAreaIcon : FrameworkElement
    {
        Forms.NotifyIcon notifyIcon;

        public static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent(
            "MouseClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotificationAreaIcon));

        public static readonly RoutedEvent MouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
            "MouseDoubleClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotificationAreaIcon));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(NotificationAreaIcon));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NotificationAreaIcon));

        public static readonly DependencyProperty FormsContextMenuProperty =
            DependencyProperty.Register("MenuItems", typeof(List<Forms.ToolStripItem>), typeof(NotificationAreaIcon), new PropertyMetadata(new List<Forms.ToolStripItem>()));

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Create and initialize the window forms notify icon based
            notifyIcon = new Forms.NotifyIcon();
            notifyIcon.Text = Text;
            notifyIcon.BalloonTipText = Text;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                notifyIcon.Icon = FromImageSource(Icon);
            }
            notifyIcon.Visible = FromVisibility(Visibility);

            if (this.MenuItems != null && this.MenuItems.Count > 0)
            {
                notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
                foreach (Forms.ToolStripItem item in this.MenuItems)
                {
                    notifyIcon.ContextMenuStrip.Items.Add(item);
                }
                //notifyIcon.ContextMenu = new Forms.ContextMenu(this.MenuItems.ToArray());
            }


            notifyIcon.MouseDown += OnMouseDown;
            notifyIcon.MouseUp += OnMouseUp;
            notifyIcon.MouseClick += OnMouseClick;
            notifyIcon.MouseDoubleClick += OnMouseDoubleClick;

            Dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;

            DependencyPropertyDescriptor dpdIcon = DependencyPropertyDescriptor.FromProperty(IconProperty, typeof(NotificationAreaIcon));
            if (dpdIcon != null)
            {
                dpdIcon.AddValueChanged(this, delegate
                {
                    notifyIcon.Icon = FromImageSource(Icon);
                });
            }
        }

        private void OnDispatcherShutdownStarted(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
        }

        private void OnMouseDown(object sender, Forms.MouseEventArgs e)
        {
            OnRaiseEvent(MouseDownEvent, new MouseButtonEventArgs(
                InputManager.Current.PrimaryMouseDevice, 0, ToMouseButton(e.Button)));
        }

        private void OnMouseUp(object sender, Forms.MouseEventArgs e)
        {
            OnRaiseEvent(MouseUpEvent, new MouseButtonEventArgs(
                InputManager.Current.PrimaryMouseDevice, 0, ToMouseButton(e.Button)));
        }

        private void OnMouseDoubleClick(object sender, Forms.MouseEventArgs e)
        {
            OnRaiseEvent(MouseDoubleClickEvent, new MouseButtonEventArgs(
                InputManager.Current.PrimaryMouseDevice, 0, ToMouseButton(e.Button)));
        }

        private void OnMouseClick(object sender, Forms.MouseEventArgs e)
        {
            OnRaiseEvent(MouseClickEvent, new MouseButtonEventArgs(
                InputManager.Current.PrimaryMouseDevice, 0, ToMouseButton(e.Button)));
        }

        private void OnRaiseEvent(RoutedEvent handler, MouseButtonEventArgs e)
        {
            e.RoutedEvent = handler;
            RaiseEvent(e);
        }

        public List<Forms.ToolStripItem> MenuItems
        {
            get { return (List<Forms.ToolStripItem>)GetValue(FormsContextMenuProperty); }
            set { SetValue(FormsContextMenuProperty, value); }
        }

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public event MouseButtonEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        public event MouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }

        #region Conversion members

        private static Drawing.Icon FromImageSource(ImageSource icon)
        {
            if (icon == null)
            {
                return null;
            }

            Drawing.Bitmap bitmap;

            using (MemoryStream stream = new MemoryStream())
            {
                BmpBitmapEncoder bbEncoder = new BmpBitmapEncoder();
                bbEncoder.Frames.Add(BitmapFrame.Create(icon as BitmapSource));
                bbEncoder.Save(stream);

                bitmap = new Drawing.Bitmap(stream);
            }

            return Drawing.Icon.FromHandle(bitmap.GetHicon());
        }

        private static bool FromVisibility(Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }

        private MouseButton ToMouseButton(Forms.MouseButtons button)
        {
            switch (button)
            {
                case Forms.MouseButtons.Left:
                    return MouseButton.Left;
                case Forms.MouseButtons.Right:
                    return MouseButton.Right;
                case Forms.MouseButtons.Middle:
                    return MouseButton.Middle;
                case Forms.MouseButtons.XButton1:
                    return MouseButton.XButton1;
                case Forms.MouseButtons.XButton2:
                    return MouseButton.XButton2;
            }
            throw new InvalidOperationException();
        }

        #endregion Conversion members
    }
}
