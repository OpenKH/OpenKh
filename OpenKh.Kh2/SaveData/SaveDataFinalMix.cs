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
    public class SaveFinalMix : ISaveData
    {
        public bool IsFinalMix => true;

        [Data(0, 0x10FC0)] public byte[] Data { get; set; }

        [Data(0)] public uint MagicCode { get; set; }
        [Data] public int Version { get; set; }
        [Data] public uint Checksum { get; set; }
        [Data] public byte WorldId { get; set; }
        [Data] public byte RoomId { get; set; }
        [Data] public byte SpawnId { get; set; }
        [Data] public byte Unused0f { get; set; }
        [Data(0x10, Count = 64 * 19, Stride = 6)] public PlaceScriptFinalMix[] PlaceScripts { get; set; }
        [Data(0x1c90, Count = 20, Stride = 0x20)] public SaveProgress[] StoryProgress { get; set; }
        // 3e8 of what?
        [Data(0x22f8, Count = 8 * Constants.WorldCount)] public byte[] RoomVisitedFlag { get; set; } // There might be a chance that it starts from 0x2300
        [Data(0x2440)] public int MunnyAmount { get; set; }
        [Data(0x2444, Count = Constants.WorldCount + 2)] public int Timer { get; set; }
        [Data(0x2498)] public byte Difficulty { get; set; }
        [Data(0x24a0, Count = 0x30)] public byte[] PuzzlePieceFlags { get; set; }
        [Data(0x24f0, Count = 13, Stride = 0x114)] public CharacterFinalMix[] Characters { get; set; }
        [Data(0x32f4, Count = 10, Stride = 0x38)] public DriveFormFinalMix[] DriveForms { get; set; }

        [Data(0x3526)] public byte SummonLevel { get; set; }
        [Data(0x3529)] public byte DriveBarCurrent { get; set; }
        [Data(0x352a)] public byte DriveBarMax { get; set; }

        [Data(0x3534, Count = Constants.WorldCount)] public PartyMembers[] WorldPartyMembers { get; set; }
        [Data(0x3580, Count = 320)] public byte[] InventoryCount { get; set; }

        [Data(0x36E0)] public int Experience { get; set; }
        [Data(0x36f8)] public short ShortcutCircle { get; set; }
        [Data(0x36fa)] public short ShortcutTriangle { get; set; }
        [Data(0x36fc)] public short ShortcutSquare { get; set; }
        [Data(0x36fe)] public short ShortcutCross { get; set; }
        [Data(0x3700)] public int BonusLevel { get; set; }

        [Data(0x41a4, BitIndex = 0)] public bool Vibration { get; set; }
        [Data(0x41a4, BitIndex = 1)] public bool Unknown41a4_1 { get; set; }
        [Data(0x41a4, BitIndex = 2)] public bool Unknown41a4_2 { get; set; }
        [Data(0x41a4, BitIndex = 3)] public bool NavigationalMap { get; set; }
        [Data(0x41a4, BitIndex = 4)] public bool FieldCameraManual { get; set; }
        [Data(0x41a4, BitIndex = 5)] public bool RightAnalogStickCommand { get; set; }
        [Data(0x41a4, BitIndex = 6)] public bool CommandMenuClassic { get; set; }
        [Data(0x41a4, BitIndex = 7)] public bool CameraLeftRightReversed { get; set; }
        [Data(0x41a5, BitIndex = 0)] public bool CameraUpDownReversed { get; set; }
        [Data(0x41a5, BitIndex = 1)] public bool Unknown41a5_1 { get; set; }
        [Data(0x41a5, BitIndex = 2)] public bool Unknown41a5_2 { get; set; }

        // WRONG.
        // [Data(0x41aa, BitIndex = 0)] public bool NewMagicUnk1_0 { get; set; }
        // [Data(0x41aa, BitIndex = 1)] public bool NewMagicUnk1_1 { get; set; }
        // [Data(0x41aa, BitIndex = 2)] public bool NewMagicUnk1_2 { get; set; }
        // [Data(0x41aa, BitIndex = 3)] public bool NewMagicUnk1_4 { get; set; }
        // [Data(0x41aa, BitIndex = 4)] public bool NewMagicUnk1_8 { get; set; }
        // [Data(0x41aa, BitIndex = 5)] public bool NewMagicFiraga { get; set; }
        // [Data(0x41aa, BitIndex = 6)] public bool NewMagicBlizzaga { get; set; }
        // [Data(0x41aa, BitIndex = 7)] public bool NewMagicThundaga { get; set; }
        // [Data(0x41ab, BitIndex = 0)] public bool NewMagicCuraga { get; set; }
        // [Data(0x41ab, BitIndex = 1)] public bool NewMagicUnkUnk { get; set; }
        // [Data(0x41b2, BitIndex = 0)] public bool NewMagicUnk2_0 { get; set; }
        // [Data(0x41b2, BitIndex = 1)] public bool NewMagicUnk2_1 { get; set; }
        // [Data(0x41b2, BitIndex = 2)] public bool NewMagicUnk2_2 { get; set; }
        // [Data(0x41b2, BitIndex = 3)] public bool NewMagicUnk2_4 { get; set; }
        // [Data(0x41b2, BitIndex = 4)] public bool NewMagicUnk2_8 { get; set; }
        // [Data(0x41b2, BitIndex = 5)] public bool NewMagicUnk2_10 { get; set; }
        // [Data(0x41b2, BitIndex = 6)] public bool NewMagicUnk2_20 { get; set; }
        // [Data(0x41b2, BitIndex = 7)] public bool NewMagicMagnega { get; set; }
        // [Data(0x41b3)] public bool NewMagicReflega { get; set; }

        [Data(0x4270)] public short ProgressTutorialMenu { get; set; }
        [Data(0x4274)] public bool NewStatusValor { get; set; }
        [Data(0x4274)] public bool NewStatusWisdom { get; set; }
        [Data(0x4274)] public bool NewStatusLimit { get; set; }
        [Data(0x4274)] public bool NewStatusMaster { get; set; }
        [Data(0x4274)] public bool NewStatusFinal { get; set; }
        [Data(0x4274)] public bool NewStatusSummonStitch { get; set; }
        [Data(0x4274)] public bool NewStatusSummonGenie { get; set; }
        [Data(0x4274)] public bool NewStatusSummonPeterPan { get; set; }
        [Data(0x4275)] public bool NewStatusSummonChickenLittle { get; set; }

        IPlaceScript[] ISaveData.PlaceScripts => PlaceScripts?.Cast<IPlaceScript>().ToArray() ?? new IPlaceScript[0];
        ICharacter[] ISaveData.Characters => Characters?.Cast<ICharacter>().ToArray() ?? new ICharacter[0];
        IDriveForm[] ISaveData.DriveForms => DriveForms?.Cast<IDriveForm>().ToArray() ?? new IDriveForm[0];

        public void Write(Stream stream) =>
            BinaryMapping.WriteObject(stream.FromBegin(), this);
    }
}
