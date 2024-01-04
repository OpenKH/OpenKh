using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using OpenKh.Command.MapGen.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OpenKh.Command.MapGenUtils
{
    [Command("OpenKh.Command.MapGenUtils")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(GenSchemaCommand))]
    internal class Program
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
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        [HelpOption]
        [Command(Description = "Generate schema for mapdef.yml")]
        private class GenSchemaCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Output dir")]
            public string? OutDir { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var schema = JsonSchema.FromType<MapGenConfig>();
                var schemaData = schema.ToJson();

                File.WriteAllText(
                    Path.Combine(OutDir ?? ".", "mapdef.schema.json"),
                    schemaData
                );

                return 0;
            }
        }
    }
}
