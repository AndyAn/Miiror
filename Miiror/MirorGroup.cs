using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miiror.Utils;
using System.Collections.ObjectModel;
using System.Collections;

namespace Miiror
{
    [Serializable]
    internal class MirorGroup
    {
        private List<iMiror> mirorGrp = null;
        private CallBackEventHandler OnCallBackEvent;

        public MirorGroup()
        {
            mirorGrp = new List<iMiror>();
        }

        public void AddMonitor(MiirorItem mi)
        {
            iMiror imi = null;

            if (string.IsNullOrEmpty(mi.Identity))
            {
                mi.Identity = mi.GetMD5Hash();
                imi = new iMiror(mi);
                imi.CallBack += new CallBackEventHandler(OnCallBackEvent);
                mirorGrp.Add(imi);
            }
            else
            {
                int index = FindIndex(mi);

                if (index > -1)
                {
                    mi.Identity = mi.GetMD5Hash();
                    mirorGrp[index].Dispose();
                    imi = new iMiror(mi);
                    imi.CallBack += new CallBackEventHandler(OnCallBackEvent);
                    mirorGrp[index] = imi;
                }
            }
        }

        public void AddMonitors(List<MiirorItem> miList)
        {
            foreach (MiirorItem item in miList)
            {
                AddMonitor(item);
            }
        }

        //public void UpdateMonitor(MiirorItem mi)
        //{
        //    int index = FindIndex(mi);

        //    if (index > -1)
        //    {
        //        mi.Identity = mi.GetMD5Hash();
        //        mirorGrp[index] = new iMiror(mi);
        //    }
        //    else
        //    {
        //        mirorGrp.Add(new iMiror(mi));
        //    }
        //}

        public void RemoveMonitor(string identity)
        {
            mirorGrp.RemoveAll(mi => mi.MirorItem.Identity == identity);
        }

        public ObservableCollection<MiirorItemDO> GetSource()
        {
            return new ObservableCollection<MiirorItemDO>(mirorGrp.Select(mi =>
                            new MiirorItemDO()
                            {
                                Source = mi.MirorItem.Source,
                                Target = mi.MirorItem.Target,
                                Filtered = mi.MirorItem.Filtered,
                                Identity = mi.MirorItem.Identity,
                                IsFolder = mi.MirorItem.IsFolder,
                                IsRecursive = mi.MirorItem.IsRecursive,
                                IsWorking = mi.MirorItem.IsWorking,
                                WorkingDisplay = (mi.MirorItem.IsWorking ? "On Monitoring" : "Stopped")
                            }));
        }

        public List<iMiror> GetMirors()
        {
            return mirorGrp;
        }

        private int FindIndex(MiirorItem mi)
        {
            return mirorGrp.FindIndex(m => m.MirorItem.Identity == mi.Identity);
        }

        public event CallBackEventHandler CallBack
        {
            add { OnCallBackEvent += new CallBackEventHandler(value); }
            remove { OnCallBackEvent -= new CallBackEventHandler(value); }
        }
    }
}
