using Microsoft.Win32;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            Dictionary<byte, HashSet<int>> rangeTriggerCount = new Dictionary<byte, HashSet<int>>();
            Dictionary<byte, Dictionary<byte, string>> rangeTriggerSample = new Dictionary<byte, Dictionary<byte, string>>();
            Dictionary<byte, HashSet<int>> frameTriggerCount = new Dictionary<byte, HashSet<int>>();
            Dictionary<byte, Dictionary<byte, string>> frameTriggerSample = new Dictionary<byte, Dictionary<byte, string>>();
            HashSet<int> param36 = new HashSet<int>();
            HashSet<int> param37 = new HashSet<int>();

            List<string> preferredFiles = new List<string> { "B_", "F_", "P_", "W_", "WM" };

            foreach (string file in Directory.GetFiles(FolderPath))
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                string fileStart = filename.Substring(0,2);

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
                    // Check count per type
                    //if (!barEntryCount.ContainsKey(barEntry.Type))
                    //{
                    //    barEntryCount.Add(barEntry.Type, 0);
                    //}
                    //
                    //barEntryCount.TryGetValue(barEntry.Type, out int count);
                    //barEntryCount[barEntry.Type] = count + 1;

                    if (barEntry.Type == Bar.EntryType.Motionset)
                    {
                        foreach (Bar.Entry barEntry2 in Bar.Read(barEntry.Stream))
                        {
                            try
                            {
                                AnimationBinary anbEntry = new AnimationBinary(barEntry2.Stream);
                                if (anbEntry.MotionTriggerFile != null)
                                {
                                    foreach (var rangeTrigger in anbEntry.MotionTriggerFile.RangeTriggerList)
                                    {
                                        // Count
                                        if (!rangeTriggerCount.ContainsKey(rangeTrigger.Trigger))
                                        {
                                            rangeTriggerCount.Add(rangeTrigger.Trigger, new HashSet<int>());
                                        }
                                        rangeTriggerCount[rangeTrigger.Trigger].Add(rangeTrigger.ParamSize);

                                        // Sample
                                        if (!rangeTriggerSample.ContainsKey(rangeTrigger.Trigger))
                                        {
                                            rangeTriggerSample.Add(rangeTrigger.Trigger, new Dictionary<byte, string>());
                                        }
                                        if (!rangeTriggerSample[rangeTrigger.Trigger].ContainsKey(rangeTrigger.ParamSize))
                                        {
                                            rangeTriggerSample[rangeTrigger.Trigger].Add(rangeTrigger.ParamSize, "");
                                        }
                                        if(rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize] == "" ||
                                            (!preferredFiles.Contains(rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize].Substring(0,2)) && (preferredFiles.Contains(fileStart)))
                                           )
                                        {
                                            rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize] = "" + (filename + " - " + barEntry.Name + "_" + barEntry2.Name);
                                        }
                                    }
                                    foreach (var frameTrigger in anbEntry.MotionTriggerFile.FrameTriggerList)
                                    {
                                        // Count
                                        if (!frameTriggerCount.ContainsKey(frameTrigger.Trigger))
                                        {
                                            frameTriggerCount.Add(frameTrigger.Trigger, new HashSet<int>());
                                        }
                                        frameTriggerCount[frameTrigger.Trigger].Add(frameTrigger.ParamSize);

                                        // Sample
                                        if (!frameTriggerSample.ContainsKey(frameTrigger.Trigger))
                                        {
                                            frameTriggerSample.Add(frameTrigger.Trigger, new Dictionary<byte, string>());
                                        }
                                        if (!frameTriggerSample[frameTrigger.Trigger].ContainsKey(frameTrigger.ParamSize))
                                        {
                                            frameTriggerSample[frameTrigger.Trigger].Add(frameTrigger.ParamSize, "");
                                        }
                                        if (frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize] == "" ||
                                            (!preferredFiles.Contains(frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize].Substring(0, 2)) && (preferredFiles.Contains(fileStart)))
                                           )
                                        {
                                            frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize] = "" + (filename + " - " + barEntry.Name + "_" + barEntry2.Name);
                                        }
                                    }
                                }
                            }
                            catch (Exception exc) { }
                        }
                        continue;
                    }

                    if (barEntry.Type != Bar.EntryType.Anb || barEntry.Stream.Length == 0)
                        continue;

                    // Check Pattern enable/disable
                    try
                    {
                        AnimationBinary anbEntry = new AnimationBinary(barEntry.Stream);
                        if (anbEntry.MotionTriggerFile != null)
                        {
                            foreach (var rangeTrigger in anbEntry.MotionTriggerFile.RangeTriggerList)
                            {
                                if (!rangeTriggerCount.ContainsKey(rangeTrigger.Trigger)) {
                                    rangeTriggerCount.Add(rangeTrigger.Trigger, new HashSet<int>());
                                }
                                rangeTriggerCount[rangeTrigger.Trigger].Add(rangeTrigger.ParamSize);
                                //if (rangeTrigger.Trigger == 36)
                                //{
                                //    param36.Add(rangeTrigger.Param1);
                                //}
                                //else if (rangeTrigger.Trigger == 37)
                                //{
                                //    param37.Add(rangeTrigger.Param1);
                                //}

                                // Sample
                                if (!rangeTriggerSample.ContainsKey(rangeTrigger.Trigger))
                                {
                                    rangeTriggerSample.Add(rangeTrigger.Trigger, new Dictionary<byte, string>());
                                }
                                if (!rangeTriggerSample[rangeTrigger.Trigger].ContainsKey(rangeTrigger.ParamSize))
                                {
                                    rangeTriggerSample[rangeTrigger.Trigger].Add(rangeTrigger.ParamSize, "");
                                }
                                if (rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize] == "" ||
                                    (!preferredFiles.Contains(rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize].Substring(0, 2)) && (preferredFiles.Contains(fileStart))) ||
                                    (preferredFiles.Contains(fileStart) && rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize].Contains("_"))
                                   )
                                {
                                    rangeTriggerSample[rangeTrigger.Trigger][rangeTrigger.ParamSize] = "" + (filename + " - " + barEntry.Name);
                                }
                            }
                            foreach (var frameTrigger in anbEntry.MotionTriggerFile.FrameTriggerList)
                            {
                                if (!frameTriggerCount.ContainsKey(frameTrigger.Trigger))
                                {
                                    frameTriggerCount.Add(frameTrigger.Trigger, new HashSet<int>());
                                }
                                frameTriggerCount[frameTrigger.Trigger].Add(frameTrigger.ParamSize);


                                // Sample
                                if (!frameTriggerSample.ContainsKey(frameTrigger.Trigger))
                                {
                                    frameTriggerSample.Add(frameTrigger.Trigger, new Dictionary<byte, string>());
                                }
                                if (!frameTriggerSample[frameTrigger.Trigger].ContainsKey(frameTrigger.ParamSize))
                                {
                                    frameTriggerSample[frameTrigger.Trigger].Add(frameTrigger.ParamSize, "");
                                }
                                if (frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize] == "" ||
                                    (!preferredFiles.Contains(frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize].Substring(0, 2)) && (preferredFiles.Contains(fileStart)) ||
                                    (preferredFiles.Contains(fileStart) && frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize].Contains("_")))
                                   )
                                {
                                    frameTriggerSample[frameTrigger.Trigger][frameTrigger.ParamSize] = "" + (filename + " - " + barEntry.Name);
                                }
                            }
                        }
                    }
                    catch(Exception exc) { }
                }
            }

            Debug.WriteLine("Range Triggers");
            foreach (byte rangeTrigger in rangeTriggerCount.Keys) {
                Debug.Write("[" + rangeTrigger + "] " + String.Join(",", rangeTriggerCount[rangeTrigger]) + "; S: ");
                Debug.WriteLine(StringifyDictionary(rangeTriggerSample[rangeTrigger]));
            }
            Debug.WriteLine("Frame Triggers");
            foreach (byte frameTrigger in frameTriggerCount.Keys)
            {
                Debug.Write("[" + frameTrigger + "] " + String.Join(",", frameTriggerCount[frameTrigger]) + "; S: ");
                Debug.WriteLine(StringifyDictionary(frameTriggerSample[frameTrigger]));
            }
        }

        private string StringifyDictionary<TKey, Tvalue>(Dictionary<TKey, Tvalue> dictio)
        {
            List<string> stringified = new List<string>();
            foreach(var key in dictio.Keys)
            {
                stringified.Add(key.ToString() + " - " + dictio[key].ToString());
            }
            return String.Join(";", stringified);
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
