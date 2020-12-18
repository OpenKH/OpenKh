using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Research.Kh2Anim.Subcommands
{
    [HelpOption]
    [Command(Description = "Summary bones count of them")]
    public class SummaryMdlxsCommand
    {
        [Required]
        [DirectoryExists]
        [Argument(0, Description = "Input directory: mdlx/mset files")]
        public string InputDir { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            Directory.GetFiles(InputDir, "*.mdlx")
                .ForEach(
                    mdlxFile =>
                    {
                        var barEntries = File.OpenRead(mdlxFile).Using(Bar.Read);
                        var modelEnt = barEntries.FirstOrDefault(it => it.Type == Bar.EntryType.Model);
                        if (modelEnt == null)
                        {
                            return;
                        }
                        var mdlx = Mdlx.Read(modelEnt.Stream);
                        if (mdlx.IsMap)
                        {
                            return;
                        }
                        if (mdlx.SubModels == null)
                        {
                            return;
                        }
                        var subModel = mdlx.SubModels.FirstOrDefault();
                        if (subModel == null)
                        {
                            return;
                        }

                        var msetFile = Path.ChangeExtension(mdlxFile, ".mset");
                        if (!File.Exists(msetFile))
                        {
                            return;
                        }

                        var msetBarEntries = File.OpenRead(msetFile).Using(Bar.Read);

                        string hitMset = null;
                        string hitMotionName = null;
                        int numHits = 0;
                        int maxIKType = 0;
                        int numT6 = 0;
                        int numT7 = 0;
                        int numT8 = 0;

                        msetBarEntries
                            .Where(it => it.Type == Bar.EntryType.Anb && it.Stream.Length >= 32)
                            .ForEach(
                                msetBarEnt =>
                                {
                                    var anbBarEntries = Bar.Read(msetBarEnt.Stream);
                                    anbBarEntries
                                        .Where(it => it.Type == Bar.EntryType.Motion)
                                        .ForEach(
                                            motionEntry =>
                                            {
                                                try
                                                {
                                                    var motion = Motion.Read(motionEntry.Stream);
                                                    if (motion.Interpolated != null)
                                                    {
                                                        numT6 = Math.Max(numT6, motion.Interpolated.Table6.Count);
                                                        numT7 = Math.Max(numT7, motion.Interpolated.Table7.Count);
                                                        numT8 = Math.Max(numT8, motion.Interpolated.Table8.Count);

                                                        motion.Interpolated.IKChains
                                                            .Where(it => it.Unk00 >= 4)
                                                            .ForEach(
                                                                it =>
                                                                {
                                                                    if (hitMset == null)
                                                                    {
                                                                        hitMset = msetFile;
                                                                        hitMotionName = msetBarEnt.Name;
                                                                    }
                                                                    maxIKType = Math.Max(maxIKType, it.Unk00);
                                                                    numHits++;
                                                                }
                                                            );
                                                    }
                                                }
                                                catch (ArgumentException)
                                                {
                                                    // ignore
                                                }
                                            }
                                        );
                                }
                            );

                        if (hitMset != null)
                        {
                            Console.WriteLine($"{mdlxFile},{subModel.BoneCount},{maxIKType},{numT6},{numT7},{numT8},{numHits},{hitMset},{hitMotionName}");
                        }
                    }
                );

            return 0;
        }

        private IEnumerable<string> Match(string msetFile, string[] mdlxFiles)
        {
            var prefix = Path.GetFileNameWithoutExtension(msetFile).ToLowerInvariant();
            foreach (var mdlx in mdlxFiles)
            {
                var target = Path.GetFileNameWithoutExtension(mdlx).ToLowerInvariant();
                if (target.StartsWith(prefix))
                {
                    yield return mdlx;
                }
            }
        }
    }
}
