using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IO = System.IO;
using System.Diagnostics;
using System.Threading;
using ZetaLongPaths;
using ZetaLongPaths.Native;

namespace Miiror.Utils
{
    internal static class FSOpt
    {
        public static bool IsDirectory(string filePath)
        {
            bool result = false;

            try
            {
                if (ZlpIOHelper.FileExists(filePath) || ZlpIOHelper.DirectoryExists(filePath))
                {
                    result = ((ZlpIOHelper.GetFileAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory);
                }
                else if (FSOList.IsFolder(filePath))
                {
                    result = true;
                }
                else if (FSOList.IsFile(filePath))
                {
                    result = false;
                }
                else
                {
                    result = (filePath.IndexOf(".", filePath.LastIndexOf(@"\")) == -1);
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
                throw ex;
            }

            return result;
        }

        public static bool IsDirectory(string filePath, string refPath)
        {
            bool result = false;

            try
            {
                if (ZlpIOHelper.FileExists(filePath) || ZlpIOHelper.DirectoryExists(filePath))
                {
                    result = IsDirectory(filePath);
                }
                else
                {
                    result = IsDirectory(refPath);
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
                throw ex;
            }

            return result;
        }

        public static void CreateDirectory(string directory)
        {
            string basePath = "";

            try
            {
                if (directory.StartsWith(@"\\"))
                {
                    // UNC Path
                    basePath = @"\\" + directory.Split('\\')[2];
                }
                else
                {
                    // Hard Disk Drive
                    basePath = directory.Split('\\')[0];
                }

                if (ZlpIOHelper.DirectoryExists(basePath))
                {
                    ZlpIOHelper.CreateDirectory(directory);
                }
                else
                {
                    throw new IO.DirectoryNotFoundException("Path root doesn't exist.");
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
                throw ex;
            }
        }

        public static bool SynchFileEntry(FileItem file, MiirorItem mi)
        {
            PathManager pm = PathManager.GetInstance(mi);
            bool result = false;

            try
            {
                string targetPath = pm.ConvertPath(file.NewFile);

                switch (file.ChangeType)
                {
                    case IO.WatcherChangeTypes.Changed:
                        if ((ZlpIOHelper.GetFileAttributes(file.NewFile) & FileAttributes.Offline) == FileAttributes.Offline
                         || (ZlpIOHelper.FileExists(targetPath) && (ZlpIOHelper.GetFileAttributes(targetPath) & FileAttributes.Offline) == FileAttributes.Offline))
                        {
                            return false;
                        }

                        if (IsDirectory(targetPath, file.NewFile))
                        {
                            CreateDirectory(targetPath);
                        }
                        else
                        {
                            ZlpIOHelper.CopyFile(file.NewFile, targetPath, true);
                        }

                        break;
                    case IO.WatcherChangeTypes.Deleted:
                        if ((ZlpIOHelper.GetFileAttributes(targetPath) & FileAttributes.Offline) == FileAttributes.Offline)
                        {
                            return false;
                        }

                        if (IsDirectory(targetPath))
                        {
                            ZlpIOHelper.DeleteDirectory(targetPath, true);
                        }
                        else
                        {
                            ZlpIOHelper.DeleteFile(targetPath);
                        }

                        break;
                    case IO.WatcherChangeTypes.Renamed:
                        ZlpIOHelper.MoveDirectory(pm.ConvertPath(file.OldFile), targetPath);

                        break;
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }

            return result;
        }

        public static bool FilterFile(string file, List<string> removeList, List<string> keepList)
        {
            bool result = false;

            try
            {
                string ext = ZlpPathHelper.GetExtension(file).TrimStart('.').ToLower();
                ext = string.IsNullOrEmpty(ext) ? ext : "*." + ext;

                result = !(removeList.FindIndex(e => e.ToLower() == ext) > -1);
                if (0 == keepList.Count || keepList.Where(l => l.IndexOf("*.*") > -1).Count() > 0)
                {
                    result = true;
                }
                else
                {
                    result = keepList.FindIndex(e => e.ToLower() == ext) > -1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public static void Report(List<FileItem> fileList)
        {
            try
            {
                string fileName = ZlpPathHelper.Combine(ZlpPathHelper.GetTempDirectoryPath(), ZlpPathHelper.GetTempFilePath());

                ZlpIOHelper.WriteAllText(fileName, string.Join("\r\n", fileList.Select(f => string.Format("{0}\t{1}", f.ChangeType, f.NewFile)).ToArray()));

                Process.Start("notepad", fileName);
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
        }

        public static bool CheckMonitorPath(string monitorPath, int maxRetryCount, int reconnectSpan)
        {
            // The temporary folder path existence flag
            bool isTempPathExist = false;

            if (!ZlpIOHelper.DirectoryExists(monitorPath))
            {
                // Retry to connect the Temporary folder
                int i;
                for (i = 0; i < maxRetryCount; i++)
                {
                    if (ReConnect(monitorPath, reconnectSpan))
                    {
                        isTempPathExist = true;
                        break;
                    }
                }

                isTempPathExist = !(maxRetryCount == i);
            }
            else
            {
                isTempPathExist = true;
            }

            return isTempPathExist;
        }

        private static bool ReConnect(string monitorPath, int reconnectSpan)
        {
            try
            {
                if (ZlpIOHelper.DirectoryExists(monitorPath))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
                return false;
            }
            // Retry to connect the temporary folder with certain interval time
            Thread.Sleep(reconnectSpan);
            return false;
        }
    }
}
