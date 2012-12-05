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
using Miiror.Controls.Dialog;
using Miiror.Controls;
using Miiror.Utils;
using System.Windows.Interop;
using Miiror.Controls.Dialog.OS;
using Forms = System.Windows.Forms;

namespace Miiror
{
    /// <summary>
    /// Interaction logic for EditPanel.xaml
    /// </summary>
    public partial class EditPanel : Window
    {
        private string source = "Drag and Drop a file/folder here [Source]";
        private string target = "Drag and Drop a file/folder here [Target]";

        //internal MiirorItemDO MonitoringItem { get; set; }
        private bool isEdit = false;
        private MiirorItem mi = new MiirorItem();

        public EditPanel(MiirorItemDO miDO)
        {
            InitializeComponent();

            Source.Content = source;
            Target.Content = target;

            if (miDO != null)
            {
                isEdit = true;
                Source.Content = miDO.Source;
                Target.Content = miDO.Target;
                FileFilter.Text = miDO.Filtered;
                Recursive.IsChecked = miDO.IsRecursive;

                mi.Filtered = miDO.Filtered;
                mi.Identity = miDO.Identity;
                mi.IsFolder = miDO.IsFolder;
                mi.IsRecursive = miDO.IsRecursive;
                mi.IsWorking = miDO.IsWorking;
                mi.Source = miDO.Source;
                mi.Target = miDO.Target;
            }
        }

        public MiirorItem ShowModel()
        {
            bool? result = ShowDialog();

            if (result.HasValue && !result.Value)
            {
                mi = new MiirorItem();
            }

            return mi;
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            MainWindow owner = (this.Owner as MainWindow);

            Margin = (owner.WindowState == WindowState.Maximized) ? new Thickness(0, 10, 0, 10) : new Thickness(10);

            this.Left = owner.Location.X;
            this.Top = (owner.Location.Y) + owner.ActualHeight / 6;
            this.Width = owner.ActualWidth;
            this.Height = owner.ActualHeight / 3 * 2;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Source_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialogEx dialog = new OpenFileDialogEx())
            {
                dialog.StartLocation = AddonWindowLocation.Right;
                dialog.DefaultViewMode = FolderViewMode.Icon;
                dialog.OpenDialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                dialog.OpenDialog.AddExtension = true;
                dialog.OpenDialog.Multiselect = false;
                dialog.OpenDialog.ValidateNames = false;
                //dialog.OpenDialog.Filter = "Image Files(*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png";
                //dialog.OpenDialog.SupportMultiDottedExtensions = true;
                //dialog.OpenDialog.ShowReadOnly = true;
                dialog.OpenDialog.ShowDialog();
                Forms.DialogResult result = dialog.ShowDialog();

                //dialog.AcceptFiles = true;
                //dialog.Path = @"C:\";
                //dialog.ShowDialog();
                if (result == Forms.DialogResult.OK)
                {
                    Source.Content = dialog.OpenDialog.FileName;
                }

                dialog.Dispose();
            }
        }

        private void Target_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FSE_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FSE_Drop(object sender, DragEventArgs e)
        {
            ButtonExt btn = sender as ButtonExt;

            string[] fileData = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (fileData.Length > 0)
            {
                object temp = btn.Content;
                btn.Content = fileData[0];

                if (Source.Content.ToString() != source && Target.Content.ToString() != target)
                {
                    if (FSOpt.IsDirectory(Source.Content.ToString()) != FSOpt.IsDirectory(Target.Content.ToString()))
                    {
                        // warning
                        MessageBox msg = new MessageBox(this.Owner as MainWindow, "The type of the file entries are different!");
                        msg.Owner = this.Owner;
                        msg.ShowDialog();

                        btn.Content = temp;
                    }
                }
            }
        }

        private void Exchange_Click(object sender, RoutedEventArgs e)
        {
            if (Source.Content.ToString() != source && Target.Content.ToString() != target)
            {
                object swap = Source.Content;
                Source.Content = Target.Content;
                Target.Content = swap;
            }
            if (Source.Content.ToString() == source && Target.Content.ToString() != target)
            {
                Source.Content = Target.Content;
                Target.Content = target;
            }
            if (Target.Content.ToString() == target && Source.Content.ToString() != source)
            {
                Target.Content = Source.Content;
                Source.Content = source;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = (this.Owner as MainWindow);

            mi.Source = Source.Content.ToString();
            mi.Target = Target.Content.ToString();
            mi.IsRecursive = Recursive.IsChecked.Value;
            mi.IsFolder = FSOpt.IsDirectory(mi.Source);
            //mi.Identity = (!isEdit ? miDO.GetMD5Hash() : MonitoringItem.Identity);
            mi.Filtered = FileFilter.Text;
            mi.IsWorking = !isEdit;

            //if (MonitoringItem == null)
            //{
            //    mw.MiSettings.MonitorList.Add(m);
            //}
            //else
            //{
            //    int index = mw.MiSettings.MonitorList.FindIndex(mi => mi.Identity == m.Identity);
            //    if (index > -1)
            //    {
            //        m.Identity = m.GetMD5Hash();
            //        mw.MiSettings.MonitorList[index] = m;
            //    }
            //}

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
