using OpenKh.Common;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using Xe.BinaryMapper;

namespace OpenKh.Recom
{
    public record RootDirInfo
    {
        public List<RootFileInfo> FilesInfo { get; set; } = new List<RootFileInfo>();
        public List<SubDirInfo> SubDirsInfo { get; set; } = new List<SubDirInfo>();

        [Data] public byte BlockCount { get; set; }
        [Data(Count = 5)] public string Id { get; set; } = "";
        [Data] public ushort FileCount { get; set; }
        [Data] public ushort FilesInfoBlockCount { get; set; }

        public static RootDirInfo Read(Stream stream)
        {
            var rdi = BinaryMapping.ReadObject<RootDirInfo>(stream);
            for (int i = 0; i < rdi.FileCount; i++)
            {
                RootFileInfo rfi = BinaryMapping.ReadObject<RootFileInfo>(stream.SetPosition(0x800 + i * 32));
                if (rfi.SubDirsInfoOffset != 0 && rfi.SubDirsCount > 0)
                {
                    rfi.SubDirsInfoStartIndex = rdi.SubDirsInfo.Count;
                    var name = rfi.Filename.Length >= 4 ? rfi.Filename.Substring(1, 3) : String.Empty;
                    uint dir_offset;
                    if (!uint.TryParse(name, out dir_offset))
                    {
                        // Skip this file
                        continue;
                    }
                    dir_offset *= 10000;
                    for (int j = 0; j < rfi.SubDirsCount; j++)
                    {
                        SubDirInfo sdi = BinaryMapping.ReadObject<SubDirInfo>(stream.SetPosition(0x800 + rfi.SubDirsInfoOffset + j * 32));
                        sdi.DirId += dir_offset;
                        rdi.SubDirsInfo.Add(sdi);
                    }
                }
                rdi.FilesInfo.Add(rfi);
            }
            return rdi;
        }

        public void ExtractFiles(Stream iso_stream, string extract_path, Action<float> onProgress = null)
        {
            if (onProgress != null)
            {
                onProgress(0.0f);
            }

            if (!Directory.Exists(extract_path))
            {
                Directory.CreateDirectory(extract_path);
            }

            var symlinks = new List<(String, String, String, uint, SubFileInfo)>();
            for(int file_ind = 0;  file_ind < FilesInfo.Count; file_ind++)
            {
                var file_info = FilesInfo[file_ind];
                if (file_info.SubDirsInfoStartIndex.HasValue)
                {
                    // This is a sub-directory, TO-DO: Extract all files in the sub-directory
                    for (int i = 0; i < file_info.SubDirsCount; i++)
                    {
                        var sub_dir_info = SubDirsInfo[file_info.SubDirsInfoStartIndex.Value + i];

                        var dir_path = Path.Combine(extract_path, sub_dir_info.DirId.ToString());
                        if (!Directory.Exists(dir_path))
                        {
                            Directory.CreateDirectory(dir_path);
                        }

                        var start_block = file_info.StartBlock + sub_dir_info.StartBlock;
                        var mem_stream = IsoUtility.GetSectors(iso_stream, start_block, (int)sub_dir_info.FilesListBlockCount);
                        if (mem_stream.Length == 0)
                        {
                            // Skip to next file
                            continue;
                        }
                        int j = 0;

                        while (mem_stream.ReadByte() > 0)
                        {
                            SubFileInfo sub_file_info = BinaryMapping.ReadObject<SubFileInfo>(mem_stream.SetPosition(j * 48));
                            if (sub_file_info.DirId == sub_dir_info.DirId)
                            {
                                var file_path = Path.Combine(dir_path, sub_file_info.Filename);
                                var mem_stream_2 = IsoUtility.GetSectors(iso_stream, start_block + sub_file_info.StartBlock, (int)sub_file_info.BlockCount);
                                byte[] buffer = sub_file_info.IsCompressed == 1 ? Decompress(mem_stream_2) : mem_stream_2.ReadBytes((int)sub_file_info.BlockCount * 0x800);
                                var file_stream = File.Open(file_path, FileMode.Create, FileAccess.Write);
                                file_stream.Write(buffer, 0, (int)sub_file_info.FileLen);
                                file_stream.Close();
                            }
                            else if (sub_file_info.SymlinkDirId == sub_dir_info.DirId)
                            {
                                // This dir contains a symlink to a file in another dir
                                // TO-DO: Decide if we want a symlink to the true location of the file
                                var symlink_path = Path.Combine(dir_path, sub_file_info.Filename);
                                var target_path = Path.Combine(extract_path, sub_file_info.DirId.ToString(), sub_file_info.Filename);
                                symlinks.Add((symlink_path, target_path, dir_path, start_block, sub_file_info));
                            }
                            //*
                            else
                            {
                                // Both dir ID's are for different dirs?
                                var symlink_path = Path.Combine(dir_path, sub_file_info.Filename);
                                var target_path = Path.Combine(extract_path, sub_file_info.SymlinkDirId.ToString(), sub_file_info.Filename);
                                symlinks.Add((symlink_path, target_path, dir_path, start_block, sub_file_info));
                            }
                            //*/
                            j++;
                        }
                    }
                }
                else
                {
                    // This is a file, extract it
                    var file_path = Path.Combine(extract_path, file_info.Filename);
                    var mem_stream = IsoUtility.GetSectors(iso_stream, file_info.StartBlock, (int)file_info.BlockCount);
                    var file_stream = File.Open(file_path, FileMode.Create, FileAccess.Write);
                    file_stream.Write(mem_stream.ReadBytes((int)file_info.BlockCount * 0x800));
                    file_stream.Close();
                }
                if (onProgress != null)
                {
                    onProgress(file_ind / (float)FilesInfo.Count);
                }
            }

            foreach ((String symlink_path, String target_path, String dir_path, uint start_block, SubFileInfo sub_file_info) in symlinks)
            {
                // Create the symbolic link
                if (File.Exists(target_path))
                {
                    if (!File.Exists(symlink_path))
                    {
                        try
                        {
                            File.CreateSymbolicLink(symlink_path, target_path);
                        }
                        catch (System.IO.IOException)
                        {
                            // Fallback in case we don't have permissions for a symlink
                            File.Copy(target_path, symlink_path, overwrite: false);
                        }
                    }
                }
                else
                {
                    var file_path = Path.Combine(dir_path, sub_file_info.Filename);
                    var mem_stream_2 = IsoUtility.GetSectors(iso_stream, start_block + sub_file_info.StartBlock, (int)sub_file_info.BlockCount);
                    byte[] buffer = sub_file_info.IsCompressed == 1 ? Decompress(mem_stream_2) : mem_stream_2.ReadBytes((int)sub_file_info.BlockCount * 0x800);
                    var file_stream = File.Open(file_path, FileMode.Create, FileAccess.Write);
                    file_stream.Write(buffer, 0, (int)sub_file_info.FileLen);
                    file_stream.Close();
                }
            }

            if (onProgress != null)
            {
                onProgress(1.0f);
            }
        }

        public static byte[] Decompress(Stream src)
        {
            return Decompress(new BinaryReader(src).ReadBytes((int)src.Length));
        }

        // Based on Ghidra decompilation
        public static byte[] Decompress(byte[] srcData)
        {
            List<byte> dstData = new List<byte>();
            byte[] tmpBuffer = new byte[256];
            int numIters = srcData.Length / 0x1000;
            numIters += srcData.Length % 0x1000 != 0 ? 1 : 0;

            for (uint i = 0; i < numIters; i++)
            {
                uint srcInd = i * 0x1000;

                uint uVar6 = 0;
                uint uVar7 = 0;
                uint uVar4 = 0;
                uint uVar8 = 1;
                uint uVar5;
                int iVar3;
                uint uVar10;
                uint uVar9;
                uint uVar11;
                while (true)
                {
                    while (true)
                    {
                        if (uVar6 == 0)
                        {
                            uVar6 = 255;
                            uVar4 = 8;
                            uVar7 = srcData[srcInd];
                            srcInd++;
                        }
                        uVar5 = uVar6 / 2;
                        iVar3 = (int)(uVar4 - 1);
                        if ((uVar6 & uVar7 & (uVar5 ^ 255)) == 0)
                            break;
                        if (uVar5 == 0)
                        {
                            uVar5 = 255;
                            iVar3 = 8;
                            uVar7 = srcData[srcInd];
                            srcInd++;
                        }
                        uVar5 = uVar5 & uVar7;
                        uVar10 = (uint)(8 - iVar3);
                        uVar6 = 255;
                        uVar4 = 8;
                        uVar7 = (uint)srcData[srcInd];
                        srcInd++;
                        if (uVar10 != 0)
                        {
                            uVar6 = (uint)(255 >> (int)(uVar10 & 31));
                            uVar4 = 8 - uVar10;
                            uVar5 = uVar5 << (int)(uVar10 & 31) | ((uVar6 ^ 255) & uVar7) >> (int)(uVar4 & 0x1f);

                        }
                        tmpBuffer[uVar8] = (byte)uVar5;
                        dstData.Add((byte)uVar5);
                        uVar8++;
                        uVar8 &= 255;
                    }
                    if (uVar5 == 0)
                    {
                        uVar5 = 255;
                        iVar3 = 8;
                        uVar7 = srcData[srcInd];
                        srcInd++;
                    }
                    uVar5 = uVar5 & uVar7;
                    uVar6 = (uint)(8 - iVar3);
                    uVar10 = 255;
                    uVar4 = 8;
                    uVar7 = srcData[srcInd];
                    if (uVar6 != 0)
                    {
                        uVar10 = (uint)(255 >> (int)(uVar6 & 31));
                        uVar4 = 8 - uVar6;
                        uVar5 = uVar5 << (int)(uVar6 & 31) | ((uVar10 ^ 255) & uVar7) >> (int)(uVar4 & 31);
                    }
                    if (uVar5 == 0)
                        break;
                    uVar9 = 0;
                    uint tmpSrcInd = srcInd + 1;
                    if (uVar10 == 0)
                    {
                        uVar10 = 255;
                        uVar4 = 8;
                        uVar7 = srcData[srcInd + 1];
                        tmpSrcInd = srcInd + 2;
                    }
                    srcInd = tmpSrcInd;
                    uVar6 = 4 - uVar4;
                    uVar11 = 4;
                    if (0 < (int)uVar6)
                    {
                        uVar9 = uVar10 & uVar7;
                        uVar10 = 255;
                        uVar4 = 8;
                        uVar7 = srcData[srcInd];
                        srcInd++;
                        uVar11 = uVar6;
                    }
                    uVar6 = uVar10 >> (int)(uVar11 & 31);
                    uVar4 = uVar4 - uVar11;
                    iVar3 = (int)(uVar9 << (int)(uVar11 & 31) | (uVar10 & uVar7 & (uVar6 ^ 255)) >> (int)(uVar4 & 31)) + 1;
                    do
                    {
                        byte bVar1 = tmpBuffer[uVar5 & 255];
                        tmpBuffer[uVar8] = bVar1;
                        dstData.Add(bVar1);
                        uVar8++;
                        uVar8 &= 255;
                        uVar5++;
                        iVar3--;
                    } while (-1 < iVar3);
                }
            }
            return dstData.ToArray();
        }
    }

    public record RootFileInfo
    {
        [Data(Count = 16)] public string Filename { get; set; } = "";
        [Data] public uint StartBlock { get; set; }
        [Data] public uint BlockCount { get; set; }
        [Data] public uint SubDirsInfoOffset { get; set; }
        [Data] public uint SubDirsCount { get; set; }
        public int? SubDirsInfoStartIndex = null;
    }

    public record SubDirInfo
    {
        [Data] public uint DirId { get; set; }
        [Data] public uint StartBlock { get; set; }
        [Data] public uint FilesDataBlockCount { get; set; }
        [Data] public byte FilesListBlockCount { get; set; }
        [Data] public byte Unk0x1D { get; set; }
        [Data] public byte Unk0x1E { get; set; }
        [Data] public byte Unk0x1F { get; set; }
    }

    public record SubFileInfo
    {
        [Data(Count = 24)] public string Filename { get; set; } = "";
        [Data] public uint FileLen { get; set; }
        [Data] public uint DirId { get; set; }
        [Data] public uint SymlinkDirId { get; set; }
        [Data] public uint BlockCount { get; set; }
        [Data] public uint StartBlock { get; set; }
        [Data] public byte FileType { get; set; }
        [Data] public byte IsCompressed { get; set; }
        [Data] public byte Unk0x2E { get; set; }
        [Data] public byte Unk0x2F { get; set; }
    }

    public class Dat
    {
        public List<SubDirInfo> SubDirsInfo { get; set; } = new List<SubDirInfo>();
        public List<List<SubFileInfo>> SubDirsFileLists { get; set; } = new List<List<SubFileInfo>>();
        public List<List<byte[]>> SubDirsFileContents { get; set; } = new List<List<byte[]>>();
    }
}
