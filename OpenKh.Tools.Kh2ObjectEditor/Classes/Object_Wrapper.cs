using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Classes
{
    public class Object_Wrapper
    {
        public string Name { get; set; }
        public string MdlxPath { get; set; }
        public string MsetPath { get; set; }

        public Bar MdlxBar { get; set; }
        public BinaryArchive MsetBar { get; set; }

        public ModelSkeletal ModelFile { get; set; }
        public ModelCollision CollisionFile { get; set; }
        public ModelTexture TextureFile { get; set; }
        public List<MotionSelector_Wrapper> MsetEntries { get; set; }

        public Object_Wrapper() { }

        public Object_Wrapper(string mdlxPath, string msetPath)
        {
            MdlxPath = mdlxPath;
            MsetPath = msetPath;
            readFiles();
            openFiles();
        }

        public void readFiles()
        {
            if (!isMdlxPathValid())
                throw new FileNotFoundException("File does not exist: " + MdlxPath);

            // MDLX
            using var streamMdlx = File.Open(MdlxPath, FileMode.Open);
            if (!Bar.IsValid(streamMdlx))
                throw new Exception("File is not a valid MDLX: " + MdlxPath);

            MdlxBar = Bar.Read(streamMdlx);

            // MSET
            if(MsetPath != null && File.Exists(MsetPath))
            {
                using var streamMset = File.Open(MsetPath, FileMode.Open);
                if (!Bar.IsValid(streamMset))
                    throw new Exception("File is not a valid MSET: " + MsetPath);

                MsetBar = BinaryArchive.Read(streamMset);
            }
        }

        public void openFiles()
        {
            if(MdlxBar != null)
            {
                foreach (Bar.Entry barEntry in MdlxBar)
                {
                    try
                    {
                        switch (barEntry.Type)
                        {
                            case Bar.EntryType.Model:
                                ModelFile = ModelSkeletal.Read(barEntry.Stream);
                                break;
                            case Bar.EntryType.ModelTexture:
                                TextureFile = ModelTexture.Read(barEntry.Stream);
                                break;
                            case Bar.EntryType.ModelCollision:
                                CollisionFile = new ModelCollision(barEntry.Stream);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception e) { }
                }
            }
            if(MsetBar != null)
            {
                MsetEntries = new List<MotionSelector_Wrapper>();
                for(int i = 0; i < MsetBar.Entries.Count; i++)
                {
                    MsetEntries.Add(new MotionSelector_Wrapper(i, MsetBar.Entries[i]));
                }
            }
        }

        public bool isMdlxPathValid()
        {
            if (!File.Exists(MdlxPath)) return false;
            if (!MdlxPath.ToLower().EndsWith(".mdlx")) return false;

            return true;
        }
        public bool isMsetPathValid()
        {
            if (!File.Exists(MsetPath)) return false;
            if (!MsetPath.ToLower().EndsWith(".mset")) return false;

            return true;
        }
    }
}
