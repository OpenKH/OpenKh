using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Kh1
{
    /// <summary>
    /// KH1 area data (.ard) archive.
    ///
    /// The header is a 32-bit entry count followed by entryCount+1 absolute offsets,
    /// so entry i spans [offsets[i], offsets[i + 1]). Every *.ard file declares 32
    /// entries and the final offset is the end of the file.
    ///
    /// Entry 5 is the resource list: a flat array of fixed 0x20 byte, NULL padded ASCII
    /// names generally laid out as (model, mset) pairs.
    /// </summary>
    public class Ard
    {
        /// <summary>Index of the entry holding the resource list.</summary>
        public const int ResourceListIndex = 5;

        /// <summary>Size in bytes of a single name record.</summary>
        public const int NameSize = 0x20;

        /// <summary>Longest name that fits a record, leaving room for the NUL terminator.</summary>
        public const int MaxNameLength = NameSize - 1;

        private const int EntryCount = 32;
        private const int HeaderSize = 4 + (EntryCount + 1) * 4;

        public static bool IsValid(Stream stream) => TryReadOffsets(stream, out _);

        /// <summary>
        /// Reads the model/mset resource list. Empty slots come back as empty strings so
        /// that indices always line up with the slots in the file.
        /// </summary>
        public static List<string> ReadResourceList(Stream stream)
        {
            var (start, end) = GetResourceListRange(stream);

            stream.SetPosition(start);
            var data = stream.ReadBytes(end - start);

            var names = new List<string>((end - start) / NameSize);
            for (var i = 0; i < data.Length; i += NameSize)
            {
                var length = 0;
                while (length < NameSize && data[i + length] != 0)
                    length++;

                names.Add(Encoding.ASCII.GetString(data, i, length));
            }

            return names;
        }

        /// <summary>
        /// Overwrites the model/mset resource list in place. The list must keep its original
        /// length: the entry size is baked into the header's offset table, so growing or
        /// shrinking it would move every section that follows.
        /// </summary>
        public static void WriteResourceList(Stream stream, IReadOnlyList<string> names)
        {
            var (start, end) = GetResourceListRange(stream);
            var slotCount = (end - start) / NameSize;

            if (names.Count != slotCount)
                throw new InvalidDataException(
                    $"The resource list has {slotCount} slots but {names.Count} were given. " +
                    "Entries can be overwritten but not added or removed.");

            var data = new byte[end - start];
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i] ?? string.Empty;
                if (name.Length > MaxNameLength)
                    throw new InvalidDataException(
                        $"The name '{name}' is {name.Length} characters long, but a resource list entry holds at most {MaxNameLength}.");

                for (var c = 0; c < name.Length; c++)
                {
                    if (name[c] > 0x7F)
                        throw new InvalidDataException($"The name '{name}' contains the non-ASCII character '{name[c]}'.");

                    data[i * NameSize + c] = (byte)name[c];
                }
            }

            stream.SetPosition(start);
            stream.Write(data);
        }

        private static (int Start, int End) GetResourceListRange(Stream stream)
        {
            if (!TryReadOffsets(stream, out var offsets))
                throw new InvalidDataException("The file is not a valid KH1 .ard archive.");

            return (offsets[ResourceListIndex], offsets[ResourceListIndex + 1]);
        }

        private static bool TryReadOffsets(Stream stream, out int[] offsets)
        {
            offsets = null;

            if (stream.Length < HeaderSize)
                return false;

            stream.SetPosition(0);
            if (stream.ReadInt32() != EntryCount)
                return false;

            var read = new int[EntryCount + 1];
            for (var i = 0; i <= EntryCount; i++)
                read[i] = stream.ReadInt32();

            // The last offset marks the end of the file, but the trailing padding is
            // stripped on disk often enough that it can overshoot by up to a sector.
            // Everything before it has to describe a real, ascending range.
            if (read[0] < HeaderSize)
                return false;

            for (var i = 0; i < EntryCount; i++)
            {
                if (read[i] > read[i + 1])
                    return false;
                if (read[i] > stream.Length)
                    return false;
            }

            var resourceListSize = read[ResourceListIndex + 1] - read[ResourceListIndex];
            if (resourceListSize <= 0 || resourceListSize % NameSize != 0)
                return false;
            if (read[ResourceListIndex + 1] > stream.Length)
                return false;

            offsets = read;
            return true;
        }
    }
}
