using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZetaLongPaths;

namespace Miiror.Utils
{
    internal class PathManager
    {
        private MiirorItem miirorItem;

        private PathManager(MiirorItem mi)
        {
            miirorItem = mi;
        }

        public static PathManager GetInstance(MiirorItem mi)
        {
            return new PathManager(mi);
        }

        public string ConvertPath(string path)
        {
            string converted;

            try
            {
                converted = ZlpPathHelper.Combine(miirorItem.Target.TrimEnd('\\'), path.Replace(miirorItem.Source, "").TrimStart('\\'));
                if (ZlpPathHelper.GetPathRoot(converted) == "\\")
                {
                    converted = miirorItem.Target.TrimEnd('\\') + "\\" + path.Replace(miirorItem.Source, "").TrimStart('\\');
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return converted;
        }
    }
}
