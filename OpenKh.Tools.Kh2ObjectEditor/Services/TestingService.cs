using Microsoft.Win32;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    /*
     * THIS IS A SERVICE ONLY USED TO TEST AND RESEARCH DATA
     * It should not be available to end users in releases (Through GUI)
     */
    public class TestingService
    {
        public string FolderPath { get; set; }

        public void RunCode()
        {
            //Apdx_Service.Instance.loadFile(Apdx_Service.Instance.ApdxPath);

            //Mset_Service.Instance.loadMotion(0);

            //Mdlx_Service.Instance.loadMdlx(Mdlx_Service.Instance.MdlxPath);

            // OPEN FILE
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames != null && openFileDialog.FileNames.Length > 0)
            {
                string filePath = openFileDialog.FileNames[0];
                DpdPData testfile;
                //Mdlx_Service.Instance.TextureFile.TextureFooterData.TextureAnimationList.Add(ImageUtils.CreateTextureAnimation(openFileDialog.FileNames[0]));
                using (FileStream fs = File.OpenRead(filePath))
                {
                    testfile = new DpdPData(fs);
                }

                filePath = filePath + "_OUT";
                MemoryStream memStream = (MemoryStream)testfile.getAsStream();
                File.WriteAllBytes(filePath, memStream.ToArray());
            }
        }
        public void TestApdx()
        {
            int invalidFiles = 0;
            Dictionary<Bar.EntryType, int> barEntryCount = new Dictionary<Bar.EntryType, int>();

            foreach (string file in Directory.GetFiles(FolderPath))
            {
                if (!ObjectEditorUtils.isFilePathValid(file, new List<string> { "apdx", "a.fm", "a.fr", "a.gr", "a.it", "a.it", "a.jp", "a.sp", "a.uk", "a.us" }))
                {
                    continue;
                }

                using FileStream streamFile = File.Open(file, FileMode.Open);

                if (!Bar.IsValid(streamFile))
                {
                    invalidFiles++;
                    continue;
                }

                // Open file
                Bar fileBar = Bar.Read(streamFile);

                foreach (Bar.Entry barEntry in fileBar)
                {
                    if (!barEntryCount.ContainsKey(barEntry.Type))
                    {
                        barEntryCount.Add(barEntry.Type, 0);
                    }

                    barEntryCount.TryGetValue(barEntry.Type, out int count);
                    barEntryCount[barEntry.Type] = count + 1;
                }
            }
        }

        public void TestMdlx()
        {
            int invalidFiles = 0;
            Dictionary<Bar.EntryType, int> barEntryCount = new Dictionary<Bar.EntryType, int>();

            foreach (string file in Directory.GetFiles(FolderPath))
            {
                if (!ObjectEditorUtils.isFilePathValid(file, "mdlx"))
                {
                    continue;
                }

                using FileStream streamFile = File.Open(file, FileMode.Open);

                if (!Bar.IsValid(streamFile))
                {
                    invalidFiles++;
                    continue;
                }

                // Open file
                Bar fileBar = Bar.Read(streamFile);

                foreach (Bar.Entry barEntry in fileBar)
                {
                    if (!barEntryCount.ContainsKey(barEntry.Type))
                    {
                        barEntryCount.Add(barEntry.Type, 0);
                    }

                    barEntryCount.TryGetValue(barEntry.Type, out int count);
                    barEntryCount[barEntry.Type] = count + 1;
                }
            }
        }

        public void TestMset()
        {
            int invalidFiles = 0;
            Dictionary<Bar.EntryType, int> barEntryCount = new Dictionary<Bar.EntryType, int>();

            foreach (string file in Directory.GetFiles(FolderPath))
            {
                if (!ObjectEditorUtils.isFilePathValid(file, "mset"))
                {
                    continue;
                }

                using FileStream streamFile = File.Open(file, FileMode.Open);

                if (!Bar.IsValid(streamFile))
                {
                    invalidFiles++;
                    continue;
                }

                // Open file
                Bar fileBar = Bar.Read(streamFile);

                foreach (Bar.Entry barEntry in fileBar)
                {
                    if (!barEntryCount.ContainsKey(barEntry.Type))
                    {
                        barEntryCount.Add(barEntry.Type, 0);
                    }

                    barEntryCount.TryGetValue(barEntry.Type, out int count);
                    barEntryCount[barEntry.Type] = count + 1;
                }
            }
        }

        // SINGLETON
        private TestingService() { }
        private static TestingService _instance = null;
        public static TestingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TestingService();
                }
                return _instance;
            }
        }
    }
}
