using OpenKh.Common;
using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class DpdPData
    {
        // ********************************************
        // PROPERTIES
        // ********************************************
        public DpdPDataHeader Header { get; set; }
        public List<PDataEntry> Entries { get; set; }
        public List<int> UnkList { get; set; }

        // ********************************************
        // FUNCTIONS
        // ********************************************
        public DpdPData(Stream stream)
        {
            // Header
            Header = BinaryMapping.ReadObject<DpdPDataHeader>(stream);

            // Unk list
            UnkList = new List<int>();
            stream.Position = Header.UnknownListAddress;
            int unkListCount = stream.ReadInt32();
            for (int i = 0; i < unkListCount; i++)
            {
                UnkList.Add(stream.ReadInt32());
            }

            // Addresses
            List<int> entryAddresses = new List<int>();
            stream.Position = Header.EntryAddressesListAddress;
            int pDataEntryCount = stream.ReadInt32();
            for (int i = 0; i < pDataEntryCount; i++)
            {
                entryAddresses.Add(stream.ReadInt32());
            }

            // PData Entries
            Entries = new List<PDataEntry>();
            for (int i = 0; i < pDataEntryCount; i++)
            {
                PDataEntry entry = new PDataEntry();
                stream.Position = entryAddresses[i];

                // Header
                entry.Header = BinaryMapping.ReadObject<PDataEntry.PDataEntryHeader>(stream);

                // Programs
                entry.ProgramList = new List<PDataEntry.PppProgram>();
                for(int j = 0; j < entry.Header.ProgramCount; j++)
                {
                    PDataEntry.PppProgram program = BinaryMapping.ReadObject<PDataEntry.PppProgram>(stream);
                    entry.ProgramList.Add(program);
                }

                // Program parameters and useVal data
                for (int j = 0; j < entry.ProgramList.Count; j++)
                {
                    PDataEntry.PppProgram program = entry.ProgramList[j];
                    program.Entries = new List<PDataEntry.PppProgram.PppProgramEntry> ();
                    program.UseVal = new List<int> ();

                    // Calc list counts
                    int paramCount = program.ParamSize / 4;
                    int useValSize = 0;
                    if(j == entry.ProgramList.Count - 1) // Last program
                    {
                        useValSize = entry.Header.CvOffPointer - program.UseValPointer;
                    }
                    else
                    {
                        useValSize = entry.ProgramList[j+1].ParamsPointer - program.UseValPointer;
                    }
                    int useValCount = useValSize / 4;


                    // Program params
                    stream.Position = program.ParamsPointer;
                    for (int k = 0; k < program.EntryCount; k++)
                    {
                        PDataEntry.PppProgram.PppProgramEntry programEntry = new PDataEntry.PppProgram.PppProgramEntry();
                        programEntry.Params = new List<int> ();
                        for(int l = 0; l < paramCount; l++)
                        {
                            programEntry.Params.Add(stream.ReadInt32());
                        }
                        program.Entries.Add(programEntry);
                    }
                    // Program UseVal list
                    stream.Position = program.UseValPointer;
                    program.UseVal = new List<int>();
                    for (int k = 0; k < useValCount; k++)
                    {
                        program.UseVal.Add(stream.ReadInt32());
                    }
                }

                // CvOff data
                stream.Position = entry.Header.CvOffPointer;
                entry.CvOff = new List<int>();
                // Size seems to always be 16 ints, but just in case we calculate it
                int endPointer = Header.UnknownListAddress - 4; // Unknown list offset minus the int that indicates there are no more entries
                if (i < pDataEntryCount - 1)
                {
                    endPointer = entry.Header.NextItemPointer;
                }
                int cvOffCount = (endPointer - entry.Header.CvOffPointer) / 4;

                for(int j = 0; j < cvOffCount; j++)
                {
                    entry.CvOff.Add(stream.ReadInt32());
                }

                Entries.Add(entry);
            }
        }

        public Stream getAsStream()
        {
            Stream fileStream = new MemoryStream();

            // Header
            BinaryMapping.WriteObject(fileStream, Header);

            // Entries
            List<int> entryAddresses = new List<int>();
            List<int> cvoffAddresses = new List<int>();
            foreach (PDataEntry entry in Entries)
            {
                entryAddresses.Add((int)fileStream.Position);

                // Header
                BinaryMapping.WriteObject(fileStream, entry.Header);

                // Programs
                List<int> programAddresses = new List<int>();
                foreach (PDataEntry.PppProgram program in entry.ProgramList)
                {
                    programAddresses.Add((int)fileStream.Position);
                    program.EntryCount = (ushort)program.Entries.Count;
                    BinaryMapping.WriteObject(fileStream, program);
                }

                // Program params & useVal
                List<int> paramAddresses = new List<int>();
                List<int> useValAddresses = new List<int>();
                foreach (PDataEntry.PppProgram program in entry.ProgramList)
                {
                    // Params
                    paramAddresses.Add((int)fileStream.Position);
                    foreach(PDataEntry.PppProgram.PppProgramEntry programEntry in program.Entries)
                    {
                        foreach(int paramInt in programEntry.Params)
                        {
                            fileStream.Write(paramInt);
                        }
                    }
                    fileStream.Write((uint)0xFFFFF000);
                    useValAddresses.Add((int)fileStream.Position);
                    foreach (int useValInt in program.UseVal)
                    {
                        fileStream.Write(useValInt);
                    }
                }

                // cvoff
                cvoffAddresses.Add((int)fileStream.Position);
                foreach (int cvOffInt in entry.CvOff)
                {
                    fileStream.Write(cvOffInt);
                }

                int endPosition = (int)fileStream.Position;

                // SET POINTERS
                for (int i = 0; i < programAddresses.Count; i++)
                {
                    fileStream.Position = programAddresses[i] + 8;
                    fileStream.Write(paramAddresses[i]);
                    fileStream.Write(useValAddresses[i]);
                }

                fileStream.Position = endPosition;
            }

            int listEndPosition = (int)fileStream.Position;
            fileStream.Write((int)0x0); // End of list

            // Unk list
            int unkListPosition = (int)fileStream.Position;
            fileStream.Write((int)UnkList.Count);
            foreach (int unkInt in UnkList)
            {
                fileStream.Write(unkInt);
            }

            // PData entries addresses
            int entryAddressesPosition = (int)fileStream.Position;
            fileStream.Write((int)entryAddresses.Count);
            foreach (int entryAddress in entryAddresses)
            {
                fileStream.Write(entryAddress);
            }
            ReadWriteUtils.alignStreamToByte(fileStream, 16);

            // SET POINTERS
            for (int i = 0; i < entryAddresses.Count; i++)
            {
                int nextOffset = listEndPosition;
                if(i < entryAddresses.Count - 1)
                {
                    nextOffset = entryAddresses[i + 1];
                }

                fileStream.Position = entryAddresses[i];
                fileStream.Write(nextOffset);
                fileStream.Write(cvoffAddresses[i]);
            }

            fileStream.Position = 0;
            return fileStream;
        }

        // ********************************************
        // INNER CLASSES
        // ********************************************
        public class DpdPDataHeader
        {
            [Data(Count = 8)] public byte[] Unk { get; set; }
            [Data] public int UnknownListAddress { get; set; }
            [Data] public int EntryAddressesListAddress { get; set; }
        }

        public class PDataEntry
        {
            public PDataEntryHeader Header { get; set; }
            public List<PppProgram> ProgramList { get; set; }
            public List<int> CvOff { get; set; } // Unknown list. Always 16 ints?

            public class PDataEntryHeader
            {
                [Data] public int NextItemPointer { get; set; }
                [Data] public int CvOffPointer { get; set; }
                [Data] public int Flag { get; set; }
                [Data] public int Status { get; set; }
                [Data] public int StartFrameInit { get; set; }
                [Data] public int TotalFrame { get; set; }
                [Data] public int LoopFrame0 { get; set; }
                [Data] public int LoopFrame1 { get; set; }
                [Data] public int Valbytes { get; set; }
                [Data] public short Priority { get; set; }
                [Data] public short ProgramCount { get; set; }
            }

            public class PppProgram
            {
                [Data] public int ProgramType { get; set; }
                [Data] public ushort ParamSize { get; set; } // Parameters size in bytes
                [Data] public ushort EntryCount { get; set; }
                [Data] public int ParamsPointer { get; set; }
                [Data] public int UseValPointer { get; set; }
                public PppProgramEnum ProgramEnum
                {
                    get
                    {
                        return (PppProgramEnum)ProgramType;
                    }
                }
                public List<PppProgramEntry> Entries { get; set; }
                public List<int> UseVal { get; set; } // Unknown list

                public class PppProgramEntry
                {
                    public List<int> Params { get; set; }
                }

                public override string ToString()
                {
                    return ProgramEnum.ToString() + " | Param Size: " + ParamSize + " | Entries: " + EntryCount;
                }

                public enum PppProgramEnum
                {
                    pppDummyFunc=1,
                    pppKeThRes=2,
                    pppKeThRes8=3,
                    pppKeThRes8x4=4,
                    pppKeThRes8x128=5,
                    pppKeThRes16=6,
                    pppKeThRes16x4=7,
                    pppKeThRes16x64=8,
                    pppKeThRes24=9,
                    pppKeThRes24x4=10,
                    pppKeThRes32=11,
                    pppKeThRes32x4=12,
                    pppKeThRes32x32=13,
                    pppKeThRes40=14,
                    pppKeThRes40x4=15,
                    pppKeThRes48=16,
                    pppKeThRes48x4=17,
                    pppKeThRes64=18,
                    pppKeThRes64x4=19,
                    pppKeThRes64x16=20,
                    pppKeThRes128=21,
                    pppKeThRes128x4=22,
                    pppKeThRes128x8=23,
                    pppKeThRes255=24,
                    pppKeThRes255x4=25,
                    pppKeHitBall=26,
                    pppKeGrvTgt=27,
                    pppAccele=28,
                    pppAngAccele=29,
                    pppSclAccele=30,
                    pppColAccele=31,
                    pppEiWfacc=32,
                    pppKeHmgEff=33,
                    pppKeGrvEff=34,
                    pppKeHitChkPxB=35,
                    pppMove=36,
                    pppAngMove=37,
                    pppSclMove=38,
                    pppColMove=39,
                    pppPoint=40,
                    pppAngle=41,
                    pppScale=42,
                    pppColor=43,
                    pppPObjPoint=44,
                    pppVertexAttend=45,
                    pppEiWindFun=46,
                    pppKeMvYpEff=47,
                    pppKeDrct=48,
                    pppVtMime=49,
                    pppKeTkFade=50,
                    pppRandFloat=51,
                    pppRandUpFloat=52,
                    pppRandDownFloat=53,
                    pppRandChar=54,
                    pppRandUpChar=55,
                    pppRandDownChar=56,
                    pppRandShort=57,
                    pppRandUpShort=58,
                    pppRandDownShort=59,
                    pppRandInt=60,
                    pppRandUpInt=61,
                    pppRandDownInt=62,
                    pppRandFV=63,
                    pppRandUpFV=64,
                    pppRandDownFV=65,
                    pppRandIV=66,
                    pppRandUpIV=67,
                    pppRandDownIV=68,
                    pppRandCV=69,
                    pppRandUpCV=70,
                    pppRandDownCV=71,
                    pppRandHCV=72,
                    pppRandUpHCV=73,
                    pppRandDownHCV=74,
                    pppSRandFV=75,
                    pppSRandUpFV=76,
                    pppSRandDownFV=77,
                    pppSRandCV=78,
                    pppSRandUpCV=79,
                    pppSRandDownCV=80,
                    pppSRandHCV=81,
                    pppSRandUpHCV=82,
                    pppSRandDownHCV=83,
                    pppSMatrix=84,
                    pppKeMatSN=85,
                    pppMatrix=86,
                    pppMatrixXYZ=87,
                    pppMatrixXZY=88,
                    pppMatrixYZX=89,
                    pppMatrixYXZ=90,
                    pppMatrixZXY=91,
                    pppMatrixZYX=92,
                    pppMatrixLoc=93,
                    pppMatrixScl=94,
                    pppMatrixFront=95,
                    pppRyjMatrixNoRot=96,
                    pppKeMatPht=97,
                    pppRyjMatrixWorld=98,
                    pppRyjMatrixWorldXYZ=99,
                    pppRyjMatrixWorldXZY=100,
                    pppRyjMatrixWorldYXZ=101,
                    pppRyjMatrixWorldYZX=102,
                    pppRyjMatrixWorldZXY=103,
                    pppRyjMatrixWorldZYX=104,
                    pppParMatrix=105,
                    pppKeParMatR=106,
                    pppChrSclXYZMatrix=107,
                    pppChrSclXZMatrix=108,
                    pppChrSclYMatrix=109,
                    pppChrYSclXYZMatrix=110,
                    pppChrXSclXYZMatrix=111,
                    pppDrawMatrix=112,
                    pppDrawMatrixFront=113,
                    pppDrawMatrixNoRot=114,
                    pppDrawMatrixWood=115,
                    pppKeDMat=116,
                    pppKeDMatFr=117,
                    pppKeDMatPht=118,
                    pppKeDMatPhtFr=119,
                    pppRyjDrawMatrixWorld=120,
                    pppRyjDrawMatrixWorldFront=121,
                    pppRyjDrawMatrixWorldNoRot=122,
                    pppRyjDrawMatrixWorldWood=123,
                    pppKeZCrct=124,
                    pppKeZCrctShp=125,
                    pppKeThTp=126,
                    pppKeThTp2=127,
                    pppKeThSft=128,
                    pppKeTh=129,
                    pppDrawMdlBS=130,
                    pppDrawMdl=131,
                    pppDrawMdlSemi=132,
                    pppDrawMdlTs=133,
                    pppDrawMdl2=134,
                    pppDrawMdlSemi2=135,
                    pppDrawMdlTs2=136,
                    pppDrawMdl3=137,
                    pppDrawMdlSemi3=138,
                    pppDrawMdlTs3=139,
                    pppDrawMdlSea=140,
                    pppDrawShape=141,
                    pppDrawShapeX=142,
                    pppDrawShapeField=143,
                    pppKeMdlBmp=144,
                    pppKeMdlDtt=145,
                    pppKeMdlTfd=146,
                    pppKeMdlTfdUv=147,
                    pppKeMdlTfd2=148,
                    pppKeMdlTfdUv2=149,
                    pppKeMdlTfd3=150,
                    pppKeMdlTfdUv3=151,
                    pppKeShpTail=152,
                    pppKeShpTailX=153,
                    pppKeShpTail2=154,
                    pppKeShpTail2X=155,
                    pppKeShpTail3=156,
                    pppKeShpTail3X=157,
                    pppKeShpTailPht=158,
                    pppKeShpTailLc=159,
                    pppKeShpDtt=160,
                    pppRyjDrawShipoly=161,
                    pppDrawHook=162,
                    pppSDMatrix=163,
                    pppSCMatrix=164,
                    pppWMatrix=165,
                    pppWMatrixXYZ=166,
                    pppWMatrixXZY=167,
                    pppWMatrixYZX=168,
                    pppWMatrixYXZ=169,
                    pppWMatrixZXY=170,
                    pppWMatrixZYX=171,
                    pppPointAp=172,
                    pppPointRAp=173,
                    pppVertexAp=174,
                    pppSegmentAp=175,
                    pppFaceAp=176,
                    pppVertexApLc=177,
                    pppVertexApAt=178,
                    pppKeBornRnd=179,
                    pppKeBornRnd2=180,
                    pppKeBornRnd3=181,
                    pppKeBornRnd4=182,
                    pppKeBornRnd5=183,
                    pppKeBornRnd6=184,
                    pppKeBornPtCmpl=185,
                    pppKeHitBorn=186,
                    pppKeThHitBorn=187,
                    pppKeLnsLpSft=188,
                    pppKeLnsLp=189,
                    pppKeLnsArnd=190,
                    pppKeLnsClm=191,
                    pppKeLnsCrn=192,
                    pppKeLnsFls=193,
                    pppKeAcmSolid=194,
                    pppKeHitChk=195,
                    pppKeThLz=196,
                    pppKeThCp=197,
                    pppKeThCpSft=198,
                    pppKeAccSpdSv=199,
                    pppRyjDrawShipolyBone=200,
                    pppRyjMegaBirth=201,
                    pppRyjMngFlag=202,
                    pppRyjMegaBirthPrize=203,
                    pppRyjMegaPlace=204,
                    pppRyjMegaPlaceShape=205,
                    pppRyjMegaPlaceModel=206,
                    pppRyjMegaPlaceLamp=207,
                    pppRyjMegaBirthModel=208,
                    pppRyjDrawKekoto=209,
                    pppMoveLoop=210,
                    pppAngMoveLoop=211,
                    pppSclMoveLoop=212,
                    pppPointLoop=213,
                    pppAngleLoop=214,
                    pppScaleLoop=215,
                    pppMatrixLoop=216,
                    pppDrawMatrixLoop=217,
                    pppDrawMdlLoop=218,
                    pppDrawMdlLoopZ=219,
                    pppRyjLight=220,
                    pppKeLnsLpT=221,
                    pppKeLnsArndT=222,
                    pppKeLnsClmT=223,
                    pppKeLnsCrnT=224,
                    pppKeLnsFlsT=225,
                    pppRyjDrawMatrixWorldDtt=226,
                    pppRyjDrawMatrixWorldDttFr=227,
                    pppRyjMegaBirthFilter=228,
                    pppRyjMegaBirthModelFilter=229,
                    pppMatrixLoopXYZ=230,
                    pppMatrixLoopXZY=231,
                    pppMatrixLoopYXZ=232,
                    pppMatrixLoopYZX=233,
                    pppMatrixLoopZXY=234,
                    pppMatrixLoopZYX=235,
                    pppVertexApLcLoop=236,
                    pppSnoScaleAll=237,
                    pppSnoMdlDttNorm=238,
                    pppRyjMegaBirthUserCtrl=239,
                    pppRyjMegaBirthUserCtrl2=240,
                    pppEiDrawShipoly=241,
                    pppRyjMegaBirthUserCtrl3=242
                }
            }
        }
    }
}
