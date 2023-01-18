using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.AnbMaker.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "fbx file: fbx to skeleton dae")]
    internal class ExportArmatureCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "fbx input")]
        public string InputModel { get; set; }

        [Argument(1, Description = "dae output")]
        public string Output { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(InputModel, Assimp.PostProcessSteps.None);

            void Walk(Assimp.Node node, int depth)
            {
            }

            Walk(scene.RootNode, 0);

            return 0;
        }
    }
}
