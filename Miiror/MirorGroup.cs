using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miiror.Utils;

namespace Miiror
{
    internal class MirorGroup
    {
        private List<iMiror> mirorGrp = null;

        public MirorGroup()
        {
            mirorGrp = new List<iMiror>();
        }

        public void AddMonitor(MiirorItem mi)
        {
            int index = FindIndex(mi);

            if (index > -1)
            {
                mirorGrp[index].Dispose();
                mirorGrp.RemoveAt(index);

            }

            mirorGrp.Add(new iMiror(mi));
        }

        public void AddMonitors(List<MiirorItem> miList)
        {
            foreach (MiirorItem item in miList)
            {
                AddMonitor(item);
            }
        }

        private int FindIndex(MiirorItem mi)
        {
            return mirorGrp.FindIndex(m => m.Identity == mi.Identity);
        }
    }
}
