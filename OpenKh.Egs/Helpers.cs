using Ionic.Zlib;
using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Egs
{
    public static class Helpers
    {
        #region Utils

        public static IEnumerable<string> GetAllFiles(string folder)
        {
            return Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                            .Select(x => x.Replace($"{folder}\\", "")
                            .Replace(@"\", "/"));
        }

        public static string ToString(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 2);
            for (var i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("X02"));

            return sb.ToString();
        }

        public static byte[] ToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static byte[] CompressData(byte[] data)
        {
            using (MemoryStream compressedStream = new MemoryStream())
            {
                var deflateStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.Default, true);

                deflateStream.Write(data, 0, data.Length);
                deflateStream.Close();

                var compressedData = compressedStream.ReadAllBytes();

                // Make sure compressed data is aligned with 0x10
                int padding = compressedData.Length % 0x10 == 0 ? 0 : (0x10 - compressedData.Length % 0x10);
                Array.Resize(ref compressedData, compressedData.Length + padding);

                return compressedData;
            }
        }

        public static Hed.Entry CreateHedEntry(string filename, byte[] decompressedData, byte[] compressedData, long offset, List<EgsHdAsset.RemasteredEntry> remasteredHeaders = null)
        {
            var fileHash = CreateMD5(filename);
            // 0x10 => size of the original asset header
            // 0x30 => size of the remastered asset header
            var dataLength = compressedData.Length + 0x10;

            if (remasteredHeaders != null)
            {
                foreach (var header in remasteredHeaders)
                {
                    dataLength += header.CompressedLength + 0x30;
                }
            }

            return new Hed.Entry()
            {
                MD5 = ToBytes(fileHash),
                ActualLength = decompressedData.Length,
                DataLength = dataLength,
                Offset = offset
            };
        }

        public static string GetRelativePath(string filePath, string origin)
        {
            return filePath.Replace($"{origin}\\", "").Replace(@"\", "/");
        }

        #endregion
    }
}
