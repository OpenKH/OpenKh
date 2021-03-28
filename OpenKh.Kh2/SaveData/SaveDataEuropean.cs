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
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SaveData
{
    public class SaveEuropean : ISaveData
    {
        public bool IsFinalMix => false;

        [Data(0, 0xb4e0)] public byte[] Data { get; set; }

        [Data(0)] public uint MagicCode { get; set; }
        [Data] public int Version { get; set; }
        [Data] public uint Checksum { get; set; }
        [Data] public byte WorldId { get; set; }
        [Data] public byte RoomId { get; set; }
        [Data] public byte SpawnId { get; set; }
        [Data] public byte Unused0f { get; set; }
        [Data(0x13, Count = 64 * 19, Stride = 3)] public PlaceScriptVanilla[] PlaceScripts { get; set; }
        [Data(0xe50, Count = 20, Stride = 0x20)] public SaveProgress[] StoryProgress { get; set; }
        [Data(0x14b8, Count = 8 * Constants.WorldCount)] public byte[] RoomVisitedFlag { get; set; }
        [Data(0x1600)] public int MunnyAmount { get; set; }
        [Data(0x1604, Count = Constants.WorldCount + 2)] public int Timer { get; set; }
        [Data(0x1658)] public byte Difficulty { get; set; }
        [Data(Count = 0)] public byte[] PuzzlePieceFlags { get; set; }
        [Data(0x1660, Count = 13, Stride = 0xf4)] public CharacterVanilla[] Characters { get; set; }
        [Data(0x22c4, Count = 9, Stride = 0x28)] public DriveFormVanilla[] DriveForms { get; set; }

        [Data(0x3534, Count = Constants.WorldCount)] public PartyMembers[] WorldPartyMembers { get; set; }
        [Data(0x2488, Count = 280)] public byte[] InventoryCount { get; set; }
        [Data(0x25E8)] public int Experience { get; set; }
        [Data(0x2600)] public short ShortcutCircle { get; set; }
        [Data] public short ShortcutTriangle { get; set; }
        [Data] public short ShortcutSquare { get; set; }
        [Data] public short ShortcutCross { get; set; }
        [Data] public int BonusLevel { get; set; }

        public bool Vibration { get; set; }
        public bool Unknown41a4_1 { get; set; }
        public bool Unknown41a4_2 { get; set; }
        public bool NavigationalMap { get; set; }
        public bool FieldCameraManual { get; set; }
        public bool RightAnalogStickCommand { get; set; }
        public bool CommandMenuClassic { get; set; }
        public bool CameraLeftRightReversed { get; set; }
        public bool CameraUpDownReversed { get; set; }
        public bool Unknown41a5_1 { get; set; }
        public bool Unknown41a5_2 { get; set; }
        public short ProgressTutorialMenu { get; set; }
        public bool NewStatusValor { get; set; }
        public bool NewStatusWisdom { get; set; }
        public bool NewStatusLimit { get; set; }
        public bool NewStatusMaster { get; set; }
        public bool NewStatusFinal { get; set; }
        public bool NewStatusSummonStitch { get; set; }
        public bool NewStatusSummonGenie { get; set; }
        public bool NewStatusSummonPeterPan { get; set; }
        public bool NewStatusSummonChickenLittle { get; set; }

        IPlaceScript[] ISaveData.PlaceScripts => PlaceScripts?.Cast<IPlaceScript>().ToArray() ?? new IPlaceScript[0];
        ICharacter[] ISaveData.Characters => Characters?.Cast<ICharacter>().ToArray() ?? new ICharacter[0];
        IDriveForm[] ISaveData.DriveForms => DriveForms?.Cast<IDriveForm>().ToArray() ?? new IDriveForm[0];

        public void Write(Stream stream) =>
            BinaryMapping.WriteObject(stream.FromBegin(), this);
    }

}
