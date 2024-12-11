using OpenKh.Kh2;
using System;
using System.IO;
using System.Linq;

namespace OpenKh.Tests
{
    internal record BarEntrySource(
        string Source
    )
    {
        public MemoryStream GetMemoryStream()
        {
            var sources = Source.Split('\t');

            return sources.Skip(1)
                .Aggregate(
                    seed: new MemoryStream(File.ReadAllBytes(sources.First())),
                    func: (lastStream, token) =>
                    {
                        if (token.StartsWith("#"))
                        {
                            var index = int.Parse(token.Substring(1));

                            var nextStream = new MemoryStream();
                            var bar = Bar.Read(lastStream);
                            bar[index].Stream.CopyTo(nextStream);
                            nextStream.Seek(0, SeekOrigin.Begin);
                            return nextStream;
                        }
                        else
                        {
                            throw new ArgumentException($"Token '{token}' must start with sharp");
                        }
                    }
                );
        }

        public string GetRelativePart()
        {
            var text = Source.Replace("\t", "\\");
            var pos = text.IndexOf("\\.\\");
            if (pos < 0)
            {
                return null;
            }
            else
            {
                return text.Substring(pos + 3);
            }
        }
    }
}
