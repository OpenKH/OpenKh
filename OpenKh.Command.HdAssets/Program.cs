using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common.Archives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenKh.Command.HdAssets
{
    [Command("OpenKh.Command.HdAsset")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(ExtractCommand),
        typeof(StripCommand))]
    class Program
    {
        private const string DefaultPrefix = "_ASSET_";

        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
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

        [Command(Description = "Extract ReMIX assets into an asset folder.")]
        private class ExtractCommand
        {
            [Required]
            [Argument(0, Description = "Input file or directory of 1.5/2.5 ReMIX assets")]
            public string Input { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Prefix used as extraction directory. By default it is '_ASSET_'", ShortName = "p", LongName = "prefix")]
            public string Prefix { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Execute the command recursively for all the sub-folders", ShortName = "r", LongName = "recursive")]
            public bool Recursive { get; set; }

            protected void OnExecute(CommandLineApplication app)
            {
                var prefix = Prefix ?? DefaultPrefix;

                foreach (var filePath in GetFiles(Input, Recursive, prefix))
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        HdAsset asset;

                        try
                        {
                            // Avoid to crash on files that are not a ReMIX asset
                            asset = HdAsset.Read(stream);
                        }
                        catch
                        {
                            // Not a ReMIX asset
                            continue;
                        }

                        var directoryName = Path.GetDirectoryName(filePath);
                        Console.Write(filePath);

                        foreach (var entry in asset.Entries)
                        {
                            var outDir = Path.GetFileNameWithoutExtension(filePath);
                            var outFileName = Path.Combine(directoryName, $"{prefix}{outDir}", entry.Name);
                            Directory.CreateDirectory(Path.GetDirectoryName(outFileName));

                            using (var outStream = File.Create(outFileName))
                            {
                                entry.Stream.Position = 0;
                                entry.Stream.CopyTo(outStream);
                            }
                        }
                    }
                }
            }
        }

        [Command(Description = "Modify ReMIX assets by unwrapping them from their HD textures, effectly making them compatible with the original games.")]
        private class StripCommand
        {
            [Required]
            [Argument(0, Description = "Input file or directory of 1.5/2.5 ReMIX assets")]
            public string Input { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Execute the command recursively for all the sub-folders", ShortName = "r", LongName = "recursive")]
            public bool Recursive { get; set; }

            protected void OnExecute(CommandLineApplication app)
            {
                foreach (var filePath in GetFiles(Input, Recursive, DefaultPrefix))
                {
                    HdAsset asset;
                    using (var stream = File.OpenRead(filePath))
                    {

                        try
                        {
                            // Avoid to crash on files that are not a ReMIX asset
                            asset = HdAsset.Read(stream);
                        }
                        catch
                        {
                            // Not a ReMIX asset
                            continue;
                        }
                    }

                    using (var stream = File.Create(filePath))
                        asset.Stream.CopyTo(stream);
                }
            }
        }

        private static string[] GetFiles(string input, bool recursive, string excludePrefix)
        {
            if (File.Exists(input))
                return new string[] { input };
            else if (Directory.Exists(input))
                return Directory.GetFiles(input, "*",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            else
                throw new FileNotFoundException(null, input);
        }
    }
}
