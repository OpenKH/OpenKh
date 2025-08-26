using Assimp;
using OpenKh.Command.AnbMaker.Utils.AssimpAnimSource;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xe.BinaryMapper;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class ModuleMotions_VM : NotifyPropertyChangedBase
    {
        // MOTIONS
        public List<MotionSelector_Wrapper> Motions { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> MotionsView { get; set; }

        // FILTERS
        private string _filterName { get; set; } = "";
        public string FilterName
        {
            get { return _filterName; }
            set
            {
                _filterName = value;
                OnPropertyChanged("FilterName");
                OnPropertyChanged("AllowMotionMove");
            }
        }
        private bool _filterHideDummies { get; set; }
        public bool FilterHideDummies
        {
            get { return _filterHideDummies; }
            set
            {
                _filterHideDummies = value;
                OnPropertyChanged("FilterHideDummies");
                OnPropertyChanged("AllowMotionMove");
            }
        }
        public bool AllowMotionMove
        {
            get { return !FilterHideDummies && FilterName == ""; }
        }

        // CONSTRUCTOR
        public ModuleMotions_VM()
        {
            Motions = new List<MotionSelector_Wrapper>();
            MotionsView = new ObservableCollection<MotionSelector_Wrapper>();
            loadMotions();
            applyFilters();
        }

        // FUNCTIONS
        public void loadMotions()
        {
            Motions.Clear();

            if (MsetService.Instance.MsetBinarc == null)
                return;

            Motions = new List<MotionSelector_Wrapper>();
            for (int i = 0; i < MsetService.Instance.MsetBinarc.Entries.Count; i++)
            {
                Motions.Add(new MotionSelector_Wrapper(i, MsetService.Instance.MsetBinarc.Entries[i]));
            }
        }

        public void applyFilters()
        {
            MotionsView.Clear();
            foreach (MotionSelector_Wrapper iMotion in Motions)
            {
                if (FilterName != null && FilterName != "" && !iMotion.Name.ToLower().Contains(FilterName.ToLower()))
                {
                    continue;
                }
                if (FilterHideDummies && iMotion.IsDummy)
                {
                    continue;
                }

                MotionsView.Add(iMotion);
            }
        }

        public void Motion_Copy(int index)
        {
            ClipboardService.Instance.StoreMotion(MsetService.Instance.MsetBinarc.Subfiles[MsetService.Instance.MsetBinarc.Entries[index].Link]);
        }

        public void Motion_Replace(int index)
        {
            MsetService.Instance.MsetBinarc.Subfiles.Add(ClipboardService.Instance.FetchMotion());
            MsetService.Instance.MsetBinarc.Entries[index].Type = BinaryArchive.EntryType.Anb;
            MsetService.Instance.MsetBinarc.Entries[index].Name = "COPY";
            MsetService.Instance.MsetBinarc.Entries[index].Link = MsetService.Instance.MsetBinarc.Subfiles.Count - 1;

            loadMotions();
            applyFilters();
            App_Context.Instance.loadMotion(index);
        }
        public void Motion_Rename(int index)
        {
            //Bar.Entry item = MsetService.Instance.MsetBar[index];
        }
        public void Motion_Import(int index, string animationPath)
        {
            // Find rootNode
            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(animationPath, Assimp.PostProcessSteps.None);

            string rootNodeName = getRootNodeName(scene.RootNode);

            // Convert to interpolated motion
            IEnumerable<BasicSourceMotion> parms;
            parms = new UseAssimp(
                    inputModel: animationPath,
                    meshName: null,
                    rootName: rootNodeName,
                    animationName: null,
                    nodeScaling: 1,
                    positionScaling: 1
            )
                .Parameters;

            List<BasicSourceMotion> parList = parms.ToList();
            var builder = new InterpolatedMotionBuilder(parList[0]);
            var ipm = builder.Ipm;

            // Get as stream
            MemoryStream motionStream = (MemoryStream)ipm.toStream();
            
            // Insert to mset
            int subfileIndex = MsetService.Instance.MsetBinarc.Entries[index].Link;
            MemoryStream replaceMotionStream = new MemoryStream(MsetService.Instance.MsetBinarc.Subfiles[subfileIndex]);
            AnimationBinary msetEntry = new AnimationBinary(replaceMotionStream);
            msetEntry.MotionFile = new Motion.InterpolatedMotion(motionStream);
            MsetService.Instance.MsetBinarc.Subfiles[subfileIndex] = ((MemoryStream)msetEntry.toStream()).ToArray();
            
            loadMotions();
        }
        public void Motion_ImportEntry(int index, string msetPath, BinaryArchive.EntryType entryType)
        {
            using FileStream streamMset = File.Open(msetPath, FileMode.Open);
            //if (!Bar.IsValid(streamMset))
            //    throw new Exception("File is not a valid MSET: " + msetPath);


            MemoryStream msetStream = new MemoryStream();
            streamMset.CopyTo(msetStream);

            MsetService msetService = MsetService.Instance;

            // DUMMY
            if(MsetService.Instance.MsetBinarc.Entries[index].Link == -1)
            {
                MsetService.Instance.MsetBinarc.Subfiles.Add(new byte[0]);
                MsetService.Instance.MsetBinarc.Entries[index].Link = MsetService.Instance.MsetBinarc.Subfiles.Count - 1;
            }

            MsetService.Instance.MsetBinarc.Subfiles[MsetService.Instance.MsetBinarc.Entries[index].Link] = msetStream.ToArray();
            MsetService.Instance.MsetBinarc.Entries[index].Type = entryType;
            MsetService.Instance.MsetBinarc.Entries[index].Name = "IMPO";

            loadMotions();
            applyFilters();
            App_Context.Instance.loadMotion(index);
        }



        // Finds the node that serves as root for the animation
        private string getRootNodeName(Node rootNode)
        {
            string rootNodeName = "";
            if (isRootName(rootNode.Name))
            {
                rootNodeName = rootNode.Name;
            }
            else
            {
                List<string> nodeNames = GetAllNames(rootNode);
                foreach (string nodeName in nodeNames)
                {
                    if (isRootName(nodeName))
                    {
                        rootNodeName = nodeName;
                        break;
                    }
                }
            }
            return rootNodeName;
        }
        private static List<string> GetAllNames(Node node)
        {
            List<string> names = new List<string>();
            if (node != null)
            {
                Traverse(node, names);
            }
            return names;
        }
        private static void Traverse(Node node, List<string> names)
        {
            names.Add(node.Name);
            foreach (Node child in node.Children)
            {
                Traverse(child, names);
            }
        }
        // Returns true if the given string contains the word bone, contains numbers and all of its numbers are zeros
        private bool isRootName(string nodeName)
        {
            if (nodeName.ToLower().Contains("bone"))
            {
                // Regex to find all numbers in the string
                Regex numberPattern = new Regex(@"\d+");
                MatchCollection matches = numberPattern.Matches(nodeName);

                foreach (Match match in matches)
                {
                    string number = match.Value;
                    if(number == "" || match.Value.Trim('0') != "") {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        // Tests the mset ingame. Writes the motion header and motion triggers.
        // IMPORTANT: Triggers must be of the same length, be careful because there's no control of this.
        public void TestMsetIngame()
        {
            string filename = Path.GetFileName(MsetService.Instance.MsetPath);

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

            byte[] msetBytes = MsetService.Instance.MsetBinarc.getAsByteArray();
            Bar msetBar = Bar.Read(new MemoryStream(msetBytes));
            foreach (Bar.Entry barMotion in msetBar)
            {
                if (barMotion.Stream.Length == 0 || barMotion.Type != Bar.EntryType.Anb)
                    continue;

                int entryOffset = barMotion.Offset;

                // Read Anb
                barMotion.Stream.Position = 0;
                AnimationBinary motionAnb = new AnimationBinary(barMotion.Stream);
                barMotion.Stream.Position = 0;

                // Get trigger file offset
                Bar barMotionAsBar = Bar.Read(barMotion.Stream);
                barMotion.Stream.Position = 0;
                int motionOffset = barMotionAsBar[0].Offset;
                int triggerOffset = barMotionAsBar[1].Offset;
                int reservedLength = 144;

                long motionFileOffset = fileAddress + entryOffset + motionOffset + reservedLength;
                long triggerFileOffset = fileAddress + entryOffset + triggerOffset;

                // Write to process memory
                if (motionAnb.MotionFile != null && motionAnb.MotionFile.MotionHeader.Type == 0)
                {
                    MemoryStream motionStream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(motionStream);

                    BinaryMapping.WriteObject(motionStream, motionAnb.MotionFile.MotionHeader);
                    BinaryMapping.WriteObject(motionStream, motionAnb.MotionFile.InterpolatedMotionHeader);

                    byte[] motionBytes = motionStream.ToArray();
                    MemoryAccess.writeMemory(ProcessService.KH2Process, motionFileOffset, motionBytes, true);
                }
                if (motionAnb.MotionTriggerFile != null)
                {
                    MemoryStream triggerStream = (MemoryStream)motionAnb.MotionTriggerFile.toStream();
                    byte[] triggerBytes = triggerStream.ToArray();
                    MemoryAccess.writeMemory(ProcessService.KH2Process, triggerFileOffset, triggerBytes, true);
                }
            }
        }
    }
}
