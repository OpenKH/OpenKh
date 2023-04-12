using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
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

                string txaName = Path.GetFileNameWithoutExtension(TxaFilePath);

                using var pmoStream = File.OpenRead(ModelFilePath);
                Pmo pmo = Pmo.Read(pmoStream);
                using var txaStream = File.OpenRead(TxaFilePath);
                Bbs.Txa txa = Bbs.Txa.Read(txaStream, pmo);

                Dictionary<string, Group> txaDef = new Dictionary<string, Group>();

                foreach (var group in txa.AnimGroups)
                {
                    Group groupDef = new Group
                    {
                        DestTex = group.DestTexName,
                        Height = group.DestHeight,
                        Width = group.DestWidth,
                    };

                    foreach (var anim in group.Anims)
                    {
                        Anim animDef = new Anim
                        {
                            LoopFrame = anim.UnkNum
                        };

                        int idx = 0;
                        foreach (var frame in anim.Frames)
                        {
                            var outName = string.Empty;
                            
                            if (frame.Image != null)
                            {
                                outName = $"{txaName}-{group.Name}-{anim.Name}-{idx}.png";
                                var outPath = Path.Combine(OutputFolder, outName);
                                using var outStream = File.OpenWrite(outPath);
                                Png.Write(outStream, frame.Image);
                                
                            }
                            
                            animDef.Frames.Add(new Frame
                            {
                                Source = outName,
                                FrameLo = frame.UnkNum1,
                                FrameHi = frame.UnkNum2
                            });
                            
                            if (idx == group.DefaultAnim)
                                groupDef.DefaultAnim = anim.Name;
                            
                            idx++;
                        }
                        
                        groupDef.Anims.Add(anim.Name, animDef);
                    }

                    txaDef.Add(group.Name, groupDef);
                }

                var defPath = Path.Combine(OutputFolder, $"{txaName}-def.json");
                File.WriteAllText(defPath, JsonSerializer.Serialize(txaDef, new JsonSerializerOptions() { WriteIndented = true }));

                return 0;
            }
        }
    }
}
