using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Miiror.Utils;
using Miiror.Controls;

namespace Miiror
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            //System.IO.Directory.CreateDirectory(@"D:\tmpfolder\112233\445566\78");
            //MiirorItem mi = new MiirorItem() { Identity = "123", IsFolder = true, Source = @"V:\Test\A", Target = @"V:\Test\B" };
            //FSOpt.Report(FileScanManager.GetInstance(mi).LooseScan());
            //FSOpt.Report(FileScanManager.GetInstance(mi).RestrictScan());

            MiirorListBox miiListBox = new MiirorListBox();

            miiListBox.Dock = DockStyle.Fill;
            miiListBox.Items.AddRange(new string[] {"123123123123", "45456456456456"});

            this.Controls.Add(miiListBox);
        }
    }
}
