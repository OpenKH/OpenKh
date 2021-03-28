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

using Xe.BinaryMapper;

namespace OpenKh.Kh2.SaveData
{
    public interface IDriveForm
    {
        short Weapon { get; set; }
        byte Level { get; set; }
        byte AbilityLevel { get; set; }
        int Experience { get; set; }
        ushort[] Abilities { get; set; }
    }

    public class DriveFormVanilla : IDriveForm
    {
        [Data(0)] public short Weapon { get; set; }
        [Data] public byte Level { get; set; }
        [Data] public byte AbilityLevel { get; set; }
        [Data] public int Experience { get; set; }
        [Data(Count = 0x10)] public ushort[] Abilities { get; set; }
    }

    public class DriveFormFinalMix : IDriveForm
    {
        [Data(0)] public short Weapon { get; set; }
        [Data] public byte Level { get; set; }
        [Data] public byte AbilityLevel { get; set; }
        [Data] public int Experience { get; set; }
        [Data(Count = 0x18)] public ushort[] Abilities { get; set; }
    }
}
