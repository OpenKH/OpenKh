/*
    Kingdom Save Editor
    Copyright (C) 2021 Luciano Ciccariello
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SaveData
{
    public static class SaveDataFactory
    {
        public const uint MagicCodeJp = 0x4a32484b;
        public const uint MagicCodeUs = 0x5532484b;
        public const uint MagicCodeEu = 0x4532484b;

        public enum GameVersion
        {
            Japanese = 0x2a,
            American = 0x2d,
            FinalMix = 0x3a
        }

        public static bool IsValid(Stream stream)
        {
            var prevPosition = stream.Position;
            var magicCode = new BinaryReader(stream).ReadUInt32();
            stream.Position = prevPosition;

            switch (magicCode)
            {
                case MagicCodeJp:
                case MagicCodeUs:
                case MagicCodeEu:
                    return true;
                default:
                    return false;
            }
        }

        public static GameVersion? GetGameVersion(Stream stream)
        {
            if (!IsValid(stream))
                return null;

            var prevPosition = stream.Position;
            stream.Position = 4;
            var version = new BinaryReader(stream).ReadUInt32();
            stream.Position = prevPosition;

            switch ((GameVersion)version)
            {
                case GameVersion.Japanese:
                case GameVersion.American:
                case GameVersion.FinalMix:
                    return (GameVersion)version;
                default:
                    return null;
            }
        }

        private static TSaveData Read<TSaveData>(Stream stream)
            where TSaveData : class, ISaveData =>
            BinaryMapping.ReadObject<TSaveData>(stream.FromBegin());

        public static ISaveData Read(Stream stream)
        {
            switch (GetGameVersion(stream))
            {
                case GameVersion.Japanese:
                    throw new NotImplementedException("Japanese save file is not yet supported.");
                case GameVersion.American:
                    return Read<SaveEuropean>(stream);
                case GameVersion.FinalMix:
                    return Read<SaveFinalMix>(stream);
                case null:
                    throw new NotImplementedException("An invalid version has been specified.");
                default:
                    throw new NotImplementedException("The version has been recognized but it is not supported.");
            }
        }

        public static void Write<TSaveData>(Stream stream, TSaveData save)
            where TSaveData : class, ISaveData
        {
            uint checksum;
            using (var tempStream = new MemoryStream())
            {
                save.Write(tempStream);
                var rawData = tempStream.SetPosition(0xc).ReadBytes();
                checksum = CalculateChecksum(tempStream.FromBegin().ReadBytes(8), 8, uint.MaxValue);
                checksum = CalculateChecksum(rawData, rawData.Length, checksum ^ uint.MaxValue);
            }

            save.Checksum = checksum;
            save.Write(stream.FromBegin());
        }

        private const int CrcPolynomial = 0x04c11db7;
        private static uint[] crc_table = GetCrcTable(CrcPolynomial)
            .Take(0x100)
            .ToArray();

        public static uint CalculateChecksum(byte[] data, int offset, uint checksum)
        {
            for (var i = 0; i < offset; i++)
                checksum = crc_table[(checksum >> 24) ^ data[i]] ^ (checksum << 8);

            return checksum ^ uint.MaxValue;
        }

        private static IEnumerable<uint> GetCrcTable(int polynomial)
        {
            for (var x = 0; ; x++)
            {
                var r = x << 24;
                for (var j = 0; j < 0xff; j++)
                    r = r << 1 ^ (r < 0 ? polynomial : 0);
                yield return (uint)r;
            }
        }
    }
}
