using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ZetaLongPaths;

namespace Miiror.Utils
{
    internal class FileScanManager
    {
        private MiirorItem miirorItem;

        public List<string> FilterList { get; set; }
        public List<string> ReserveList { get; set; }

        private FileScanManager(MiirorItem mi)
        {
            miirorItem = mi;

            FilterList = new List<string>();
            ReserveList = new List<string>();
        }

        public static FileScanManager GetInstance(MiirorItem mi)
        {
            return new FileScanManager(mi);
        }

        public List<FileItem> LooseScan()
        {
            return RestrictScan(false);
        }

        public List<FileItem> RestrictScan(bool isRestrict = true)
        {
            List<string> source = new List<string>();
            List<string> target = new List<string>();
            List<FileItem> results;

            try
            {
                if (miirorItem.IsFolder)
                {
                    //source.AddRange(Directory.GetFileSystemEntries(miirorItem.Source, "*.*").ToList());
                    //target.AddRange(Directory.GetFileSystemEntries(miirorItem.Target, "*.*").ToList());
                    source.AddRange(Directory.GetDirectories(miirorItem.Source, "*.*", SearchOption.AllDirectories));
                    source.AddRange(Directory.GetFiles(miirorItem.Source, "*.*", SearchOption.AllDirectories));
                    target.AddRange(Directory.GetDirectories(miirorItem.Target, "*.*", SearchOption.AllDirectories));
                    target.AddRange(Directory.GetFiles(miirorItem.Target, "*.*", SearchOption.AllDirectories));
                    //source.AddRange(ZlpIOHelper.GetDirectories(miirorItem.Source, "*.*").Select(d => d.FullName));
                    //source.AddRange(ZlpIOHelper.GetFiles(miirorItem.Source, "*.*", SearchOption.AllDirectories).Select(f => f.FullName));
                    //target.AddRange(ZlpIOHelper.GetDirectories(miirorItem.Target, "*.*").Select(d => d.FullName));
                    //target.AddRange(ZlpIOHelper.GetFiles(miirorItem.Target, "*.*", SearchOption.AllDirectories).Select(f => f.FullName));
                }
                else
                {
                    source.Add(miirorItem.Source);
                    target.Add(miirorItem.Target);
                }

                source = FilterFiles(source);
                target = FilterFiles(target);

                results = CompareFSO(source, target, WatcherChangeTypes.Changed);
                if (isRestrict)
                {
                    results.AddRange(CompareFSO(target, source, WatcherChangeTypes.Deleted));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return results;
        }

        private List<string> FilterFiles(List<string> list)
        {
            List<string> newList = new List<string>();

            try
            {
                foreach (string fileSysEntry in list)
                {
                    if (FSOpt.FilterFile(fileSysEntry, FilterList, ReserveList))
                    {
                        newList.Add(fileSysEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return newList;
        }

        private List<FileItem> CompareFSO(List<string> source, List<string> target, WatcherChangeTypes changeType)
        {
            List<FileItem> results = new List<FileItem>();
            string targetFile = "";
            int index = 0;
            string idxFile = "";

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
                    try
                    {
                        targetFile = pm.ConvertPath(entry);
                        //index = target.FindIndex(e => e.ToLower() == targetFile.ToLower());
                        idxFile = target.AsParallel().FirstOrDefault(e => e.ToLower() == targetFile.ToLower());
                        //if (index == -1)
                        if (idxFile.Length == 0)
                        {
                            results.Add(new FileItem() { ChangeType = changeType, NewFile = (changeType == WatcherChangeTypes.Deleted) ? targetFile : entry, OldFile = "" });
                        }
                        else
                        {
                            if (changeType == WatcherChangeTypes.Changed)
                            {
                                //if (new ZlpFileInfo(entry).LastWriteTime > new ZlpFileInfo(targetFile).LastWriteTime)
                                if (ZlpIOHelper.GetFileLastWriteTime(entry) > ZlpIOHelper.GetFileLastWriteTime(targetFile))
                                {
                                    results.Add(new FileItem() { ChangeType = WatcherChangeTypes.Changed, NewFile = entry, OldFile = "" });
                                }
                            }
                        }
                    }
                    catch (Exception xxx)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
                throw ex;
            }

            return results;
        }
    }
}
