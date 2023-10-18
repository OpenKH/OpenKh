using OpenKh.Kh2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using static OpenKh.Tools.Kh2MsetEditor.ViewModels.DataView_VM;

namespace OpenKh.Tools.Kh2MsetEditor.ViewModels
{
    public class Transfer_VM
    {
        public Bar ToMset { get; set; }
        public ObservableCollection<AnbEntryWrapper> toEntryList_View { get; set; }
        public Bar.Entry toSelected { get; set; }
        public Bar FromMset { get; set; }
        public ObservableCollection<AnbEntryWrapper> fromEntryList_View { get; set; }
        public Bar.Entry fromSelected { get; set; }
        public ObservableCollection<boneTransferWrapper> boneTransferWrappers { get; set; }
        public short ToBoneCount { get; set; }
        public short FromBoneCount { get; set; }

        public Transfer_VM() { }

        public void loadToFile(string filePath)
        {
            ToMset = fileToBar(filePath);
            loadToViewList();
        }
        public void loadFromFile(string filePath)
        {
            FromMset = fileToBar(filePath);
            loadFromViewList();
        }

        public Bar fileToBar(string filePath)
        {
            if (!DataView_VM.isValidFilepath(filePath))
                return null;

            using var stream = File.Open(filePath, FileMode.Open);
            if (!Bar.IsValid(stream))
                return null;

            return Bar.Read(stream);
        }

        public static void loadViewList(ObservableCollection<AnbEntryWrapper> anbEntries, Bar msetBar)
        {
            if (anbEntries == null)
                anbEntries = new ObservableCollection<AnbEntryWrapper>();

            anbEntries.Clear();
            int barIndex = 0;
            foreach (Bar.Entry barEntry in msetBar)
            {
                anbEntries.Add(new AnbEntryWrapper(barIndex, barEntry));
                barIndex++;
            }
        }

        public void loadToViewList()
        {
            if (toEntryList_View == null)
                toEntryList_View = new ObservableCollection<AnbEntryWrapper>();

            toEntryList_View.Clear();
            int barIndex = 0;
            foreach (Bar.Entry barEntry in ToMset)
            {
                toEntryList_View.Add(new AnbEntryWrapper(barIndex, barEntry));
                barIndex++;
            }
            searchToBoneCount();
        }
        public void loadFromViewList()
        {
            if (fromEntryList_View == null)
                fromEntryList_View = new ObservableCollection<AnbEntryWrapper>();

            fromEntryList_View.Clear();
            int barIndex = 0;
            foreach (Bar.Entry barEntry in FromMset)
            {
                fromEntryList_View.Add(new AnbEntryWrapper(barIndex, barEntry));
                barIndex++;
            }
        }

        public class boneTransferWrapper
        {
            public short FromBone { get; set; }
            public short? ToBone { get; set; }

            public boneTransferWrapper(short FromBone, short? ToBone)
            {
                this.FromBone = FromBone;
                this.ToBone = ToBone;
            }
        }
        public void loadBoneTransferWrappers()
        {
            if (boneTransferWrappers == null) {
                boneTransferWrappers = new ObservableCollection<boneTransferWrapper>();
            }
            boneTransferWrappers.Clear();

            for (short i = 0; i < FromBoneCount; i++) {
                boneTransferWrappers.Add(new boneTransferWrapper(i,null));
            }
        }
        public void loadBoneTransferFile(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (!line.Contains(","))
                    continue;

                string[] lineData = line.Split(",");
                if (lineData.Length != 2 || boneTransferWrappers == null)
                    continue;

                foreach (boneTransferWrapper transfer in boneTransferWrappers)
                {
                    if (transfer.FromBone == System.Int16.Parse(lineData[0])) {
                        transfer.ToBone = System.Int16.Parse(lineData[1]);
                    }
                }
                
            }
        }
        public void searchToBoneCount()
        {
            foreach(AnbEntryWrapper anbWrapper in toEntryList_View)
            {
                if (anbWrapper.Entry.Stream.Length < 1) {
                    continue;
                }
                anbWrapper.Entry.Stream.Position = 0;
                AnimationBinary anb = new AnimationBinary(anbWrapper.Entry.Stream);
                ToBoneCount = anb.MotionFile.InterpolatedMotionHeader.BoneCount;
                break;
            }
        }

        public void transferAnb()
        {
            if (toSelected == null || fromSelected == null || fromSelected.Stream.Length == 0)
                return;

            fromSelected.Stream.Position = 0;

            AnimationBinary fromAnb = new AnimationBinary(fromSelected.Stream);

            short boneCountFrom = fromAnb.MotionFile.InterpolatedMotionHeader.BoneCount;

            // BONE TRANSFERS
            Dictionary<short, short?> boneTransfers = new Dictionary<short, short?>();
            // Bones
            foreach (boneTransferWrapper boneTransfer in boneTransferWrappers)
            {
                boneTransfers.Add(boneTransfer.FromBone, boneTransfer.ToBone);
            }
            // IK Helpers
            foreach (int i in Enumerable.Range(0, fromAnb.MotionFile.IKHelpers.Count))
            {
                boneTransfers.Add(fromAnb.MotionFile.IKHelpers[i].Index, (short)(ToBoneCount + i));
            }
            // None
            boneTransfers.Add(-1, -1);

            // INITIAL POSE - Reassign used bones, delete unused 
            for (int i = fromAnb.MotionFile.InitialPoses.Count - 1; i >= 0; i--)
            {
                Motion.InitialPose initialPose = fromAnb.MotionFile.InitialPoses[i];

                if (boneTransfers[initialPose.BoneId] != null)
                {
                    initialPose.BoneId = (short)boneTransfers[initialPose.BoneId];
                }
                else
                {
                    fromAnb.MotionFile.InitialPoses.RemoveAt(i);
                }
            }

            // F-CURVE FORWARD - Reassign used bones, delete unused 
            // F-CURVE INVERSE - Remain the same
            int deletedFCurveCount = 0;
            for (int i = fromAnb.MotionFile.FCurvesForward.Count - 1; i >= 0; i--)
            {
                Motion.FCurve fCurve = fromAnb.MotionFile.FCurvesForward[i];

                if (boneTransfers[fCurve.JointId] != null)
                {
                    fCurve.JointId = (short)boneTransfers[fCurve.JointId];
                }
                else
                {
                    fromAnb.MotionFile.FCurvesForward.RemoveAt(i);
                    deletedFCurveCount++;
                }
            }


            // CONSTRAINTS - If it uses an unused bone delete, otherwise reassign
            for (int i = fromAnb.MotionFile.Constraints.Count - 1; i >= 0; i--)
            {
                Motion.Constraint constraint = fromAnb.MotionFile.Constraints[i];

                if (boneTransfers[constraint.ConstrainedJointId] == null || boneTransfers[constraint.SourceJointId] == null)
                {
                    fromAnb.MotionFile.Constraints.RemoveAt(i);
                    continue;
                }
                else
                {
                    constraint.ConstrainedJointId = (short)boneTransfers[constraint.ConstrainedJointId];
                    constraint.SourceJointId = (short)boneTransfers[constraint.SourceJointId];
                }
            }

            // EXPRESSIONS - Should always be over IK Helpers
            for (int i = fromAnb.MotionFile.Expressions.Count - 1; i >= 0; i--)
            {
                Motion.Expression expression = fromAnb.MotionFile.Expressions[i];

                if (boneTransfers[expression.TargetId] != null)
                {
                    expression.TargetId = (short)boneTransfers[expression.TargetId];
                }
                else
                {
                    //fromAnb.MotionFile.BoneFCurves.RemoveAt(i);
                }
            }

            // IK HELPERS
            for (int i = fromAnb.MotionFile.IKHelpers.Count - 1; i >= 0; i--)
            {
                Motion.IKHelper IkHelper = fromAnb.MotionFile.IKHelpers[i];
                IkHelper.Index = (short)boneTransfers[IkHelper.Index];
                IkHelper.ParentId = (short)boneTransfers[IkHelper.ParentId];
                IkHelper.SiblingId = (short)boneTransfers[IkHelper.SiblingId];
                IkHelper.ChildId = (short)boneTransfers[IkHelper.ChildId];
            }

            // JOINTS - CHECK LATER
            Dictionary<short, List<short>> IKRelocators = new Dictionary<short, List<short>>();
            Dictionary<short, byte> flagSaves = new Dictionary<short, byte>();
            IKRelocators.Add(-1, new List<short>());
            foreach (Motion.Joint joint in fromAnb.MotionFile.Joints)
            {
                if (boneTransfers[joint.JointId] != null)
                {
                    flagSaves.Add((short)boneTransfers[joint.JointId], joint.Flags);
                }

                if (joint.JointId >= boneCountFrom)
                {
                    IKRelocators[-1].Add((short)boneTransfers[joint.JointId]);
                }
                else if (IKRelocators[-1].Count != 0)
                {
                    short reloc = (short)boneTransfers[joint.JointId];
                    IKRelocators.Add(reloc, new List<short>());
                    foreach (short relocator in IKRelocators[-1])
                    {
                        IKRelocators[reloc].Add(relocator);
                    }
                    IKRelocators[-1].Clear();
                }
            }
            fromAnb.MotionFile.Joints.Clear();
            for (short i = 0; i < ToBoneCount; i++)
            {
                if (IKRelocators.ContainsKey(i))
                {
                    foreach (short reloc in IKRelocators[i])
                    {
                        fromAnb.MotionFile.Joints.Add(new Motion.Joint(reloc));
                    }
                }

                fromAnb.MotionFile.Joints.Add(new Motion.Joint(i));
            }
            if (IKRelocators.ContainsKey(-1))
            {
                foreach (short reloc in IKRelocators[-1])
                {
                    fromAnb.MotionFile.Joints.Add(new Motion.Joint(reloc));
                }
            }
            foreach (Motion.Joint joint in fromAnb.MotionFile.Joints)
            {
                if (flagSaves.ContainsKey(joint.JointId))
                {
                    joint.Flags = flagSaves[joint.JointId];
                }
            }

            // POSITION
            for (int i = 0; i < fromAnb.MotionFile.RootPosition.FCurveId.Length; i++)
            {
                if (fromAnb.MotionFile.RootPosition.FCurveId[i] != -1)
                {
                    fromAnb.MotionFile.RootPosition.FCurveId[i] -= deletedFCurveCount;
                }
            }

            fromAnb.MotionFile.InterpolatedMotionHeader.BoneCount = ToBoneCount;
            fromSelected.Stream = fromAnb.toStream(); // load new data on origin entry
            fromSelected.Stream.Position = 0;

            toSelected.Name = fromSelected.Name;
            toSelected.Index = fromSelected.Index;
            toSelected.Type = fromSelected.Type;
            toSelected.Stream = fromAnb.toStream();
            toSelected.Stream.Position = 0;
            toSelected.Type = fromSelected.Type;
        }
    }
}
