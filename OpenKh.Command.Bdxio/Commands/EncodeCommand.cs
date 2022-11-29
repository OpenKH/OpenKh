using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.Bdxio.Commands
{
    [HelpOption]
    [Command(Description = "encode bdx")]
    internal class EncodeCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Input text file")]
        public string? InputFile { get; set; }

        [Argument(1, Description = "Output bdx file")]
        public string? OutputFile { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("Encode");

            if (InputFile == null)
            {
                throw new NullReferenceException("InputFile must be set!");
            }

            var outFile = Path.GetFullPath(OutputFile ?? Path.GetFileName(Path.ChangeExtension(InputFile, ".bdx")));

            logger.Debug($"Saving to: {outFile}");

            var ascii = BdxAsciiModel.ParseText(File.ReadAllText(InputFile));
            var decoder = new BdxEncoder(
                header: new YamlDotNet.Serialization.DeserializerBuilder()
                    .Build()
                    .Deserialize<BdxHeader>(
                        ascii.Header ?? ""
                    ),
                script: ascii.GetLineNumberRetainedScriptBody(),
                scriptName: InputFile,
                loadScript: fileName => File.ReadAllText(
                    Path.Combine(
                        Path.GetDirectoryName(InputFile) ?? ".",
                        fileName
                    )
                )
            );
            File.WriteAllBytes(
                outFile,
                decoder.Content
            );
            return 0;
        }
    }
}
