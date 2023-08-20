using OpenKh.Kh2;
using OpenKh.Kh2.Utils;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class Apdx_Service
    {
        // Apdx File
        public string ApdxPath { get; set; }
        public Bar ApdxBar { get; set; }

        // Bar Entries - Ordered in occurrence count in the files in the obj folder
        public Pax PaxFile { get; set; } // 979
        // Wd (32, BGM Instrument Data) - 734
        // Seb (31) - 734
        public Imgd ImgdFile { get; set; } // 220
        // Seqd (25) - 220
        // IopVoice (34) - 144
        // Event (22) - 349
        // BarUnknown (46) - 8
        // WrappedCollisionData (38) - 21
        // Dummy (0) - 40

        public void loadFile(string filepath)
        {
            // Validations
            if (!ObjectEditorUtils.isFilePathValid(filepath, new List<string> { "apdx", "a.fm", "a.fr", "a.gr", "a.it", "a.it", "a.jp", "a.sp", "a.uk", "a.us" }))
                throw new Exception("File not valid");

            ApdxPath = filepath;

            using FileStream streamApdxFile = File.Open(ApdxPath, FileMode.Open);

            if (!Bar.IsValid(streamApdxFile))
                throw new Exception("File is not a valid BAR: " + ApdxPath);

            // Open file
            ApdxBar = Bar.Read(streamApdxFile);

            foreach (Bar.Entry barEntry in ApdxBar)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Pax:
                        PaxFile = new Pax(barEntry.Stream);
                        break;
                    case Bar.EntryType.Imgd:
                        ImgdFile = Imgd.Read(barEntry.Stream);
                        break;
                    case Bar.EntryType.Seqd:
                        //SqdFile = Sqd.Read(barEntry.Stream);
                        break;
                    default:
                        break;
                }
            }
        }

        public void saveToBar()
        {
            foreach (Bar.Entry barEntry in ApdxBar)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Pax:
                        barEntry.Stream = PaxFile.getAsStream();
                        break;
                    case Bar.EntryType.Imgd:
                        //barEntry.Stream = ;
                        break;
                    case Bar.EntryType.Seqd:
                        //barEntry.Stream = ;
                        break;
                    default:
                        break;
                }
            }
        }

        public void saveFile()
        {
            saveToBar();

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            string filename = Path.GetFileName(ApdxPath);
            filename = filename.Replace(".a.", ".out.a.");
            sfd.FileName = filename;
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, ApdxBar);
                /*if(memStream.Length % 16 != 0) // PC pads BAR files but it may not be needed
                {
                    int paddingSize = ReadWriteUtils.bytesRequiredToAlignToByte(memStream.Position, 16);
                    ReadWriteUtils.addBytesToStream(memStream, paddingSize, 0xCD);
                }*/
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }

        // SINGLETON
        private Apdx_Service() { }
        private static Apdx_Service instance = null;
        public static Apdx_Service Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Apdx_Service();
                }
                return instance;
            }
        }
        public static void reset()
        {
            instance = new Apdx_Service();
        }
    }
}
