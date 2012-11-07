using System;
using System.Reflection;
using System.Collections.Generic;

namespace Miiror.Utils
{
    [Serializable]
    internal class MiirorSettings
    {
        public bool MinimizedClose = false;

        public bool ShowMinimizingBox = true;

        public bool StartupWithWindows = false;

        public string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);

        public List<MiirorItem> MonitorList = new List<MiirorItem>();
    }
}
