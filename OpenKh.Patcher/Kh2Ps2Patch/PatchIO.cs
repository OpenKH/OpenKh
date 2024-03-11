using OpenKh.Common;
using OpenKh.Patcher.Kh2Ps2Patch.BlockSystem;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    /// <summary>
    /// The patch formats are taken from https://github.com/GovanifY/KH2FM_Toolkit
    /// 
    /// KH2FM_Toolkit is programmed by GovanifY https://www.govanify.com https://www.twitter.com/GovanifY
    /// KH2FM_Toolkit Copyright (c) 2015 Gauvain "GovanifY" Roussel-Tarbouriech
    /// </summary>
    public class PatchIO
    {
        public bool VerifySignature(Memory<byte> memory)
        {
            var magic = BinaryPrimitives.ReadUInt32LittleEndian(memory.Span);

            if (magic != 1345472587u && magic != 1362249803u)
            {
                return false;
            }

            return true;
        }

        public PatchHeader Read(Memory<byte> memory)
        {
            var cursor = memory;

            var header = new PatchHeader();

            var magic = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            if (magic != 1345472587u && magic != 1362249803u)
            {
                throw new InvalidDataException("The signature must be either KH2P or KH2Q");
            }

            var tableOffset = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            var firstEntryOffset = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            header.Revision = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            header.Author = ReadCString(ref cursor);

            cursor = memory.Slice(Convert.ToInt32(tableOffset));

            var offsetChangeLog = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            var offsetCredits = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            var offsetOtherInfo = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            header.ChangeLogs = ReadMultiTexts(memory.Slice(Convert.ToInt32(tableOffset + offsetChangeLog)));
            header.Credits = ReadMultiTexts(memory.Slice(Convert.ToInt32(tableOffset + offsetCredits)));
            header.OtherInformation = ReadSingleText(memory.Slice(Convert.ToInt32(tableOffset + offsetOtherInfo)));

            {
                cursor = memory.Slice(Convert.ToInt32(firstEntryOffset));

                var countEntries = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                cursor = cursor.Slice(4);

                for (uint index = 0; index < countEntries; index++)
                {
                    var hash = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                    cursor = cursor.Slice(4);

                    var fileOffset = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                    cursor = cursor.Slice(4);

                    var compressedSize = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                    cursor = cursor.Slice(4);

                    var uncompressedSize = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                    cursor = cursor.Slice(4);

                    var parent = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                    cursor = cursor.Slice(4);

                    var relink = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
                    cursor = cursor.Slice(4);

                    var isCompressed = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span) != 0;
                    cursor = cursor.Slice(4);

                    var isNew = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span) == 1; //Custom
                    cursor = cursor.Slice(4);

                    header.PatchEntries.Add(
                        new PatchEntry
                        {
                            Hash = hash,
                            CompressedSize = compressedSize,
                            UncompressedSize = uncompressedSize,
                            Parent = parent,
                            Relink = relink,
                            IsCompressed = isCompressed,
                            IsCustomFile = isNew,
                            RawContent = memory.Slice(Convert.ToInt32(fileOffset), Convert.ToInt32(compressedSize)),
                        }
                    );

                    cursor = cursor.Slice(60); // it is said that this is padding
                }
            }

            return header;
        }

        private string ReadSingleText(Memory<byte> cursor)
        {
            return ReadCString(ref cursor);
        }

        private string[] ReadMultiTexts(Memory<byte> cursor)
        {
            var count = BinaryPrimitives.ReadUInt32LittleEndian(cursor.Span);
            cursor = cursor.Slice(4);

            if (count != 0)
            {
                cursor = cursor.Slice(Convert.ToInt32(count * 4));

                return Enumerable.Range(0, Convert.ToInt32(count))
                    .Select(_ => ReadCString(ref cursor))
                    .ToArray();
            }
            else
            {
                return new string[0];
            }
        }

        private string ReadCString(ref Memory<byte> cursor)
        {
            var span = cursor.Span;

            for (int offset = 0; ; offset++)
            {
                if (span.Length <= offset)
                {
                    cursor = cursor.Slice(offset);
                    return Encoding.Latin1.GetString(span.Slice(0, offset));
                }
                if (span[offset] == 0)
                {
                    cursor = cursor.Slice(offset + 1);
                    return Encoding.Latin1.GetString(span.Slice(0, offset));
                }
            }
        }

        public byte[] Write(PatchHeader header)
        {
            var fixedDataBlocks = new List<ContentBlock>();

            var headerBlk = new LengthBlock(16, "header");
            fixedDataBlocks.Add(
                new ContentBlock(Encoding.Latin1.GetBytes(header.Author + "\0"), "author")
                    .AppendTo(headerBlk)
            );

            var stringTableBlk = new LengthBlock(12, "stringTab");
            headerBlk.Children.Add(stringTableBlk);

            var changeLogsBlk = new LengthBlock(4 + 4 * header.ChangeLogs.Length, "changeLogs");
            foreach (var one in header.ChangeLogs)
            {
                fixedDataBlocks.Add(new ContentBlock(Encoding.Latin1.GetBytes(one + "\0")).AppendTo(changeLogsBlk));
            }
            stringTableBlk.Children.Add(changeLogsBlk);

            var creditsBlk = new LengthBlock(4 + 4 * header.Credits.Length, "credits");
            foreach (var one in header.Credits)
            {
                fixedDataBlocks.Add(new ContentBlock(Encoding.Latin1.GetBytes(one + "\0")).AppendTo(creditsBlk));
            }
            stringTableBlk.Children.Add(creditsBlk);

            fixedDataBlocks.Add(
                new ContentBlock(Encoding.Latin1.GetBytes(header.OtherInformation + "\0"), "otherInfo")
                    .AppendTo(stringTableBlk)
            );

            var entryHeadersBlk = new LengthBlock(4 + (32 + 60) * header.PatchEntries.Count, "contentHeaders");
            headerBlk.Children.Add(entryHeadersBlk);

            foreach (var one in header.PatchEntries)
            {
                fixedDataBlocks.Add(
                    new ContentBlock(one.RawContent, "content")
                        .AppendTo(entryHeadersBlk)
                );
            }

            var buffer = new byte[BlockHelper.EnsureDescendantOffsets(headerBlk, 0)];
            {
                foreach (var one in fixedDataBlocks)
                {
                    one.TransferTo(buffer);
                }

                {
                    var cursor = headerBlk.SliceBuffer(buffer);

                    BinaryPrimitives.WriteUInt32LittleEndian(cursor.Span, 1345472587u);
                    cursor = cursor.Slice(4);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, stringTableBlk.EnsuredOffset);
                    cursor = cursor.Slice(4);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, entryHeadersBlk.EnsuredOffset);
                    cursor = cursor.Slice(4);

                    BinaryPrimitives.WriteUInt32LittleEndian(cursor.Span, header.Revision);
                }

                {
                    var cursor = stringTableBlk.SliceBuffer(buffer);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, changeLogsBlk.EnsuredOffset - stringTableBlk.EnsuredOffset);
                    cursor = cursor.Slice(4);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, creditsBlk.EnsuredOffset - stringTableBlk.EnsuredOffset);
                    cursor = cursor.Slice(4);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, stringTableBlk.Tag("otherInfo").EnsuredOffset - stringTableBlk.EnsuredOffset);
                }

                {
                    var cursor = changeLogsBlk.SliceBuffer(buffer);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, header.ChangeLogs.Length);
                    cursor = cursor.Slice(4);

                    foreach (var one in changeLogsBlk.Children)
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, one.EnsuredOffset - changeLogsBlk.EnsuredOffset);
                        cursor = cursor.Slice(4);
                    }
                }

                {
                    var cursor = creditsBlk.SliceBuffer(buffer);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, header.Credits.Length);
                    cursor = cursor.Slice(4);

                    foreach (var one in creditsBlk.Children)
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, one.EnsuredOffset - creditsBlk.EnsuredOffset);
                        cursor = cursor.Slice(4);
                    }
                }
                {
                    var cursor = entryHeadersBlk.SliceBuffer(buffer);

                    BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, header.PatchEntries.Count);
                    cursor = cursor.Slice(4);

                    foreach (var (entry, contentBlk) in header.PatchEntries.Zip(entryHeadersBlk.Tags("content")))
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(cursor.Span, entry.Hash);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, contentBlk.EnsuredOffset);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, contentBlk.Length);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteUInt32LittleEndian(cursor.Span, entry.UncompressedSize);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteUInt32LittleEndian(cursor.Span, entry.Parent);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteUInt32LittleEndian(cursor.Span, entry.Relink);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, entry.IsCompressed ? 1 : 0);
                        cursor = cursor.Slice(4);

                        BinaryPrimitives.WriteInt32LittleEndian(cursor.Span, entry.IsCustomFile ? 1 : 0);
                        cursor = cursor.Slice(4);

                        cursor = cursor.Slice(60); // padding
                    }
                }
            }
            return buffer;
        }
    }
}
