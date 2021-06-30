using Ionic.Zlib;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Egs;
using OpenKh.Kh1;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Command.IdxImg
{
    partial class Program
    {
        [Command("hed", Description = "Make operation on the Epic Games Store release of Kingdom Hearts"),
         Subcommand(typeof(ExtractCommand)),
         Subcommand(typeof(PatchCommand)),
         Subcommand(typeof(ListCommand))]
        public class EpicGamesAssets
        {
            protected int OnExecute(CommandLineApplication app)
            {
                app.ShowHelp();
                return 1;
            }

            public class ExtractCommand
            {
                [Required]
                [Argument(0, Description = "Kingdom Hearts HED input filter; you can specify 'kh2_first.hed', 'kh2_*' or even '*' to extract everything.")]
                public string InputHed { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                [Option(CommandOptionType.NoValue, Description = "Do not extract files that are already found in the destination directory", ShortName = "n")]
                public bool DoNotExtractAgain { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    var directory = Path.GetDirectoryName(InputHed);
                    var filter = Path.GetFileName(InputHed);

                    Directory
                        .GetFiles(string.IsNullOrEmpty(directory) ? "." : directory, filter)
                        .Where(x => x.EndsWith(".hed"))
                        .AsParallel()
                        .ForAll(inputHed => Egs.EgsTools.Extract(inputHed, OutputDir, DoNotExtractAgain));

                    return 0;
                }
            }

            [Command("patch", Description = "Replace one or multiple files in a PKG file.")]
            private class PatchCommand
            {
                [Required]
                [Argument(0, Description = "The PKG file that will be patched.")]
                public string PkgFile { get; set; }

                [Required]
                [Argument(1, Description = "Folder that contains the files to replace.")]
                public string InputFolder { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Path where the patched PKG will be dropped.", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    EgsTools.Patch(PkgFile, InputFolder, OutputDir);

                    return 0;
                }
            }

            [Command("list", Description = "List the content of a HED file ")]
            private class ListCommand
            {
                [Required]
                [Argument(0, Description = "Kingdom Hearts HED input file")]
                public string InputHed { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    EgsTools.List(InputHed);

                    return 0;
                }
            }
        }
    }
}
