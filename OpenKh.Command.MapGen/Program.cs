using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Common.Exceptions;
using OpenKh.Command.MapGen.Utils;
using NLog;

namespace OpenKh.Command.MapGen
{
    [Command("OpenKh.Command.MapGen")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (InvalidFileException e)
            {
                Console.WriteLine(e.Message);
                return 3;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
            }
        }

        [Required]
        [FileExists]
        [Argument(0, Description = "Input file: mapdef.yml or model.{fbx,dae}")]
        public string InputFile { get; set; }

        [Argument(1, Description = "Output map file")]
        public string OutputMap { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            try
            {
                new MapGenUtil().Run(InputFile, OutputMap);

                return 0;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
