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
    public interface ICharacter
    {
        short Weapon { get; set; }
        short Unk02 { get; set; }
        byte HpCur { get; set; }
        byte HpMax { get; set; }
        byte MpCur { get; set; }
        byte MpMax { get; set; }
        byte ApBoost { get; set; }
        byte StrengthBoost { get; set; }
        byte MagicBoost { get; set; }
        byte DefenseBoost { get; set; }
        byte Unk0c { get; set; }
        byte Unk0d { get; set; }
        byte Unk0e { get; set; }
        byte Level { get; set; }
        byte ArmorCount { get; set; }
        byte AccessoryCount { get; set; }
        byte ItemCount { get; set; }
        byte UnknownCount { get; set; }
        short[] Armors { get; set; }
        short[] Accessories { get; set; }
        short[] Items { get; set; }
        short[] ItemAutoReload { get; set; }

        ushort[] Abilities { get; set; }

        byte BattleStyle { get; set; }
        byte AbilityStyle1 { get; set; }
        byte AbilityStyle2 { get; set; }
        byte AbilityStyle3 { get; set; }
        byte AbilityStyle4 { get; set; }
    }

    public class CharacterVanilla : ICharacter
    {
        [Data(Count = 0xf4)] public byte[] Data { get; set; }

        [Data(0)] public short Weapon { get; set; }
        [Data] public short Unk02 { get; set; }
        [Data] public byte HpCur { get; set; }
        [Data] public byte HpMax { get; set; }
        [Data] public byte MpCur { get; set; }
        [Data] public byte MpMax { get; set; }
        [Data] public byte ApBoost { get; set; }
        [Data] public byte StrengthBoost { get; set; }
        [Data] public byte MagicBoost { get; set; }
        [Data] public byte DefenseBoost { get; set; }
        [Data] public byte Unk0c { get; set; }
        [Data] public byte Unk0d { get; set; }
        [Data] public byte Unk0e { get; set; }
        [Data] public byte Level { get; set; }
        [Data] public byte ArmorCount { get; set; }
        [Data] public byte AccessoryCount { get; set; }
        [Data] public byte ItemCount { get; set; }
        [Data] public byte UnknownCount { get; set; }
        [Data(Count = 8)] public short[] Armors { get; set; }
        [Data(Count = 8)] public short[] Accessories { get; set; }
        [Data(Count = 8)] public short[] Items { get; set; }
        [Data(Count = 8)] public short[] ItemAutoReload { get; set; }
        [Data(0x54, Count = 0x30)] public ushort[] Abilities { get; set; }
        [Data] public byte BattleStyle { get; set; }
        [Data] public byte AbilityStyle1 { get; set; }
        [Data] public byte AbilityStyle2 { get; set; }
        [Data] public byte AbilityStyle3 { get; set; }
        [Data] public byte AbilityStyle4 { get; set; }
    }

    public class CharacterFinalMix : ICharacter
    {
        [Data(Count = 0x114)] public byte[] Data { get; set; }

        [Data(0)] public short Weapon { get; set; }
        [Data] public short Unk02 { get; set; }
        [Data] public byte HpCur { get; set; }
        [Data] public byte HpMax { get; set; }
        [Data] public byte MpCur { get; set; }
        [Data] public byte MpMax { get; set; }
        [Data] public byte ApBoost { get; set; }
        [Data] public byte StrengthBoost { get; set; }
        [Data] public byte MagicBoost { get; set; }
        [Data] public byte DefenseBoost { get; set; }
        [Data] public byte Unk0c { get; set; }
        [Data] public byte Unk0d { get; set; }
        [Data] public byte Unk0e { get; set; }
        [Data] public byte Level { get; set; }
        [Data] public byte ArmorCount { get; set; }
        [Data] public byte AccessoryCount { get; set; }
        [Data] public byte ItemCount { get; set; }
        [Data] public byte UnknownCount { get; set; }
        [Data(Count = 8)] public short[] Armors { get; set; }
        [Data(Count = 8)] public short[] Accessories { get; set; }
        [Data(Count = 8)] public short[] Items { get; set; }
        [Data(Count = 8)] public short[] ItemAutoReload { get; set; }
        [Data(0x54, Count = 0x50)] public ushort[] Abilities { get; set; }
        [Data] public byte BattleStyle { get; set; }
        [Data] public byte AbilityStyle1 { get; set; }
        [Data] public byte AbilityStyle2 { get; set; }
        [Data] public byte AbilityStyle3 { get; set; }
        [Data] public byte AbilityStyle4 { get; set; }
    }
}
