using Assimp;
using OpenKh.Command.AnbMaker.Utils.AssimpAnimSource;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class ModuleMotions_VM : NotifyPropertyChangedBase
    {
        // MOTIONS
        public List<MotionSelector_Wrapper> Motions { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> MotionsView { get; set; }
        public Bar.Entry copiedMotion { get; set; }

        // FILTERS
        private string _filterName { get; set; }
        public string FilterName
        {
            get { return _filterName; }
            set
            {
                _filterName = value;
                OnPropertyChanged("FilterName");
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
            }
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

            if (MsetService.Instance.MsetBar == null)
                return;

            Motions = new List<MotionSelector_Wrapper>();
            for (int i = 0; i < MsetService.Instance.MsetBar.Count; i++)
            {
                Motions.Add(new MotionSelector_Wrapper(i, MsetService.Instance.MsetBar[i]));
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
            Bar.Entry item = MsetService.Instance.MsetBar[index];
            copiedMotion = new Bar.Entry();
            copiedMotion.Index = item.Index;
            copiedMotion.Name = item.Name;
            copiedMotion.Type = item.Type;
            item.Stream.Position = 0;
            copiedMotion.Stream = new MemoryStream();
            item.Stream.CopyTo(copiedMotion.Stream);
            item.Stream.Position = 0;
            copiedMotion.Stream.Position = 0;
        }

        public void Motion_Replace(int index)
        {
            if (copiedMotion == null)
                return;

            MsetService.Instance.MsetBar[index] = copiedMotion;
            loadMotions();
            applyFilters();
        }
        public void Motion_Rename(int index)
        {
            Bar.Entry item = MsetService.Instance.MsetBar[index];
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
            var motionStream = (MemoryStream)ipm.toStream();

            // Insert to mset
            MsetService.Instance.MsetBar[index].Stream.Position = 0;
            AnimationBinary msetEntry = new AnimationBinary(MsetService.Instance.MsetBar[index].Stream);
            msetEntry.MotionFile = new Motion.InterpolatedMotion(motionStream);
            MsetService.Instance.MsetBar[index].Stream = msetEntry.toStream();

            loadMotions();
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
    }
}
