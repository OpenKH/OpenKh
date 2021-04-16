using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using OpenKh.Bbs;
using Xe.BinaryMapper;
using OpenKh.Common;

namespace OpenKh.Tools.PMORetarget
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private static readonly IBinaryMapping Mapping =
           MappingConfiguration.DefaultConfiguration()
               .ForTypeMatrix4x4()
               .Build();

        Pmo srcPmo;
        Pmo.SkeletonHeader targetSkeletonHeader;
        Pmo.BoneData[] targetBones;

        private void PMOSourceButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "BBS Model (*.pmo)|*.pmo|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                FileStream srcFile = File.Open(dialog.FileName, FileMode.Open);
                srcPmo = Pmo.Read(srcFile);
                srcFile.Close();
            }
        }

        private void PMOTargetButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "BBS Model (*.pmo)|*.pmo|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                FileStream trgFile = File.OpenRead(dialog.FileName);
                trgFile.Seek(0xC, SeekOrigin.Begin);
                BinaryReader r = new BinaryReader(trgFile);
                trgFile.Seek(r.ReadUInt32(), SeekOrigin.Begin);

                targetSkeletonHeader = BinaryMapping.ReadObject<Pmo.SkeletonHeader>(trgFile);
                targetBones = new Pmo.BoneData[targetSkeletonHeader.BoneCount];
                for (int j = 0; j < targetSkeletonHeader.BoneCount; j++)
                {
                    targetBones[j] = Mapping.ReadObject<Pmo.BoneData>(trgFile);
                }

                trgFile.Close();
            }
        }

        private void ApplyRetargettingButton_Click(object sender, EventArgs e)
        {
            if(srcPmo != null && targetSkeletonHeader != null)
            {
                var srcBoneList = srcPmo.boneList.ToList();
                var trgBoneList = targetBones.ToList();

                foreach(Pmo.MeshChunks chunk in srcPmo.Meshes)
                {
                    for(int i = 0; i < chunk.SectionInfo_opt1.SectionBoneIndices.Length; i++)
                    {
                        if(chunk.SectionInfo_opt1.SectionBoneIndices[i] != 0xFF)
                        {
                            var a = srcBoneList[chunk.SectionInfo_opt1.SectionBoneIndices[i]].JointName;
                            var foundBone = trgBoneList.Find(x => x.JointName == a);

                            if (foundBone != null)
                            {
                                chunk.SectionInfo_opt1.SectionBoneIndices[i] = (byte)foundBone.BoneIndex;
                            }
                        }
                    }
                }

                foreach (Pmo.BoneData bData in targetBones)
                {
                    var b = srcBoneList.Find(x => x.JointName == bData.JointName);

                    if(b != null)
                    {
                        bData.Transform = b.Transform;
                        bData.InverseTransform = b.InverseTransform;
                    }
                }

            }

            srcPmo.skeletonHeader = targetSkeletonHeader;
            srcPmo.boneList = targetBones;
            FileStream n = File.Create("Test.pmo");
            Pmo.Write(n, srcPmo);
            n.Close();
        }
    }
}
