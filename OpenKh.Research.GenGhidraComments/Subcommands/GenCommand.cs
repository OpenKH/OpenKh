using CsvHelper;
using Kaitai;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Research.GenGhidraComments.Extensions;
using OpenKh.Research.GenGhidraComments.Ksy;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenKh.Research.GenGhidraComments.Subcommands
{
    [HelpOption]
    [Command(Description = "Generate comments data")]
    class GenCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "d", Description = "Dir having pcsx2.log and readmemfrm.txt")]
        public string InputDir { get; set; } = @"H:\Proj\khkh_xldM\MEMO\expSim\1220";

        [Option(CommandOptionType.SingleValue, Description = "KH2 game dir having extracted contents")]
        public string KH2Dir { get; set; } = @"H:\KH2fm.OpenKH";

        [Option(CommandOptionType.SingleValue)]
        public string IncludeExtensions { get; set; } = ".fm";

        protected int OnExecute(CommandLineApplication app)
        {
            var csvItems = new List<CsvItemRef>();

            var selectIndexer = new Regex("\\[[0-9]+\\]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            // From csv output of: 010 Editor binary template result
            Directory.GetFiles(InputDir, "*.csv")
                .ForEach(
                    csv =>
                    {
                        csvItems.AddRange(
                            new CsvReader(new StringReader(File.ReadAllText(csv)), CultureInfo.InvariantCulture)
                                .GetRecords<CsvItem>()
                                .Select(
                                    item =>
                                    {
                                        var ofs = Convert.ToInt32(Path.GetFileNameWithoutExtension(csv), 16);
                                        return new CsvItemRef
                                        {
                                            adr = ofs + Convert.ToInt32(item.Start.TrimEnd('h'), 16),
                                            size = Convert.ToInt32(item.Start.TrimEnd('h'), 16),
                                            item = item,
                                            shortenComment = //selectIndexer.Replace(item.Name, "[]")   
                                                item.Name
                                                .Split(' ').Last(),
                                        };
                                    }
                                )
                        );
                    }
                );

            csvItems.Sort();

            // # py.S_IEXPA: 003b8bc0  field2d/jp/eh0field.2dd 
            var loadedList = File.ReadAllLines(Path.Combine(InputDir, "pcsx2.log"))
                .Select(line => Regex.Match(line, "^# py\\.S_IEXPA:\\s+(?<adr>[0-9a-fA-F]{8})\\s+(?<file>[\\S]+)"))
                .Where(match => match.Success && IncludeExtensions.Contains(Path.GetExtension(match.Groups["file"].Value), StringComparison.OrdinalIgnoreCase))
                .Select(
                    match => new LoadedFile
                    {
                        adr = Convert.ToInt32(match.Groups["adr"].Value, 16),
                        file = match.Groups["file"].Value,
                        fullPath = Path.Combine(KH2Dir, match.Groups["file"].Value),
                    }
                        .Also(
                            it => it.size = new FileInfo(it.fullPath)
                                .Let(info => info.Exists ? Convert.ToInt32(info.Length) : 0)
                        )
                )
                .ToArray();

            var ofs2Name = new SortedDictionary<int, string>();

            loadedList
                .ForEach(
                    loaded =>
                    {
                        //if (loaded.file == "obj/P_EH000_MEMO.mset")
                        if (loaded.file.EndsWith(".mset") || loaded.file.EndsWith(".mdlx") || loaded.file.EndsWith(".map")
                            || loaded.file.EndsWith(".ard") || loaded.file.EndsWith(".fm"))
                        {
                            var tracer = new Tracer(ofs2Name, loaded.adr, $"{Path.GetExtension(loaded.file).TrimStart('.')}:");
                            var model = new Kh2Bar(new KaitaiStream(File.ReadAllBytes(loaded.fullPath)), tracer: tracer);
                            File.WriteAllText(Path.GetFileNameWithoutExtension(loaded.file) + ".txt", tracer.writer.ToString());
                        }
                    }
                );

            var readMemFrmPattern = new Regex("^(?<pc>[0-9A-F]{8})\\s?(?<target>[0-9A-F]{8})", RegexOptions.Compiled);

            var readFromMemList = File.ReadAllLines(Path.Combine(InputDir, "readmemfrm.txt"))
                .Select(line => readMemFrmPattern.Match(line))
                .Where(match => match.Success)
                .Select(
                    match => new ReadMem
                    {
                        pc = Convert.ToInt32(match.Groups["pc"].Value, 16),
                        target = Convert.ToInt32(match.Groups["target"].Value, 16),
                    }
                )
                .ToArray();

            var comments = new List<Comment>();

            foreach (var group in readFromMemList.GroupBy(it => it.pc))
            {
                foreach (var readMem in group)
                {
                    foreach (var hit in csvItems
                        .Where(it => it.Contains(readMem.target))
                        .TakeLast(1)
                    )
                    {
                        comments.Add(
                            new Comment
                            {
                                pc = group.Key,
                                comment = hit.shortenComment,
                            }
                        );
                    }

                    Func<int, string> helper = target => DescribeFromNearestFile(loadedList, target);
                    //Func<int, string> helper = target => DescribeFromNearestItem(ofs2Name, target);

                    if (LookupJustAndAlsoNearest(ofs2Name, readMem.target, helper, out string hitBody))
                    {
                        // "mset:.Kh2Bar.Files.FileEntry.File.Kh2Bar.Files.FileEntry.File.Kh2Motion.Motion.Interpolated.IkHelpers.IkHelperTable.RotateX"
                        var hitPairs = (hitBody.Contains(":") ? hitBody : ":" + hitBody).Split(':');
                        var hitFileExt = hitPairs[0]; // "mset"
                        var hitMemberPath = hitPairs[1]; // ".Kh2Bar.Files.FileEntry.File.Kh2Bar.Files.FileEntry.File.Kh2Motion.Motion.Interpolated.IkHelpers.IkHelperTable.RotateX"

                        comments.Add(
                            new Comment
                            {
                                pc = group.Key,
                                comment = $"{hitFileExt}:{string.Join(".", hitMemberPath.Split('.').TakeLast(2))}",
                            }
                        );
                    }

#if false
                    foreach (var hit in loadedList
                        .Select(it => it.TryTest(readMem.target))
                        .Where(it => it != null)
                        .Take(1)
                    )
                    {
                        comments.Add(
                            new Comment
                            {
                                pc = group.Key,
                                comment = hit.comment,
                            }
                        );
                    }
#endif
                }
            }

            foreach (var group in comments
                .OrderBy(it => it.pc)
                .GroupBy(it => it.pc)
            )
            {
                var filteredComments = group.Select(it => it.comment).Distinct().ToArray();
                var cnt = filteredComments.Count();
                if (cnt >= 4)
                {
                    Console.WriteLine($"{group.Key:X8}|{cnt:#,##0} usages. |{string.Join("|", filteredComments)}");
                }
                else
                {
                    Console.WriteLine($"{group.Key:X8}|{string.Join(", ", filteredComments).CutBy(80)}");
                }
            }

            return 0;
        }

        private bool LookupJustAndAlsoNearest(SortedDictionary<int, string> ofs2Name, int target, Func<int, string> helper, out string hitBody)
        {
            if (ofs2Name.TryGetValue(target, out hitBody))
            {
                return true;
            }
            hitBody = helper(target);
            return true;
        }

        private string DescribeFromNearestItem(SortedDictionary<int, string> ofs2Name, int target)
        {
            int lastOfs = 0;
            string lastName;
            if (ofs2Name.TryGetValue(target, out lastName))
            {
                return lastName;
            }
            foreach (var pair in ofs2Name)
            {
                var ofs = (int)target - (int)pair.Key;
                if (ofs < 0)
                {
                    return $"{lastName}+ {lastOfs} ";
                }
                lastOfs = ofs;
                lastName = pair.Value;
            }
            return "?";
        }

        private string DescribeFromNearestFile(LoadedFile[] loadedFiles, int target)
        {
            foreach (var file in loadedFiles)
            {
                var ofs = (uint)target - (uint)file.adr;
                if (ofs < (uint)file.size)
                {
                    return $"{Path.GetExtension(file.file).TrimStart('.')}:{file.file}+ {ofs} ";
                }
            }
            return "?";
        }

        class Comment
        {
            /// <summary>
            /// program counter, reading memory from.
            /// </summary>
            public int pc;

            /// <summary>
            /// "mset:IkHelperTable.RotateX"
            /// </summary>
            public string comment;
        }

        class ReadMem
        {
            public int pc;
            public int target;

            public override string ToString() => $"{pc:X8} {target:X8}";
        }

        class HitResult
        {
            public int target;
            public string comment;
        }

        class LoadedFile
        {
            /// <summary>
            /// 0x003b8bc0
            /// </summary>
            public int adr;

            /// <summary>
            /// field2d/jp/eh0field.2dd
            /// </summary>
            public string file;

            public string fullPath;

            public int size;

            public HitResult TryTest(int target)
            {
                var hit = adr <= target && target < adr + size;

                if (hit)
                {
                    return new HitResult
                    {
                        target = target,
                        comment = $"{Path.GetExtension(file)}",
                    };
                }

                return null;
            }

            public override string ToString() => $"{adr:X8} {size:#,##0} {file}";
        }

        class CsvItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Start { get; set; }
            public string Size { get; set; }
        }

        class CsvItemRef : IComparable<CsvItemRef>
        {
            public int adr;
            public int size;
            public string shortenComment;

            public bool Contains(int target) => adr <= target && target < adr + size;

            public int CompareTo([AllowNull] CsvItemRef other)
            {
                return adr.CompareTo(other.adr);
            }

            public CsvItem item;
        }
    }
}
