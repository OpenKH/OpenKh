using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using SharpDX.DirectWrite;
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

            BdxStream = new MemoryStream(BdxStreamOut);
            BdxStream.Position = 0;
            MdlxService.Instance.BdxFile = BdxStream;
        }
    }
}
