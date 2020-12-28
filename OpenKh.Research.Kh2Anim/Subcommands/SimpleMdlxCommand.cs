using McMaster.Extensions.CommandLineUtils;
using OpenKh.Research.Kh2Anim.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace OpenKh.Research.Kh2Anim.Subcommands
{
    [HelpOption]
    [Command(Description = "Create mdlx having 2 bones")]
    class SimpleMdlxCommand
    {
        [Required]
        [Argument(0, Description = "Output file: model.mdlx")]
        public string OutputMdlx { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            File.WriteAllBytes(OutputMdlx, MdlxMaker.CreateMdlxHaving2Bones().ToArray());
            return 0;
        }
    }
}
