using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_Effects_Control : UserControl
    {
        public M_Effects_Control()
        {
            InitializeComponent();
        }

        private void Button_ExportDpx(object sender, RoutedEventArgs e)
        {
            if (ApdxService.Instance.PaxFile?.DpxPackage == null)
                return;

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = "Effects.dpx";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                Stream tempStream = ApdxService.Instance.PaxFile.DpxPackage.getAsStream();
                MemoryStream memStream = new MemoryStream();
                tempStream.CopyTo(memStream);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }
        private void Button_ImportDpx(object sender, RoutedEventArgs e)
        {
            if (ApdxService.Instance.PaxFile?.DpxPackage == null)
                return;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "DPX file | *.dpx";
            bool? success = openFileDialog.ShowDialog();

            if (success == true)
            {
                if (Directory.Exists(openFileDialog.FileName))
                {
                    return;
                }
                else if (File.Exists(openFileDialog.FileName))
                {
                    using (FileStream fs = File.OpenRead(openFileDialog.FileName))
                    {
                        Dpx dpxFile = new Dpx(fs);
                        importDpx(dpxFile);
                    }
                }
            }
        }

        private void importDpx(Dpx dpxFile)
        {
            int dpdCount = ApdxService.Instance.PaxFile.DpxPackage.DpdList.Count;
            int effectCount = ApdxService.Instance.PaxFile.DpxPackage.ParticleEffects.Count;
            foreach (Dpd dpd in dpxFile.DpdList)
            {
                ApdxService.Instance.PaxFile.DpxPackage.DpdList.Add(dpd);
            }

            foreach (Dpx.DpxParticleEffect effect in dpxFile.ParticleEffects)
            {
                effect.DpdId += dpdCount;
                effect.EffectNumber += (uint)effectCount;
                ApdxService.Instance.PaxFile.DpxPackage.ParticleEffects.Add(effect);
            }
        }
    }
}
