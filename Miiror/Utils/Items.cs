using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Miiror.Utils
{
    internal struct MiirorItem
    {
        public string Source;
        public string Target;
        public string Identity;
        public bool IsFolder;
        public bool IsRecursive;
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
