using System;
using System.Collections.Generic;
using System.Text;
using OpenKh.Kh2;
using System.IO;
using System.Linq;

namespace OpenKh.Command.DoctChanger.Utils
{
    public class DumpDoctUtil
    {
        private readonly Doct doct;
        private readonly TextWriter writer;

        public DumpDoctUtil(Doct doct, TextWriter writer)
        {
            this.doct = doct;
            this.writer = writer;

            if (doct.Entry1List.Any())
            {
                DumpEntry1(0, 0);
            }
        }

        private void DumpEntry1(int index, int indent)
        {
            if (index == -1)
            {
                return;
            }

            var entry = doct.Entry1List[index];
            writer.WriteLine($"{new string(' ', indent)}[Entry1] {nameof(entry.Unk)}={entry.Unk}; {nameof(entry.BoundingBox)}={entry.BoundingBox}");

            for (var idx = entry.Entry2Index; idx < entry.Entry2LastIndex; idx++)
            {
                DumpEntry2(idx, indent + 1);
            }

            DumpEntry1(entry.Child1, indent + 1);
            DumpEntry1(entry.Child2, indent + 1);
            DumpEntry1(entry.Child3, indent + 1);
            DumpEntry1(entry.Child4, indent + 1);
            DumpEntry1(entry.Child5, indent + 1);
            DumpEntry1(entry.Child6, indent + 1);
            DumpEntry1(entry.Child7, indent + 1);
            DumpEntry1(entry.Child8, indent + 1);
        }

        private void DumpEntry2(int index, int indent)
        {
            var entry = doct.Entry2List[index];
            writer.WriteLine($"{new string(' ', indent)}[Entry2] {nameof(entry.Flags)}={entry.Flags}; {nameof(entry.BoundingBox)}={entry.BoundingBox}");
        }
    }
}
