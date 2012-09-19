using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using Miiror.Utils;

namespace Miiror
{
    internal class iMiror : IDisposable
    {
        const int WATCH_SPAN = 500;

        FileSystemWatcher fsw = new FileSystemWatcher();
        FileScanManager fsm = null;
        Timer timer = new Timer();
        MiirorItem miirorItem;
        List<FileItem> fileStack = new List<FileItem>();

        public string Identity { get { return miirorItem.Identity; } }

        public iMiror(MiirorItem mi)
        {
            miirorItem = mi;
            fsm = FileScanManager.GetInstance(mi);
            DoLooseScan();

            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.Renamed += new RenamedEventHandler(fsw_Renamed);
            fsw.Path = mi.IsFolder ? mi.Source : Path.GetDirectoryName(mi.Source);
            fsw.Filter = mi.IsFolder ? "*.*" : mi.Source;
            fsw.IncludeSubdirectories = mi.IsRecursive;
            fsw.NotifyFilter = NotifyFilters.DirectoryName 
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite 
                             | NotifyFilters.Size;
            fsw.EnableRaisingEvents = true;

            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Interval = WATCH_SPAN;
            timer.Enabled = true;
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (fileStack)
                {
                    if (fileStack.Count == 0)
                    {
                        timer.Stop();
                        timer.Enabled = false;
                    }
                    else
                    {
                        List<FileItem> filtered = new List<FileItem>();
                        foreach (FileItem item in fileStack)
                        {
                            if (!FSOpt.SynchFileEntry(item, miirorItem))
                            {
                                filtered.Add(item);
                            }
                        }
                        fileStack.Clear();
                        fileStack.AddRange(filtered);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
        }

        void fsw_Renamed(object sender, RenamedEventArgs e)
        {
            lock (fileStack)
            {
                fileStack.Add(new FileItem() { ChangeType = e.ChangeType, NewFile = e.FullPath, OldFile = e.OldFullPath });
            }

            timer.Enabled = true;
            timer.Start();
        }

        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            lock (fileStack)
            {
                fileStack.Add(new FileItem() { ChangeType = e.ChangeType == WatcherChangeTypes.Deleted ? e.ChangeType : WatcherChangeTypes.Changed, NewFile = e.FullPath });
            }

            timer.Enabled = true;
            timer.Start();
        }

        public void DoLooseScan()
        {
            lock (fileStack)
            {
                fileStack.AddRange(fsm.LooseScan());
            }

            timer.Enabled = true;
            timer.Start();
        }

        public void DoRestrictScan()
        {
            lock (fileStack)
            {
                fileStack.AddRange(fsm.RestrictScan());
            }

            timer.Enabled = true;
            timer.Start();
        }

        public void Dispose()
        {
            fsw.Dispose();
            fsw = null;

            fsm = null;

            timer.Stop();
            timer.Enabled = false;
            timer.Dispose();
            timer = null;

            fileStack = null;
        }
    }
}
