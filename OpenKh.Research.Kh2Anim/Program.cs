using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Common.Exceptions;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Research.Kh2Anim.Models;
using OpenKh.Research.Kh2Anim.Subcommands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace OpenKh.Research.Kh2Anim
{
    [Command("OpenKh.Research.Kh2Anim")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(BakeCommand),
        typeof(BakeryCommand),
        typeof(FryCommand),
        typeof(BurnCommand),
        typeof(SimpleMdlxCommand),
        typeof(SummaryMdlxsCommand)
    )]
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
                Console.Error.WriteLine(e.Message);
                return 3;
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
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
    }
}
