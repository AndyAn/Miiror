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
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using IO = System.IO;
using Miiror.Utils;
using Miiror.Controls;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Timers;
using System.ComponentModel;

namespace Miiror
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal MiirorSettings MiSettings;
        private MirorGroup mirorGroup;
        private string STARTUP_TOOLTIP = "Run with Windows Startup: {0}";

        public MainWindow()
        {
            InitializeComponent();
            Icon = IconManager.GetIcon("systray");
            Margin = new Thickness(10);
        }

        #region Events

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Max.Content = "1";
                Max.ToolTip = "Maximize Window";
            }
            else
            {
                Max.Content = "2";
                Max.ToolTip = "Restore Down";
            }

            WindowStyleChange(WindowState);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotificationAreaIcon.Icon = IconManager.GetIcon("systray");
            //Hide();

            MiSettings = LoadConfig();
            InitializeMonitorList();
            BindListBox();

            Startup.IsChecked = MiSettings.StartupWithWindows;
            Startup.ToolTip = string.Format(STARTUP_TOOLTIP, Startup.IsChecked.Value);
            SetWindowsStartup(Startup.IsChecked.Value);

            MiList.PreviewMouseDoubleClick += new MouseButtonEventHandler(MiList_MouseDoubleClick);
            //AddMonitoring();

            BalloonBox bb = new BalloonBox(this, "Miiror is started...");
            bb.Show();
        }

        private void MiList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point mPoint = e.GetPosition(MiList);

            if (mPoint.Y <= 48 * MiList.Items.Count)
            {
                MiirorItemDO item = MiList.SelectedItem as MiirorItemDO;
                EditMonitoring(item);
            }
            else
            {
                AddMonitoring();
            }
        }

        private void Startup_Click(object sender, RoutedEventArgs e)
        {
            Startup.ToolTip = string.Format(STARTUP_TOOLTIP, Startup.IsChecked.Value);
            MiSettings.StartupWithWindows = Startup.IsChecked.Value;
            SaveConfig();

            SetWindowsStartup(Startup.IsChecked.Value);
        }

        void mirorGroup_CallBack(MiirorItem mi, bool isTimerOn, bool isWatcherOn)
        {
            BindListBox();
        }

        #endregion

        #region Provate Methods

        private void SetWindowsStartup(bool isChecked)
        {
            Microsoft.Win32.RegistryKey hkml = Microsoft.Win32.Registry.LocalMachine;

            try
            {
                Microsoft.Win32.RegistryKey runs = hkml.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                string[] valueNames = runs.GetValueNames();

                if (isChecked)
                {
                    runs.SetValue("MiirorStartup", Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    foreach (string keyName in valueNames)
                    {
                        if ("MIIRORSTARTUP" == keyName.ToUpper())
                        {
                            runs.DeleteValue("MiirorStartup", false);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //return 0;
            }
            finally
            {
                hkml.Close();
            }
            //return 1;
        }

        private void InitializeMonitorList()
        {
            mirorGroup = new MirorGroup();
            mirorGroup.CallBack += new CallBackEventHandler(mirorGroup_CallBack);

            for (int i = 0; i < MiSettings.MonitorList.Count; i++)
            {
                MiirorItem mi = MiSettings.MonitorList[i].Clone();
                mi.Identity = "";
                MiSettings.MonitorList[i] = mi;
            }

            mirorGroup.AddMonitors(MiSettings.MonitorList);
        }

        private void UpdateMonitorList(MiirorItem mi)
        {
            mirorGroup.AddMonitor(mi);

            mi.Identity = mi.GetMD5Hash();
            int index = MiSettings.MonitorList.FindIndex(m => m.Identity == mi.Identity);

            if (index > -1)
            {
                MiSettings.MonitorList[index] = mi;
            }
            else
            {
                MiSettings.MonitorList.Add(mi);
            }
        }

        private MiirorSettings LoadConfig()
        {
            string cfgFile = IO.Path.Combine(Environment.CurrentDirectory, "Miiror.cfg");

            if (!IO.File.Exists(cfgFile))
            {
                ObjectSerializer<MiirorSettings>.Save(new MiirorSettings(), cfgFile);
            }

            return ObjectSerializer<MiirorSettings>.Load(cfgFile);
        }

        private bool SaveConfig()
        {
            bool isSaved = true;

            try
            {
                string cfgFile = IO.Path.Combine(Environment.CurrentDirectory, "Miiror.cfg");

                ObjectSerializer<MiirorSettings>.Save(MiSettings, cfgFile);
            }
            catch
            {
                isSaved = false;
            }

            return isSaved;
        }

        private bool SaveConfig(bool isRefresh)
        {
            if (isRefresh)
            {
                MiSettings.MonitorList.Clear();
                foreach (MiirorItemDO item in mirorGroup.GetSource())
                {
                    MiSettings.MonitorList.Add(new MiirorItem()
                    {
                        Filtered = item.Filtered,
                        Identity = item.Identity,
                        IsFolder = item.IsFolder,
                        IsRecursive = item.IsRecursive,
                        IsWorking = item.IsWorking,
                        Source = item.Source,
                        Target = item.Target
                    });
                }
            }

            return SaveConfig();
        }

        private void AddMonitoring()
        {
            EditMonitoring(null);
        }

        private void EditMonitoring(MiirorItemDO item)
        {
            EditPanel ep = new EditPanel(item);

            ep.Owner = this;
            MiirorItem mi = ep.ShowModel();

            if (string.IsNullOrEmpty(mi.Source) && string.IsNullOrEmpty(mi.Target))
            {
                return;
            }

            UpdateMonitorList(mi);
            BindListBox();
            SaveConfig();
        }

        private void BindListBox()
        {
            this.Dispatcher.BeginInvoke(new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    ObservableCollection<MiirorItemDO> data = mirorGroup.GetSource();

                    if (data.Count > 0)
                    {
                        MiList.ItemsSource = data;
                    }
                    else
                    {
                        MiList.ItemsSource = null;
                    }
                }), new object[] {this, System.EventArgs.Empty});
        }

        private void WindowStyleChange(WindowState windowState)
        {
            if (windowState == WindowState.Maximized)
            {
                Margin = new Thickness(0);
            }
            else
            {
                Margin = new Thickness(10);
            }
        }

        #endregion

        #region Windows API

        private readonly int agWidth = 4; //12;
        private readonly int borderThickness = 4;
        private Point mousePoint = new Point();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }
        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32API.WM_NCHITTEST:
                    //if (WindowState != WindowState.Normal)
                    //{
                    //    break;
                    //}

                    mousePoint.X = Forms.Control.MousePosition.X;
                    mousePoint.Y = Forms.Control.MousePosition.Y;

                    #region test mouse location

                    // left top corner
                    if (mousePoint.Y - Location.Y - Margin.Top <= agWidth
                       && mousePoint.X - Location.X - Margin.Left <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTTOPLEFT);
                    }
                    // left bottom corner    
                    else if (ActualHeight + Location.Y - mousePoint.Y - Margin.Bottom <= agWidth
                       && mousePoint.X - Location.X - Margin.Left <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTBOTTOMLEFT);
                    }
                    // right top corner
                    else if (mousePoint.Y - Location.Y - Margin.Top <= agWidth
                       && ActualWidth + Location.X - mousePoint.X - Margin.Right <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTTOPRIGHT);
                    }
                    // right bottom corner
                    else if (ActualWidth + Location.X - mousePoint.X - Margin.Right <= agWidth
                       && ActualHeight + Location.Y - mousePoint.Y - Margin.Bottom <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTBOTTOMRIGHT);
                    }
                    // left side of window
                    else if (mousePoint.X - Location.X - Margin.Left <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTLEFT);
                    }
                    // right side of window
                    else if (ActualWidth + Location.X - mousePoint.X - Margin.Right <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTRIGHT);
                    }
                    // top of window
                    else if (mousePoint.Y - Location.Y - Margin.Top <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTTOP);
                    }
                    // bottom of window
                    else if (ActualHeight + Location.Y - mousePoint.Y - Margin.Bottom <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTBOTTOM);
                    }
                    // moving window
                    else if ((mousePoint.Y - Location.Y - Margin.Top <= 32 && mousePoint.X - Location.X - Margin.Left - Margin.Right <= ActualWidth - 160) ||
                        (mousePoint.Y - Location.Y - Margin.Top > 32 && mousePoint.Y - Location.Y - Margin.Top <= 74))
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTCAPTION);
                    }

                    break;

                    #endregion

                case Win32API.WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        internal Point Location
        {
            get
            {
                return new Point(WindowState == WindowState.Maximized ? 0 : Left, WindowState == WindowState.Maximized ? 0 : Top);
            }
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            Win32API.MINMAXINFO mmi = (Win32API.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Win32API.MINMAXINFO));

            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = Win32API.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                Win32API.MONITORINFO monitorInfo = new Win32API.MONITORINFO();
                Win32API.GetMonitorInfo(monitor, monitorInfo);
                Win32API.RECT rcWorkArea = monitorInfo.rcWork;
                Win32API.RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);// -3;
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);// -3;
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);// +6;
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);// +6;
                mmi.ptMinTrackSize.x = (int)this.MinWidth;
                mmi.ptMinTrackSize.y = (int)this.MinHeight;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #endregion

        #region Control Panel

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (MiSettings.ShowMinimizingBox)
            {
                MinimizingBox mb = new MinimizingBox(this);

                mb.Owner = this;
                bool? result = mb.ShowDialog();

                if (!result.Value) return;

                BindListBox();
                SaveConfig();
            }

            if (MiSettings.MinimizedClose)
            {
                Hide();
            }
            else
            {
                Close();
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            Hide();
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                //Max.Content = "2";
                //Max.ToolTip = "Restore Down";
            }
            else
            {
                WindowState = WindowState.Normal;
                //Max.Content = "1";
                //Max.ToolTip = "Maximize Window";
            }
            //WindowStyleChange(WindowState);
        }

        #endregion

        #region System Tray Events

        private void Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void StopAll_Click(object sender, EventArgs e)
        {
            foreach (iMiror item in mirorGroup.GetMirors())
            {
                item.Stop();
            }

            BindListBox();
            SaveConfig(true);
        }

        private void StartAll_Click(object sender, EventArgs e)
        {
            foreach (iMiror item in mirorGroup.GetMirors())
            {
                item.Start();
            }

            BindListBox();
            SaveConfig(true);
        }

        private void Loose_Click(object sender, EventArgs e)
        {
            foreach (iMiror item in mirorGroup.GetMirors())
            {
                item.DoLooseScan();
            }
        }

        private void Restrict_Click(object sender, EventArgs e)
        {
            foreach (iMiror item in mirorGroup.GetMirors())
            {
                item.DoRestrictScan();
            }
        }

        private void NTFArea_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Show_Click(sender, null);
        }

        private void Show_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = System.Windows.WindowState.Normal;
            this.Focus();
        }

        #endregion

        #region Tools Panel

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddMonitoring();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            MiirorItemDO item = MiList.SelectedItem as MiirorItemDO;
            EditMonitoring(item);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (null != MiList.SelectedItem)
            {
                mirorGroup.RemoveMonitor((MiList.SelectedItem as MiirorItemDO).Identity);
                BindListBox();
                SaveConfig();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (null != MiList.SelectedItem)
            {
                string identity = (MiList.SelectedItem as MiirorItemDO).Identity;

                foreach (iMiror item in mirorGroup.GetMirors())
                {
                    if (item.MirorItem.Identity == identity)
                    {
                        item.Start();
                    }
                }
            }

            BindListBox();
            SaveConfig(true);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (null != MiList.SelectedItem)
            {
                string identity = (MiList.SelectedItem as MiirorItemDO).Identity;

                foreach (iMiror item in mirorGroup.GetMirors())
                {
                    if (item.MirorItem.Identity == identity)
                    {
                        item.Stop();
                    }
                }
            }

            BindListBox();
            SaveConfig(true);
        }

        private void LooseScan_Click(object sender, RoutedEventArgs e)
        {
            if (null != MiList.SelectedItem)
            {
                string identity = (MiList.SelectedItem as MiirorItemDO).Identity;

                foreach (iMiror item in mirorGroup.GetMirors())
                {
                    if (item.MirorItem.Identity == identity)
                    {
                        item.DoLooseScan();
                    }
                }
            }
        }

        private void RestrictScan_Click(object sender, RoutedEventArgs e)
        {
            if (null != MiList.SelectedItem)
            {
                string identity = (MiList.SelectedItem as MiirorItemDO).Identity;

                foreach (iMiror item in mirorGroup.GetMirors())
                {
                    if (item.MirorItem.Identity == identity)
                    {
                        item.DoRestrictScan();
                    }
                }
            }
        }

        private void TExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
