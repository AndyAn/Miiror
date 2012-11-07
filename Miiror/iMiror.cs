using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using Miiror.Utils;
using System.Runtime.Serialization;

namespace Miiror
{
    public delegate void CallBackEventHandler(MiirorItem mi, bool isTimerOn, bool isWatcherOn); 

    [DataContract]
    internal class iMiror : IDisposable
    {
        const int WATCH_SPAN = 500;
        const int RECONNECT_SPAN = 5000;
        const int MAX_RETRY_COUNT = 5;

        FileSystemWatcher fsw = new FileSystemWatcher();
        FileScanManager fsm = null;
        Timer timer = new Timer();
        List<FileItem> fileStack = new List<FileItem>();
        List<string> FilterList = new List<string>();
        List<string> ReserveList = new List<string>();
        private CallBackEventHandler OnCallBackEvent;  

        [DataMember]
        public MiirorItem MirorItem { get; private set; }

        public iMiror(MiirorItem mi)
        {
            MirorItem = mi;

            if (0 < mi.Filtered.Length)
            {
                if (mi.Filtered.StartsWith("~"))
                {
                    FilterList.AddRange(mi.Filtered.Substring(1).Split('|'));
                }
                else
                {
                    ReserveList.AddRange(mi.Filtered.Split('|'));
                }
            }

            try
            {
                fsm = FileScanManager.GetInstance(mi);
                fsm.FilterList = FilterList;
                fsm.ReserveList = ReserveList;
                if (mi.IsWorking)
                {
                    DoLooseScan();
                }

                SystemWatch(true);
            }
            catch (IOException IOex)
            {
                Log.GetInstance().WriteLog(IOex.Message);
                Stop();
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
            finally
            {
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Interval = WATCH_SPAN;
                if (mi.IsWorking)
                {
                    timer.Enabled = true;
                    timer.Start();
                }
            }
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
                            if (!FSOpt.SynchFileEntry(item, MirorItem))
                            {
                                filtered.Add(item);
                            }
                        }
                        fileStack.Clear();
                        fileStack.AddRange(filtered);
                    }
                }
            }
            catch (IOException IOex)
            {
                Log.GetInstance().WriteLog(IOex.Message);
                Stop();
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
        }

        void fsw_Error(object sender, ErrorEventArgs e)
        {
            string monitorPath = MirorItem.IsFolder ? MirorItem.Source : Path.GetDirectoryName(MirorItem.Source);

            try
            {
                Exception watchException = e.GetException();
                Log.GetInstance().WriteLog(watchException.Message);

                if (FSOpt.CheckMonitorPath(monitorPath, MAX_RETRY_COUNT, RECONNECT_SPAN))
                {
                    int retryCounter = 0;
                    // We need to create new version of the object because the
                    // old one is now corrupted
                    fsw = new FileSystemWatcher();
                    while (!fsw.EnableRaisingEvents)
                    {
                        try
                        {
                            // This will throw an error at the
                            // watcher.NotifyFilter line if it can't get the path.
                            SystemWatch(true);
                            System.Threading.Thread.Sleep(RECONNECT_SPAN);
                            retryCounter++;

                            if (MAX_RETRY_COUNT == retryCounter)
                            {
                                break;
                            }
                        }
                        catch
                        {
                            // Sleep for a while; otherwise, it takes a bit of
                            // processor time
                            System.Threading.Thread.Sleep(RECONNECT_SPAN);
                        }
                    }

                    Stop();
                }
                else
                {
                    Stop();
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
                Stop();
            }
        }

        void fsw_Renamed(object sender, RenamedEventArgs e)
        {
            lock (fileStack)
            {
                if (FSOpt.FilterFile(e.FullPath, FilterList, ReserveList))
                {
                    fileStack.Add(new FileItem() { ChangeType = e.ChangeType, NewFile = e.FullPath, OldFile = e.OldFullPath });
                }
            }

            timer.Enabled = true;
            timer.Start();
        }

        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            lock (fileStack)
            {
                if (FSOpt.FilterFile(e.FullPath, FilterList, ReserveList))
                {
                    fileStack.Add(new FileItem() { ChangeType = e.ChangeType == WatcherChangeTypes.Deleted ? e.ChangeType : WatcherChangeTypes.Changed, NewFile = e.FullPath });
                }
            }

            timer.Enabled = true;
            timer.Start();
        }

        void SystemWatch(bool isStart)
        {
            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.Created += new FileSystemEventHandler(fsw_Changed);
            fsw.Deleted += new FileSystemEventHandler(fsw_Changed);
            fsw.Renamed += new RenamedEventHandler(fsw_Renamed);
            fsw.Error += new ErrorEventHandler(fsw_Error);
            fsw.Path = MirorItem.IsFolder ? MirorItem.Source : Path.GetDirectoryName(MirorItem.Source);
            fsw.Filter = MirorItem.IsFolder ? "*.*" : MirorItem.Source;
            fsw.IncludeSubdirectories = MirorItem.IsRecursive;
            fsw.NotifyFilter = NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size;
            fsw.EnableRaisingEvents = isStart;
        }

        public event CallBackEventHandler CallBack
        {
            add { OnCallBackEvent += new CallBackEventHandler(value); }
            remove { OnCallBackEvent -= new CallBackEventHandler(value); }
        }  

        public void Start()
        {
            if (!fsw.EnableRaisingEvents)
            {
                try
                {
                    fsw = new FileSystemWatcher();
                    SystemWatch(true);
                }
                catch (IOException IOex)
                {
                    Log.GetInstance().WriteLog(IOex.Message);
                    Stop();
                    return;
                }
                catch (Exception ex)
                {
                    Log.GetInstance().WriteLog(ex.Message);
                    return;
                }
            }

            timer.Enabled = true;
            timer.Start();
            MirorItem = new MiirorItem()
            {
                Filtered = MirorItem.Filtered,
                Identity = MirorItem.Identity,
                IsFolder = MirorItem.IsFolder,
                IsWorking = true,
                IsRecursive = MirorItem.IsRecursive,
                Source = MirorItem.Source,
                Target = MirorItem.Target
            };
        }

        public void Stop()
        {
            timer.Stop();
            timer.Enabled = false;
            MirorItem = new MiirorItem()
            {
                Filtered = MirorItem.Filtered,
                Identity = MirorItem.Identity,
                IsFolder = MirorItem.IsFolder,
                IsWorking = false,
                IsRecursive = MirorItem.IsRecursive,
                Source = MirorItem.Source,
                Target = MirorItem.Target
            };

            OnCallBackEvent(MirorItem, timer.Enabled, fsw.EnableRaisingEvents);
        }

        public void Reset()
        {
            fsw = new FileSystemWatcher();
            SystemWatch(false);
            Start();
        }

        public void DoLooseScan()
        {
            try
            {
                lock (fileStack)
                {
                    fileStack.AddRange(fsm.LooseScan());
                }

                timer.Enabled = true;
                timer.Start();
            }
            catch (IOException IOex)
            {
                Log.GetInstance().WriteLog(IOex.Message);
                Stop();
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
        }

        public void DoRestrictScan()
        {
            try
            {
                lock (fileStack)
                {
                    fileStack.AddRange(fsm.RestrictScan());
                }

                timer.Enabled = true;
                timer.Start();
            }
            catch (IOException IOex)
            {
                Log.GetInstance().WriteLog(IOex.Message);
                Stop();
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
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
