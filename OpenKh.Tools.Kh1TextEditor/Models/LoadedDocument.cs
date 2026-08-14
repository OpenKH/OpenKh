using OpenKh.Kh1;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh1TextEditor.Models
{
    internal sealed class LoadedDocument
    {
        private readonly Action<Stream> _write;

        private LoadedDocument(
            string fileName,
            string relativePath,
            string category,
            string format,
            Action<Stream> write)
        {
            FileName = fileName;
            RelativePath = relativePath;
            Category = category;
            Format = format;
            _write = write;
        }

        public string FileName { get; }
        public string RelativePath { get; }
        public string Category { get; }
        public string Format { get; }
        public List<TextOccurrence> Entries { get; } = new();

        public static LoadedDocument Read(string fileName, string rootPath)
        {
            var extension = Path.GetExtension(fileName);
            var relativePath = Directory.Exists(rootPath)
                ? Path.GetRelativePath(rootPath, fileName)
                : Path.GetFileName(fileName);

            if (string.Equals(extension, ".binl", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = File.OpenRead(fileName);
                if (Kh1Binl.IsValid(stream))
                {
                    var binl = Kh1Binl.Read(stream);
                    var document = new LoadedDocument(fileName, relativePath, "BINL", "BINL", binl.Write);
                    foreach (var item in binl.Entries)
                    {
                        var entry = item;
                        document.Entries.Add(new TextOccurrence(
                            document,
                            entry.Index,
                            entry.Offset,
                            () => entry.Text,
                            value => entry.Text = value));
                    }
                    return document;
                }

                if (Kh1MessageV361.IsValid(stream))
                {
                    var message = Kh1MessageV361.Read(stream);
                    var document = new LoadedDocument(fileName, relativePath, "BINL", "BINL-v361", message.Write);
                    foreach (var item in message.Entries)
                    {
                        var entry = item;
                        document.Entries.Add(new TextOccurrence(
                            document,
                            entry.Index,
                            entry.Offset,
                            () => entry.Text,
                            value => entry.Text = value));
                    }
                    return document;
                }

                return null;
            }

            if (string.Equals(extension, ".kmb", StringComparison.OrdinalIgnoreCase))
            {
                var kmb = Kh1Kmb.Read(fileName);
                var document = new LoadedDocument(fileName, relativePath, "KMB", "KMB", kmb.Write);
                foreach (var item in kmb.Entries)
                {
                    var entry = item;
                    document.Entries.Add(new TextOccurrence(
                        document,
                        entry.Index,
                        entry.Offset,
                        () => entry.Text,
                        value => entry.Text = value));
                }
                return document;
            }

            if (string.Equals(extension, ".bin", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsTextBinFile(fileName))
                    return null;

                var textBin = Kh1TextBin.Read(fileName);
                var document = new LoadedDocument(fileName, relativePath, "BIN", "BIN", textBin.Write);
                foreach (var item in textBin.Entries)
                {
                    var entry = item;
                    document.Entries.Add(new TextOccurrence(
                        document,
                        entry.Index,
                        entry.Offset,
                        () => entry.Text,
                        value => entry.Text = value));
                }
                return document;
            }

            if (string.Equals(extension, ".ev", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".evdl", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = File.OpenRead(fileName);
                if (!Kh1EventMessage.IsValid(stream))
                    return null;
                var eventMessage = Kh1EventMessage.Read(stream);
                if (eventMessage.Entries.Count == 0)
                    return null;

                var category = extension.Equals(".ev", StringComparison.OrdinalIgnoreCase) ? "EV" : "EVDL";
                var document = new LoadedDocument(fileName, relativePath, category, category, eventMessage.Write);
                foreach (var item in eventMessage.Entries)
                {
                    var entry = item;
                    document.Entries.Add(new TextOccurrence(
                        document,
                        entry.Index,
                        entry.Offset,
                        () => entry.Text,
                        value => entry.Text = value));
                }
                return document;
            }

            throw new InvalidDataException("The file is not a supported KH1 remastered text file.");
        }

        public static bool IsTextBinFile(string fileName)
        {
            var name = Path.GetFileName(fileName);
            return name.Contains("AbilityHelp", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("AbilityName", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("ItemHelp", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("Word.bin", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("SASAMSG.BIN", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("ChallengeMsg", StringComparison.OrdinalIgnoreCase);
        }

        public byte[] BuildFile()
        {
            using var output = new MemoryStream();
            _write(output);
            return output.ToArray();
        }

        public static void WriteFile(string fileName, byte[] data)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            Directory.CreateDirectory(directory);
            var temporaryFile = Path.Combine(
                directory,
                $".{Path.GetFileName(fileName)}.{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllBytes(temporaryFile, data);
                File.Move(temporaryFile, fileName, true);
            }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }
    }
}
