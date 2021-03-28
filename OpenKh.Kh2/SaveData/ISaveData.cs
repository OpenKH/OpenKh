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

using System.IO;

namespace OpenKh.Kh2.SaveData
{
    public interface ISaveData
    {
        bool IsFinalMix { get; }

        uint MagicCode { get; set; }
        int Version { get; set; }
        uint Checksum { get; set; }
        byte WorldId { get; set; }
        byte RoomId { get; set; }
        byte SpawnId { get; set; }
        byte Unused0f { get; set; }
        IPlaceScript[] PlaceScripts { get; }
        SaveProgress[] StoryProgress { get; set; }

        byte[] RoomVisitedFlag { get; set; } // There might be a chance that it starts from 0x2300
        int MunnyAmount { get; set; }
        int Timer { get; set; }
        byte Difficulty { get; set; }
        byte[] PuzzlePieceFlags { get; set; }
        ICharacter[] Characters { get; }
        IDriveForm[] DriveForms { get; }

        PartyMembers[] WorldPartyMembers { get; set; }
        byte[] InventoryCount { get; set; }

        int Experience { get; set; }
        short ShortcutCircle { get; set; }
        short ShortcutTriangle { get; set; }
        short ShortcutSquare { get; set; }
        short ShortcutCross { get; set; }
        int BonusLevel { get; set; }

        bool Vibration { get; set; }
        bool Unknown41a4_1 { get; set; }
        bool Unknown41a4_2 { get; set; }
        bool NavigationalMap { get; set; }
        bool FieldCameraManual { get; set; }
        bool RightAnalogStickCommand { get; set; }
        bool CommandMenuClassic { get; set; }
        bool CameraLeftRightReversed { get; set; }
        bool CameraUpDownReversed { get; set; }
        bool Unknown41a5_1 { get; set; }
        bool Unknown41a5_2 { get; set; }

        short ProgressTutorialMenu { get; set; }
        bool NewStatusValor { get; set; }
        bool NewStatusWisdom { get; set; }
        bool NewStatusLimit { get; set; }
        bool NewStatusMaster { get; set; }
        bool NewStatusFinal { get; set; }
        bool NewStatusSummonStitch { get; set; }
        bool NewStatusSummonGenie { get; set; }
        bool NewStatusSummonPeterPan { get; set; }
        bool NewStatusSummonChickenLittle { get; set; }

        void Write(Stream stream);
    }
}
