using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class AttachmentService
    {
        public string Attach_MdlxPath { get; set; }
        public int Attach_BoneId { get; set; }
        public Bar Attach_MdlxBar { get; set; }
        public ModelSkeletal Attach_ModelFile { get; set; }
        public ModelCollision Attach_CollisionFile { get; set; }
        public ModelTexture Attach_TextureFile { get; set; }
        public List<MotionSelector_Wrapper> Attach_MsetEntries { get; set; }
        public AnimationBinary Attach_AnimBinary { get; set; }

        public void LoadAttachment(string attachmentPath, int? attachToBone = null)
        {
            if (!ObjectEditorUtils.isFilePathValid(attachmentPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + attachmentPath);

            Attach_MdlxPath = attachmentPath;

            string tempMsetPath = attachmentPath.ToLower().Replace(".mdlx", ".mset");
            if (!ObjectEditorUtils.isFilePathValid(tempMsetPath, "mset"))
            {
                tempMsetPath = null;
            }

            using var streamMdlx = File.Open(attachmentPath, FileMode.Open);
            if (!Bar.IsValid(streamMdlx))
                throw new Exception("File is not a valid MDLX: " + attachmentPath);

            Attach_MdlxBar = Bar.Read(streamMdlx);

            foreach (Bar.Entry barEntry in Attach_MdlxBar)
            {
                try
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            Attach_ModelFile = ModelSkeletal.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelTexture:
                            Attach_TextureFile = ModelTexture.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelCollision:
                            Attach_CollisionFile = new ModelCollision(barEntry.Stream);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e) { }
            }

            if (attachToBone != null)
            {
                Attach_BoneId = attachToBone.Value;
            }

            // MSET
            if (tempMsetPath != null)
            {
                if (!ObjectEditorUtils.isFilePathValid(tempMsetPath, "mset"))
                    throw new FileNotFoundException("Mset does not exist: " + tempMsetPath);

                using var streamMset = File.Open(tempMsetPath, FileMode.Open);
                if (!Bar.IsValid(streamMset))
                    throw new Exception("File is not a valid MSET: " + tempMsetPath);

                BinaryArchive Attach_MsetBar = BinaryArchive.Read(streamMset);

                Attach_MsetEntries = new List<MotionSelector_Wrapper>();
                for (int i = 0; i < Attach_MsetBar.Entries.Count; i++)
                {
                    Attach_MsetEntries.Add(new MotionSelector_Wrapper(i, Attach_MsetBar.Entries[i]));
                }
            }
        }

        // SINGLETON
        private AttachmentService() { }
        private static AttachmentService _instance = null;
        public static AttachmentService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AttachmentService();
                }
                return _instance;
            }
        }
        public static void Reset()
        {
            _instance = new AttachmentService();
        }
    }
}
