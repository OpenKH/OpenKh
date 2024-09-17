using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class M_EffectDpds_VM
    {
        public ObservableCollection<DpdWrapper> DpdList { get; set; }

        public M_EffectDpds_VM()
        {
            DpdList = new ObservableCollection<DpdWrapper>();
            loadDpds();
        }

        public void loadDpds()
        {
            if (ApdxService.Instance.PaxFile?.DpxPackage?.DpdList == null || ApdxService.Instance.PaxFile.DpxPackage.DpdList.Count < 0)
                return;

            DpdList.Clear();
            for (int i = 0; i < ApdxService.Instance.PaxFile.DpxPackage.DpdList.Count; i++)
            {
                DpdWrapper wrapper = new DpdWrapper();
                wrapper.Id = i;
                wrapper.Name = "DPD " + i;
                wrapper.DpdItem = ApdxService.Instance.PaxFile.DpxPackage.DpdList[i];

                DpdList.Add(wrapper);
            }
        }
        public void Dpd_Copy(int index)
        {
            ClipboardService.Instance.StoreDpd(ApdxService.Instance.PaxFile.DpxPackage.DpdList[index]);
        }
        public void Dpd_AddCopied()
        {
            if (ClipboardService.Instance.FetchDpd() == null)
                return;

            ApdxService.Instance.PaxFile.DpxPackage.DpdList.Add(ClipboardService.Instance.FetchDpd());
            loadDpds();
        }
        public void Dpd_Replace(int index)
        {
            if (ClipboardService.Instance.FetchDpd() == null)
                return;

            ApdxService.Instance.PaxFile.DpxPackage.DpdList[index] = ClipboardService.Instance.FetchDpd();
            loadDpds();
        }
        public void Dpd_Export(int index)
        {
            Kh2.Dpd dpd = ApdxService.Instance.PaxFile.DpxPackage.DpdList[index];

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "DPD_" + index;
            dlg.DefaultExt = ".dpd";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                MemoryStream dpdStream = new MemoryStream();
                dpd.getAsStream().CopyTo(dpdStream);
                File.WriteAllBytes(dlg.FileName, dpdStream.ToArray());
            }
        }
        public void Dpd_Import()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    Kh2.Dpd dpd;
                    using (Stream stream = openFileDialog.OpenFile())
                    {
                        dpd = new Kh2.Dpd(stream);
                    }
                    ApdxService.Instance.PaxFile.DpxPackage.DpdList.Add(dpd);
                    loadDpds();
                }
            }
        }

        public class DpdWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Dpd DpdItem { get; set; }
        }
    }
}
