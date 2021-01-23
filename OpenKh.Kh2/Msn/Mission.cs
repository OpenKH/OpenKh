using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Msn
{
    public class Mission
    {
        public class _Header
        {
            [Data] public ushort Magic { get; set; }
            [Data] public ushort Id { get; set; }
            
            [Data] public byte Unk03 { get; set; }
            //[Data(4, BitIndex = 0)] public bool IsBossBattle { get; set; }
            //[Data(4, BitIndex = 1)] public bool IsDriveDisabled { get; set; }
            
            [Data] public byte Unk04 { get; set; }
            //[Data(5, BitIndex = 1)] public bool IsMickeySpawnable { get; set; }
            //[Data(5, BitIndex = 3)] public bool IsMagicDisabled { get; set; }
            //[Data(5, BitIndex = 4)] public bool IsRetryPossible { get; set; }
            //[Data(5, BitIndex = 5)] public bool AreSummonsEnabled { get; set; }

            [Data] public ushort InformationBarTextId { get; set; }
            [Data] public byte PauseMenuType { get; set; }
            [Data] public byte HelpId { get; set; }
            [Data] public ushort PauseMenuTextId { get; set; }
            [Data] public byte Help { get; set; }
            [Data] public byte BonsRewardId { get; set; }
            [Data] public byte AntiFormMultiplier { get; set; }
            [Data] public byte Padding { get; set; }
            [Data] public int SoundEffectStart { get; set; }
            [Data] public int SoundEffectFinish { get; set; }
            [Data] public int SoundEffectFail { get; set; }
        }

        public _Header Header { get; set; }
        public List<Function> Functions { get; set; }

        public enum Operation
        {
            CameraStart = 1,
            CameraComplete = 2,
            CameraFailed = 3,
            Timer = 4,
            Counter = 5,
            Gauge = 6, 
            ComboCounter = 7,
            MissionScore = 8,
            Watch = 9,
            LimitCost = 0xA,
            DriveRefillRatio = 0xB,
            AddDrive = 0xC,
            CameraPrize = 0xD
        }

        public class Function
        {
            public Operation Opcode { get; set; }
            public List<int> Parameters { get; set; }

            public override string ToString()
            {
                return $"{(int)Opcode:X02}({string.Join(", ", Parameters.Select(p => $"{p:X}"))})";
            }
        }

        private Mission(Stream stream)
        {
            Header = BinaryMapping.ReadObject<_Header>(stream);
            Functions = ParseScript(stream.ReadBytes((int)stream.Length - 0x1C - 4));
        }

        private List<Function> ParseScript(byte[] data)
        {
            var functions = new List<Function>();

            for (int pc = 0; pc < data.Length;)
            {
                var opcode = (ushort)(data[pc++] | (data[pc++] << 8));
                var parameterCount = (ushort)(data[pc++] | (data[pc++] << 8));
                var parameters = new List<int>(parameterCount);

                for (var i = 0; i < parameterCount; i++)
                {
                    var parameter = data[pc++] |
                        (data[pc++] << 8) |
                        (data[pc++] << 16) |
                        (data[pc++] << 24);
                    parameters.Add(parameter);
                }

                functions.Add(new Function
                {
                    Opcode = (Operation)opcode,
                    Parameters = parameters
                });
            }

            return functions;
        }

        public static Mission Read(Stream stream) => new Mission(stream);
    }
}
