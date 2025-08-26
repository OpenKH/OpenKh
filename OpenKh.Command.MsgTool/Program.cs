using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace OpenKh.Command.MsgTool
{
    [Command("OpenKh.Command.MsgTool")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        private static readonly Dictionary<string, IMessageEncoder> Encoders = new Dictionary<string, IMessageEncoder>()
        {
            ["eusys"] = Kh2.Messages.Encoders.InternationalSystem,
            ["euevt"] = Kh2.Messages.Encoders.InternationalSystem,
            ["jpsys"] = Kh2.Messages.Encoders.JapaneseSystem,
            ["jpevt"] = Kh2.Messages.Encoders.JapaneseEvent,
        };

        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Option(ShortName = "i", LongName = "input", Description = "MSG file (almost all the files inside msg/{language}/ are considered MSGs)")]
        public string Input { get; }

        [Required]
        [McMaster.Extensions.CommandLineUtils.AllowedValues("eusys", "euevt", "jpsys", "jpevt")]
        [Option(ShortName = "e", LongName = "encoder", Description = "Specify which encoder should be used")]
        public string Encoder { get; }

        private void OnExecute()
        {
            var outputFileName = Input.Replace(".bar", ".xml", StringComparison.InvariantCultureIgnoreCase);

            using (var stream = File.OpenRead(Input))
            {
                var barEntries = Bar.Read(stream);
                foreach (Bar.Entry barEntry in barEntries)
                {
                    if (barEntry.Type == Bar.EntryType.List)
                    {
                        barEntry.Stream.Position = 0;
                        ConvertMsgToXml(barEntry.Stream, outputFileName, Encoders[Encoder]);

                        break;
                    }
                }
            }
        }

        private void ConvertMsgToXml(Stream inStream, string fileName, IMessageEncoder encoder)
        {
            using (var outStream = File.Create(fileName))
                ConvertMsgToXml(inStream, outStream, encoder);
        }

        private void ConvertMsgToXml(Stream inStream, Stream outStream, IMessageEncoder encoder)
        {
            var root = MsgSerializer.SerializeXEntries(Msg.Read(inStream), encoder, true);
            var document = new XDocument(root);
            document.Save(outStream);
        }
    }
}
