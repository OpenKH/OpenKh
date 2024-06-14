using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xe.BinaryMapper;

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
        private void Button_Test(object sender, RoutedEventArgs e)
        {
            if (ApdxService.Instance.PaxFile?.DpxPackage == null)
                return;

            TestEffectsIngame();
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


        // Tests the Effects ingame. Writes the element/caster entries.
        // IMPORTANT: elements must be of the same length, be careful because there's no control of this.
        public void TestEffectsIngame()
        {
            string filename = Path.GetFileName(ApdxService.Instance.ApdxPath);

            if (filename == "")
                return;

            long fileAddress;
            try
            {
                fileAddress = ProcessService.getAddressOfFile(filename);
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show("Game is not running", "There was an error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            if (fileAddress == 0)
            {
                System.Windows.Forms.MessageBox.Show("Couldn't find file", "There was an error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            int entryOffset = -1;
            foreach (Bar.Entry entry in ApdxService.Instance.ApdxBar)
            {
                if (entry.Type == Bar.EntryType.Pax)
                {
                    entryOffset = entry.Offset;
                    break;
                }
            }
            if (entryOffset == -1)
            {
                System.Windows.Forms.MessageBox.Show("AI file not found", "There was an error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            // ELEMENTS
            int paxHeaderSize = 16;
            long elementsAddress = fileAddress + entryOffset + paxHeaderSize;
            MemoryStream elementsStream = new MemoryStream();
            foreach (var elem in ApdxService.Instance.PaxFile.Elements)
            {
                BinaryMapping.WriteObject(elementsStream, elem);
            }
            byte[] elementsBytes = elementsStream.ToArray();
            MemoryAccess.writeMemory(ProcessService.KH2Process, elementsAddress, elementsBytes, true);

            // EFFECTS - Not as simple due to Ids being based on offsets
            //int dpxHeaderSize = 16;
            //int effectSize = 32;
            //long effectsAddress = fileAddress + entryOffset + ApdxService.Instance.PaxFile.Header.DpxOffset + dpxHeaderSize;
            //for (int i = 0; i < ApdxService.Instance.PaxFile.DpxPackage.ParticleEffects.Count; i++)
            //{
                //long effectAddress = effectsAddress + (effectSize * i) + 4; // First int is a dynamic offset
                //var dpd = ApdxService.Instance.PaxFile.DpxPackage.ParticleEffects[i];
                //MemoryStream effectsStream = new MemoryStream();
                //BinaryMapping.WriteObject(effectsStream, dpd);
                //byte[] effectsBytes = effectsStream.ToArray().Skip(4).ToArray();
                //MemoryAccess.writeMemory(ProcessService.KH2Process, effectAddress, effectsBytes, true);
            //}
        }
    }
}
