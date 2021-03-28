using OpenKh.Kh2;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace OpenKh.Command.Layout
{
    [Command("OpenKh.Command.Layout")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(UnlayoutCommand),
        typeof(RelayoutCommand),
        typeof(UnsequenceCommand),
        typeof(ResequenceCommand))]
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

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private static string ChangeExtension(string fileName, string newExtension)
        {
            var path = Path.GetDirectoryName(fileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            return Path.Combine(path, $"{fileNameWithoutExt}.{newExtension}");
        }

        private static int Serialize<T>(
            string inputFileName,
            string outputFileName,
            Func<Stream, bool> isValid,
            Func<Stream, T> read)
        {
            using var inStream = File.OpenRead(inputFileName);
            if (!isValid(inStream))
                throw new InvalidFileException(inputFileName, typeof(T).Name.ToLower());
            inStream.Position = 0;

            if (string.IsNullOrEmpty(outputFileName))
                outputFileName = ChangeExtension(inputFileName, "yml");

            using var outStream = File.CreateText(outputFileName);
            new YamlDotNet.Serialization.Serializer().Serialize(outStream, read(inStream));

            return 0;
        }

        private static int Deserialize<T>(
            string inputFileName,
            string outputFileName,
            string extension,
            Action<T, Stream> write)
        {
            var item = new YamlDotNet.Serialization.Deserializer()
                .Deserialize<T>(File.ReadAllText(inputFileName));

            if (string.IsNullOrEmpty(outputFileName))
                outputFileName = ChangeExtension(inputFileName, extension);

            using var outStream = File.Create(outputFileName);
            write(item, outStream);

            return 0;
        }

        [Command("unlayout", Description = "Deserialize a Kingdom Hearts II layout file to YAML")]
        private class UnlayoutCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Kingdom Hearts II layout file")]
            public string Input { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output file", ShortName = "o", LongName = "output")]
            public string Output { get; set; }

            protected int OnExecute(CommandLineApplication app) =>
                Serialize(Input, Output, Kh2.Layout.IsValid, Kh2.Layout.Read);
        }

        [Command("relayout", Description = "Serialize a YAML to Kingdom Hearts II layout")]
        private class RelayoutCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "File name of YAML representatiof a layout")]
            public string Input { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output file", ShortName = "o", LongName = "output")]
            public string Output { get; set; }

            protected int OnExecute(CommandLineApplication app) =>
                Deserialize<Kh2.Layout>(Input, Output, "layout", (x, s) => x.Write(s));
        }

        [Command("unsequence", Description = "Deserialize a Kingdom Hearts II sequence file to YAML")]
        private class UnsequenceCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Kingdom Hearts II sequence file")]
            public string Input { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output file", ShortName = "o", LongName = "output")]
            public string Output { get; set; }

            protected int OnExecute(CommandLineApplication app) =>
                Serialize(Input, Output, Sequence.IsValid, Sequence.Read);
        }

        [Command("resequence", Description = "Serialize a YAML to Kingdom Hearts II sequence")]
        private class ResequenceCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "File name of YAML representatiof a sequence")]
            public string Input { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output file", ShortName = "o", LongName = "output")]
            public string Output { get; set; }

            protected int OnExecute(CommandLineApplication app) =>
                Deserialize<Sequence>(Input, Output, "sequence", (x, s) => x.Write(s));
        }
    }

    internal class InvalidFileException : Exception
    {
        public InvalidFileException(string fileName, string type) :
            base($"The file {fileName} is not recognized as type {type}.")
        {

        }
    }
}
