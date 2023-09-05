using OpenKh.Common;
using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
        public class ParticleData
        {
            public PDataHeader Header { get; set; }
            DpdPData ParticleDataFile { get; set; }

            public byte[] Unknown { get; set; } // Structure is unknown so the binary is stored for now

            public class PDataHeader
            {
                [Data(Count = 32)] public float[] afBbox { get; set; }
                [Data(Count = 4)] public float[] vecOffset { get; set; }
                [Data(Count = 4)] public int[] ivecAngle { get; set; }
                [Data(Count = 4)] public float[] vecScale { get; set; }
                [Data(Count = 16)] public byte[] abUseTexture { get; set; }
                [Data(Count = 16)] public byte[] abUseShape { get; set; }
                [Data(Count = 16)] public byte[] abUseRsd { get; set; }
                [Data(Count = 16)] public byte[] abUseVsf { get; set; }
                [Data] public int nWorkMemSize { get; set; }
                [Data] public int nPacketMemSize { get; set; }
                [Data] public byte bViewClipStop { get; set; }
                [Data] public byte bChrScale { get; set; }
                [Data] public byte bBindPosOnly { get; set; }
                [Data] public byte bNonAmbient { get; set; }
                [Data] public ushort ushSeNo { get; set; }
                [Data] public ushort ushSeVolAdd { get; set; }
                [Data] public float fSeWait { get; set; }
                [Data] public int nBoneNo { get; set; }
                [Data] public float fWait { get; set; }
                [Data] public int nRes { get; set; }
            }

            public class PDataFile
            {
                public Header FileHeader { get; set; }
                public List<PDataEntry> Entries { get; set; }
                public List<int> UnkList { get; set; } // Increases per entry

                public class Header
                {
                    [Data(Count = 8)] public byte[] Unk { get; set; }
                    [Data] public int UnknownListAddress { get; set; }
                    [Data] public int EntryAddressesListAddress { get; set; }
                }

                public class PDataEntry
                {
                    public byte[] Data { get; set; } // Since the exact structure is not known, this is used to store all of the data from each entry
                    [Data] public int ptr_next { get; set; }
                    [Data] public int ptr_cvoff { get; set; }
                    [Data] public int flag { get; set; }
                    [Data] public int status { get; set; }
                    [Data] public int startFrameInit { get; set; }
                    [Data] public int totalFrame { get; set; }
                    [Data] public int loopFrame0 { get; set; }
                    [Data] public int loopFrame1 { get; set; }
                    [Data] public int valbytes { get; set; }
                    [Data] public short prio { get; set; }
                    [Data] public short itemCount { get; set; }
                    public List<Item> Items { get; set; }

                    // Each item may be a call to the particle system (ppp)
                    public class Item
                    {
                        public ItemHeader Header { get; set; }
                        public byte[] Data { get; set; }

                        public class ItemHeader
                        {
                            [Data] public int unk0 { get; set; } // Increases per itemHeader
                            [Data] public short unk4 { get; set; }
                            [Data] public short unk6 { get; set; }
                            [Data] public int unkSP_8 { get; set; } // Increases per unkSP (8 & 12) including previous itemHeaders
                            [Data] public int unkSP_12 { get; set; } // Increases per unkSP (8 & 12) including previous itemHeaders
                        }
                    }
                }

                public PDataFile(Stream stream)
                {
                    // Header
                    FileHeader = BinaryMapping.ReadObject<Header>(stream);

                    // Unk list
                    UnkList = new List<int>();
                    int unkListSize = (FileHeader.EntryAddressesListAddress - FileHeader.UnknownListAddress) / 4;
                    stream.Position = FileHeader.UnknownListAddress;
                    for(int i = 0; i < unkListSize; i++)
                    {
                        UnkList.Add(stream.ReadInt32());
                    }

                    // Addresses
                    List<int> entryAddresses = new List<int>();
                    stream.Position = FileHeader.EntryAddressesListAddress;
                    int addressListLength = stream.ReadInt32();
                    for (int i = 0; i < addressListLength; i++)
                    {
                        entryAddresses.Add(stream.ReadInt32());
                    }

                    //Entries
                    Entries = new List<PDataEntry>();
                    for (int i = 0; i < addressListLength; i++)
                    {
                        stream.Position = entryAddresses[i];

                        int entrySize = (i < addressListLength - 1) ? entryAddresses[i + 1] : FileHeader.UnknownListAddress - 4;
                        entrySize -= entryAddresses[i];

                        PDataEntry entry = new PDataEntry();
                        entry.Data = stream.ReadBytes(entrySize);
                        Entries.Add(entry);
                    }
                }

                public Stream getAsStream()
                {
                    Stream fileStream = new MemoryStream();

                    // Header
                    BinaryMapping.WriteObject(fileStream, FileHeader);

                    // Entries
                    List<int> addressList = new List<int>();
                    foreach(PDataEntry entry in Entries)
                    {
                        addressList.Add( (int)fileStream.Position );
                        fileStream.Write(entry.Data);
                    }

                    int endEntryPosition = (int)fileStream.Position;
                    fileStream.Write((int)0); // End of List

                    // Unk List
                    int unkListPosition = (int)fileStream.Position;
                    foreach (int unkValue in UnkList)
                    {
                        fileStream.Write(unkValue);
                    }

                    // Address list
                    int addressListPosition = (int)fileStream.Position;
                    fileStream.Write((int)addressList.Count);
                    foreach (int address in addressList)
                    {
                        fileStream.Write(address);
                    }

                    ReadWriteUtils.alignStreamToByte(fileStream, 16);

                    // Header addresses
                    fileStream.Position = 8;
                    fileStream.Write(unkListPosition);
                    fileStream.Write(addressListPosition);

                    // Entry linked list addresses
                    for (int i = 0; i < addressList.Count; i++)
                    {
                        fileStream.Position = addressList[i];
                        if(i < addressList.Count - 1) {
                            fileStream.Write(addressList[i + 1]);
                        }
                        else {
                            fileStream.Write(endEntryPosition);
                        }
                    }

                    fileStream.Position = 0;
                    return fileStream;
                }
            }

            public ParticleData() { }

            public ParticleData(Stream particleDataStream)
            {
                long startingAddress = particleDataStream.Position;

                Header = BinaryMapping.ReadObject<PDataHeader>(particleDataStream);
                Stream fileStream = new MemoryStream();
                particleDataStream.CopyTo(fileStream);
                particleDataStream.Position = startingAddress;

                fileStream.Position = 0;
                ParticleDataFile = new DpdPData(fileStream);
            }

            public Stream getAsStream()
            {
                Stream fileStream = new MemoryStream();

                BinaryMapping.WriteObject(fileStream, Header);
                ParticleDataFile.getAsStream().CopyTo(fileStream);

                fileStream.Position = 0;
                return fileStream;
            }
        }
    }
}
