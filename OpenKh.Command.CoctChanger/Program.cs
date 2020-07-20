using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace OpenKh.Command.CoctChanger
{
    [Command("OpenKh.Command.CoctChanger")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(CreateDummyCoctCommand)/*, typeof(ReadCoctCommand), typeof(UseThisCoctCommand)*/)]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return 1;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }


        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [HelpOption]
        [Command(Description = "coct file: create single room")]
        private class CreateDummyCoctCommand
        {
            [Required]
            [Argument(0, Description = "Output coct")]
            public string CoctOut { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "bbox: minX,Y,Z,maxX,Y,Z (default: -18000,0,-18000,18000,18000,18000)", ShortName = "b", LongName = "bbox")]
            public string BBox { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var coct = new Coct();

                var bbox = BBox.Split(',')
                    .Select(one => float.Parse(one))
                    .ToArray();

                throw new NotImplementedException();//TODO

                var buff = new MemoryStream();
                coct.Write(buff);
                buff.Position = 0;
                File.WriteAllBytes(CoctOut, buff.ToArray());

                return 0;
            }
        }
    }
}
