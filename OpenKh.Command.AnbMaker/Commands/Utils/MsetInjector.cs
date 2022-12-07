using NLog;
using OpenKh.Command.AnbMaker.Commands.Interfaces;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Commands.Utils
{
    internal class MsetInjector
    {
        internal void InjectMotionTo(IMsetInjector arg, byte[] motion)
        {
            if (string.IsNullOrEmpty(arg.MsetFile))
            {
                return;
            }

            var logger = LogManager.GetLogger("MsetInjector");

            logger.Debug($"Going to inject new motion data into existing mset file");

            logger.Debug($"Loading {arg.MsetFile}");

            var (msetBar, msetLen) = File.OpenRead(arg.MsetFile).Using(stream => (Bar.Read(stream), stream.Length));

            logger.Debug($"{msetBar.Count} entries in mset");

            logger.Debug($"Locating bar entry #{arg.MsetIndex}");

            var msetBarEntry = msetBar[arg.MsetIndex];
            if (msetBarEntry.Type != Bar.EntryType.Anb)
            {
                throw new Exception($"#{arg.MsetIndex}: {msetBarEntry.Type} must be EntryType.Anb!");
            }

            logger.Debug($"Loading anb");

            var anbBar = Bar.Read(msetBarEntry.Stream);

            logger.Debug($"{anbBar.Count} entries in anb");

            logger.Debug($"Locating bar entry having EntryType.Motion");

            var anbBarEntry = anbBar.Single(it => it.Type == Bar.EntryType.Motion);

            logger.Debug($"Found. Motion data: fromSize {anbBarEntry.Stream.Length:#,##0} newSize {motion.Length:#,##0}");

            anbBarEntry.Stream = new MemoryStream(motion).FromBegin();

            logger.Debug($"Packing new anb");

            var anbNewBarStream = new MemoryStream();
            Bar.Write(anbNewBarStream, anbBar);

            msetBarEntry.Stream = anbNewBarStream.FromBegin();

            logger.Debug($"Packing new mset");

            var msetBarNewStream = new MemoryStream();
            Bar.Write(msetBarNewStream, msetBar);

            logger.Debug($"Writing to mset");

            logger.Debug($"Mset file: fromSize {msetLen:#,##0} newSize {msetBarNewStream.Length:#,##0}");

            File.WriteAllBytes(
                arg.MsetFile,
                msetBarNewStream.ToArray()
            );

            logger.Debug($"Done");
        }
    }
}
