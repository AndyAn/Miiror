using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;

namespace Miiror.Utils
{
    public class MiirorItemDO : DependencyObject
    {
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(MiirorItemDO), null);

        public string Target
        {
            get { return (string)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(string), typeof(MiirorItemDO), null);


        public string Identity
        {
            get { return (string)GetValue(IdentityProperty); }
            set { SetValue(IdentityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Identity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdentityProperty =
            DependencyProperty.Register("Identity", typeof(string), typeof(MiirorItemDO), null);

        public string Filtered
        {
            get { return (string)GetValue(FilteredProperty); }
            set { SetValue(FilteredProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filtered.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilteredProperty =
            DependencyProperty.Register("Filtered", typeof(string), typeof(MiirorItemDO), null);

        public bool IsFolder
        {
            get { return (bool)GetValue(IsFolderProperty); }
            set { SetValue(IsFolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFolderProperty =
            DependencyProperty.Register("IsFolder", typeof(bool), typeof(MiirorItemDO), null);

        public bool IsRecursive
        {
            get { return (bool)GetValue(IsRecursiveProperty); }
            set { SetValue(IsRecursiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRecursive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRecursiveProperty =
            DependencyProperty.Register("IsRecursive", typeof(bool), typeof(MiirorItemDO), null);

        public bool IsWorking
        {
            get { return (bool)GetValue(IsWorkingProperty); }
            set { SetValue(IsWorkingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsWorking.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWorkingProperty =
            DependencyProperty.Register("IsWorking", typeof(bool), typeof(MiirorItemDO), null);

        public string WorkingDisplay
        {
            get { return (string)GetValue(WorkingDisplayProperty); }
            set { SetValue(WorkingDisplayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WorkingDisplay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WorkingDisplayProperty =
            DependencyProperty.Register("WorkingDisplay", typeof(string), typeof(MiirorItemDO), null);
    }

    [Serializable]
    public struct MiirorItem
    {
        public string Source;
        public string Target;
        public string Identity;
        public string Filtered;
        public bool IsFolder;
        public bool IsRecursive;
        public bool IsWorking;

        public string GetMD5Hash()
        {
            StringBuilder sb = new StringBuilder();
            string value = (string.IsNullOrEmpty(Source) ? "" : Source) + "|" + (string.IsNullOrEmpty(Target) ? "" : Target);
            if (value == "|")
            {
                value = this.GetHashCode().ToString() + "|" + this.GetType().GetHashCode().ToString();
            }
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public MiirorItem Clone()
        {
            return new MiirorItem()
            {
                Filtered = this.Filtered,
                Identity = this.Identity,
                IsFolder = this.IsFolder,
                IsRecursive = this.IsRecursive,
                IsWorking = this.IsWorking,
                Source = this.Source,
                Target = this.Target
            };
        }
   }

    internal struct FileItem
    {
        public string NewFile;
        public string OldFile;
        public WatcherChangeTypes ChangeType;
    }

    internal struct FSObject
    {
        public string FullPathName;
        public bool IsFile;
    }
}
