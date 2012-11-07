using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Miiror.Utils
{
    internal static class FSOList
    {
        private static List<FSObject> list = new List<FSObject>();

        public static void Add(FSObject fi)
        {
            list.Add(fi);
        }

        public static void UpdateList()
        {
            List<FSObject> fl = new List<FSObject>();

            try
            {
                foreach (FSObject item in list)
                {
                    if (File.Exists(item.FullPathName))
                    {
                        fl.Add(item);
                    }
                }

                list = fl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsFile(string path)
        {
            return list.FindIndex(p => p.FullPathName.ToLower() == path.ToLower() && p.IsFile) > -1;
        }

        public static bool IsFolder(string path)
        {
            return list.FindIndex(p => p.FullPathName.ToLower() == path.ToLower() && !p.IsFile) > -1;
        }
    }
}
