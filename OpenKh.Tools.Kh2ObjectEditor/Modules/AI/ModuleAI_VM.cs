using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.AI
{
    public class ModuleAI_VM : NotifyPropertyChangedBase
    {
        public Stream BdxStream { get; set; }
        public byte[] BdxStreamOut { get; set; } // Write test
        private string _BdxDecoded { get; set; }
        public string BdxDecoded
        {
            get { return _BdxDecoded; }
            set
            {
                _BdxDecoded = value;
                OnPropertyChanged("BdxDecoded");
            }
        }

        public ModuleAI_VM()
        {
            if (MdlxService.Instance.BdxFile != null)
            {
                BdxStream = MdlxService.Instance.BdxFile;
                read();
            }
        }

        public void clipboardCopy()
        {
            Clipboard.SetText(BdxDecoded);
        }
        public void clipboardPaste()
        {
            BdxDecoded = Clipboard.GetText();
        }

        public void read()
        {
            BdxStream.Position = 0;
            //bdxStream = new MemoryStream(File.ReadAllBytes(InputFile));
            BdxDecoder decoder = new BdxDecoder(BdxStream, codeRevealer: true, codeRevealerLabeling: true);
            BdxDecoded = BdxDecoder.TextFormatter.Format(decoder);
            BdxStream.Position = 0;
        }

        public void write()
        {
            var ascii = BdxAsciiModel.ParseText(BdxDecoded);

            
            try
            {
                var decoder = new BdxEncoder(
                    header: new YamlDotNet.Serialization.DeserializerBuilder()
                        .Build()
                        .Deserialize<BdxHeader>(
                            ascii.Header ?? ""
                        ),
                    script: ascii.GetLineNumberRetainedScriptBody(),
                    scriptName: null,
                    loadScript: null
                );
                BdxStreamOut = decoder.Content;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BdxStream = new MemoryStream(BdxStreamOut);
            BdxStream.Position = 0;
            MdlxService.Instance.BdxFile = BdxStream;
        }

        // Tests the AI ingame.
        // IMPORTANT: AI must be of the same length, be careful because there's no control of this.
        public void TestAiIngame()
        {
            string filename = Path.GetFileName(MdlxService.Instance.MdlxPath);

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
            foreach(Bar.Entry entry in MdlxService.Instance.MdlxBar)
            {
                if(entry.Type == Bar.EntryType.Bdx)
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

            long aiFileAddress = fileAddress + entryOffset;

            MemoryStream aiStream = new MemoryStream();
            MdlxService.Instance.BdxFile.Position = 0;
            MdlxService.Instance.BdxFile.CopyTo(aiStream);
            MdlxService.Instance.BdxFile.Position = 0;
            aiStream.Position = 0;

            byte[] streamBytes = aiStream.ToArray();
            MemoryAccess.writeMemory(ProcessService.KH2Process, aiFileAddress, streamBytes, true);
        }
    }
}
