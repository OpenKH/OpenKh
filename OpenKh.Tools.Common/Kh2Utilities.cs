using OpenKh.Kh2;
using OpenKh.Common;
using OpenKh.Common.Exceptions;
using System.Collections.Generic;
using System.IO;
using Xe.Tools.Wpf.Dialogs;
using System.Linq;
using System;

namespace OpenKh.Tools.Common
{
    public class Kh2Utilities
    {
        private static readonly List<FileDialogFilter> IdxFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("KH2.IDX", "idx").AddAllFiles();

        private static readonly List<FileDialogFilter> MsgFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("sys.bar", "bar", "msg", "bin").AddAllFiles();

        public static bool? OpenMsgFromIdxDialog(Action<List<Msg.Entry>> onSuccess) =>
            FileDialog.OnOpen(fileName => onSuccess(ReadMsgFromIdx(fileName)), IdxFilter);

        public static bool? OpenMsgFromBarDialog(Action<List<Msg.Entry>> onSuccess) =>
            FileDialog.OnOpen(fileName => onSuccess(File.OpenRead(fileName).Using(ReadMsg)), MsgFilter);

        public static List<Msg.Entry> ReadMsgFromIdx(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Idx.IsValid(stream))
                throw new InvalidFileException(typeof(Idx));

            var imgFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.img";
            var imgFilePath = Path.Combine(Path.GetDirectoryName(fileName), imgFileName);

            if (!File.Exists(imgFilePath))
                throw new FileNotFoundException($"Unable to find {imgFileName} in the same directory of the IDX.");

            return File.OpenRead(imgFilePath).Using(imgStream => ReadMsgFromIdx(stream, imgStream));
        });

        public static List<Msg.Entry> ReadMsgFromIdx(Stream idxStream, Stream imgStream)
        {
            var img = new Img(imgStream, Idx.Read(idxStream), false);
            foreach (var language in Constants.Regions)
            {
                var stream = img.FileOpen($"msg/{language}/sys.bar");
                if (stream != null)
                    return ReadMsgFromBar(stream);
            }

            throw new FileNotFoundException($"Unable to find a 'sys.bar' between the supported languages.");
        }

        public static List<Msg.Entry> ReadMsg(Stream stream)
        {
            if (Bar.IsValid(stream))
                return ReadMsgFromBar(stream);
            else if (Msg.IsValid(stream))
                return ReadMsgFromRawMsg(stream);

            throw new InvalidFileException<Msg>();
        }

        public static List<Msg.Entry> ReadMsgFromBar(Stream stream)
        {
            if (!Bar.IsValid(stream))
                throw new InvalidFileException(typeof(Bar));

            var entries = Bar.Read(stream);
            var entry = entries
                .FirstOrDefault(x => x.Type == Bar.EntryType.List && x.Name == "sys");

            if (entry == null)
                throw new InvalidFileException<Msg>();

            return ReadMsgFromRawMsg(entry.Stream);
        }

        public static List<Msg.Entry> ReadMsgFromRawMsg(Stream stream)
        {
            if (!Msg.IsValid(stream))
                throw new InvalidFileException<Msg>();

            return Msg.Read(stream);
        }
    }
}
