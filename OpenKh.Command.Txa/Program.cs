using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using OpenKh.Bbs;
using OpenKh.Tools.Common.Imaging;

namespace OpenKh.Command.Txa
{
    [Command("OpenKh.Command.Txa")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(UnmakeCommand))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private class UnmakeCommand
        {
            [Required]
            [Argument(0, "TXA File", "The txa file to extract from")]
            public string TxaFilePath { get; set; }

            [Required]
            [Argument(1, "Model File", "The PMO file to source palette data from")]
            public string ModelFilePath { get; set; }

            [Argument(2, "Output Folder")]
            public string OutputFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                if (string.IsNullOrWhiteSpace(OutputFolder))
                {
                    OutputFolder = Environment.CurrentDirectory;
                }

                string txaname = Path.GetFileNameWithoutExtension(TxaFilePath);

                using var pmoStream = File.OpenRead(ModelFilePath);
                Pmo pmo = Pmo.Read(pmoStream);
                using var txaStream = File.OpenRead(TxaFilePath);
                Bbs.Txa txa = Bbs.Txa.Read(txaStream, pmo);

                foreach (var group in txa.AnimGroups)
                {
                    foreach (var anim in group.Anims)
                    {
                        int idx = 0;
                        foreach (var frame in anim.Frames)
                        {
                            if (frame.Image != null)
                            {
                                var outpath = Path.Combine(OutputFolder, $"{txaname}-{group.Name}-{anim.Name}-{idx}.png");
                                using var outStream = File.OpenWrite(outpath);
                                Png.Write(outStream, frame.Image);
                            }
                            idx++;
                        }
                    }
                }

                return 0;
            }
        }
    }
}
