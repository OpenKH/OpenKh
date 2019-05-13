using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Xml.Linq;

namespace OpenKh.Command.MsgTool
{
    class Program
    {
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

        [Option(ShortName = "i", LongName = "input", Description = "IDX input")]
        public string Input { get; }

        private void OnExecute()
        {
            var outputFileName = Input.Replace(".bar", ".xml", StringComparison.InvariantCultureIgnoreCase);

            using (var stream = File.OpenRead(Input))
            {
                var barEntries = Bar.Open(stream);
                foreach (Bar.Entry barEntry in barEntries)
                {
                    if (barEntry.Type == Bar.EntryType.Msg)
                    {
                        barEntry.Stream.Position = 0;
                        ConvertMsgToXml(barEntry.Stream, outputFileName);

                        break;
                    }
                }
            }
        }

        private void ConvertMsgToXml(Stream inStream, string fileName)
        {
            using (var outStream = File.Create(fileName))
                ConvertMsgToXml(inStream, outStream);
        }

        private void ConvertMsgToXml(Stream inStream, Stream outStream)
        {
            var root = MsgSerializer.SerializeXEntries(Msg.Open(inStream), true);
            var document = new XDocument(root);
            document.Save(outStream);
        }
    }
}
