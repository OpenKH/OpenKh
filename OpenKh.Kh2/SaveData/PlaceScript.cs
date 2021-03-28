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
    public interface IPlaceScript
    {
        byte Map { get; set; }
        byte Battle { get; set; }
        byte Event { get; set; }
    }
    public class PlaceScriptVanilla : IPlaceScript
    {
        [Data] public byte Map { get; set; }
        [Data] public byte Battle { get; set; }
        [Data] public byte Event { get; set; }
    }

    public class PlaceScriptFinalMix : IPlaceScript
    {
        [Data] public byte Map { get; set; }
        [Data] public byte MapSecondary { get; set; }
        [Data] public byte Battle { get; set; }
        [Data] public byte BattleSecondary { get; set; }
        [Data] public byte Event { get; set; }
        [Data] public byte EventSecondary { get; set; }
    }
}
