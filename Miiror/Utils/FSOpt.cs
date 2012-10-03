using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Miiror.Utils
{
    internal static class FSOpt
    {
        public static bool IsDirectory(string filePath)
        {
            bool result = false;

            try
            {
                if (File.Exists(filePath))
                {
                    result = ((File.GetAttributes(filePath) | FileAttributes.Directory) == FileAttributes.Directory);
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

                if (Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    throw new DirectoryNotFoundException("Path root doesn't exist.");
                }
            }
            catch(Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
        }

        public static bool SynchFileEntry(FileItem file, MiirorItem mi)
        {
            PathManager pm = PathManager.GetInstance(mi);
            string targetPath = pm.ConvertPath(file.NewFile);
            bool result = false;

            try
            {
                switch (file.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        if ((File.GetAttributes(file.NewFile) | FileAttributes.Offline) == FileAttributes.Offline
                         || (File.GetAttributes(targetPath) | FileAttributes.Offline) == FileAttributes.Offline)
                        {
                            return false;
                        }

                        if (IsDirectory(targetPath))
                        {
                            CreateDirectory(targetPath);
                        }
                        else
                        {
                            File.Copy(file.NewFile, targetPath, true);
                        }

                        break;
                    case WatcherChangeTypes.Deleted:
                        if ((File.GetAttributes(targetPath) | FileAttributes.Offline) == FileAttributes.Offline)
                        {
                            return false;
                        }

                        if (IsDirectory(targetPath))
                        {
                            Directory.Delete(targetPath, true);
                        }
                        else
                        {
                            File.Delete(targetPath);
                        }

                        break;
                    case WatcherChangeTypes.Renamed:
                        Directory.Move(pm.ConvertPath(file.OldFile), targetPath);

                        break;
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Log.GetInstance().WriteLog(ex.Message);
            }

            return result;
        }

        public static void Report(List<FileItem> fileList)
        {
            try
            {
                string fileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                File.WriteAllLines(fileName, fileList.Select(f => string.Format("{0}\t{1}", f.ChangeType, f.NewFile)).ToArray(), Encoding.UTF8);

                Process.Start("notepad", fileName);
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLog(ex.Message);
            }
        }
    }
}
