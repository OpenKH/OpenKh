using McMaster.Extensions.CommandLineUtils;
using NLog;
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
    [Command(Description = "decode bdx")]
    internal class DecodeCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Input bdx file")]
        public string? InputFile { get; set; }

        [Argument(1, Description = "Output text file")]
        public string? OutputFile { get; set; }

        [Option(LongName = "labels", ShortName = "l", Description = "Additional addresses to decode as code")]
        public int[]? Labels { get; set; }

        [Option(LongName = "code-revealer", ShortName = "r", Description = "Decode unrevealed as code")]
        public bool CodeRevealer { get; set; }

        [Option(LongName = "code-revealer-labeling", ShortName = "b", Description = "Mark unrevealed code as label, and then replace pushImm arg")]
        public bool CodeRevealerLabeling { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("Decode");

            if (InputFile == null)
            {
                throw new NullReferenceException("InputFile must be set!");
            }

            var outFile = Path.GetFullPath(OutputFile ?? Path.GetFileName(Path.ChangeExtension(InputFile, ".bdscript")));

            logger.Debug($"Saving to: {outFile}");

            var bdxStream = new MemoryStream(File.ReadAllBytes(InputFile));
            var decoder = new BdxDecoder(bdxStream, Labels, CodeRevealer, CodeRevealerLabeling);
            File.WriteAllText(
                outFile,
                BdxDecoder.TextFormatter.Format(decoder)
            );
            return 0;
        }
    }
}
