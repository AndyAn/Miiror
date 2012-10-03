using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Miiror.Utils
{
    internal class FileScanManager
    {
        private MiirorItem miirorItem;

        private FileScanManager(MiirorItem mi)
        {
            miirorItem = mi;
        }

        public static FileScanManager GetInstance(MiirorItem mi)
        {
            return new FileScanManager(mi);
        }

        public List<FileItem> LooseScan()
        {
            List<string> source = new List<string>();
            List<string> target = new List<string>();

            if (miirorItem.IsFolder)
            {
                //source.AddRange(Directory.GetFileSystemEntries(miirorItem.Source, "*.*").ToList());
                //target.AddRange(Directory.GetFileSystemEntries(miirorItem.Target, "*.*").ToList());
                source.AddRange(Directory.GetDirectories(miirorItem.Source, "*.*", SearchOption.AllDirectories));
                source.AddRange(Directory.GetFiles(miirorItem.Source, "*.*", SearchOption.AllDirectories));
                target.AddRange(Directory.GetDirectories(miirorItem.Target, "*.*", SearchOption.AllDirectories));
                target.AddRange(Directory.GetFiles(miirorItem.Target, "*.*", SearchOption.AllDirectories));
            }
            else
            {
                source.Add(miirorItem.Source);
                target.Add(miirorItem.Target);
            }

            return CompareFSO(source, target, WatcherChangeTypes.Changed);
        }

        public List<FileItem> RestrictScan()
        {
            List<string> source = new List<string>();
            List<string> target = new List<string>();

            if (miirorItem.IsFolder)
            {
                //source.AddRange(Directory.GetFileSystemEntries(miirorItem.Source, "*.*").ToList());
                //target.AddRange(Directory.GetFileSystemEntries(miirorItem.Target, "*.*").ToList());
                source.AddRange(Directory.GetDirectories(miirorItem.Source, "*.*", SearchOption.AllDirectories));
                source.AddRange(Directory.GetFiles(miirorItem.Source, "*.*", SearchOption.AllDirectories));
                target.AddRange(Directory.GetDirectories(miirorItem.Target, "*.*", SearchOption.AllDirectories));
                target.AddRange(Directory.GetFiles(miirorItem.Target, "*.*", SearchOption.AllDirectories));
            }
            else
            {
                source.Add(miirorItem.Source);
                target.Add(miirorItem.Target);
            }

            List<FileItem> results = CompareFSO(source, target, WatcherChangeTypes.Changed);
            results.AddRange(CompareFSO(target, source, WatcherChangeTypes.Deleted));

            return results;
        }

        private List<FileItem> CompareFSO(List<string> source, List<string> target, WatcherChangeTypes changeType)
        {
            List<FileItem> results = new List<FileItem>();
            string targetFile = "";
            int index = 0;

            try
            {
                MiirorItem mi = new MiirorItem()
                {
                    Identity = miirorItem.Identity,
                    IsFolder = miirorItem.IsFolder,
                    Source = (changeType == WatcherChangeTypes.Deleted) ? miirorItem.Target : miirorItem.Source,
                    Target = (changeType == WatcherChangeTypes.Deleted) ? miirorItem.Source : miirorItem.Target
                };

                PathManager pm = PathManager.GetInstance(mi);
                foreach (string entry in source)
                {
                    targetFile = pm.ConvertPath(entry);
                    index = target.FindIndex(e => e.ToLower() == targetFile.ToLower());

                    if (index == -1)
                    {
                        results.Add(new FileItem() { ChangeType = changeType, NewFile = (changeType == WatcherChangeTypes.Deleted) ? targetFile : entry, OldFile = "" });
                    }
                    else
                    {
                        if (changeType == WatcherChangeTypes.Changed)
                        {
                            if (new FileInfo(entry).LastWriteTime > new FileInfo(targetFile).LastWriteTime)
                            {
                                results.Add(new FileItem() { ChangeType = WatcherChangeTypes.Changed, NewFile = entry, OldFile = "" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }

            return results;
        }
    }
}
