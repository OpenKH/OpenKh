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
    public class SaveProgress
    {
        [Data(Count = 0x20)] public byte[] Flags { get; set; }

        public bool GetFlag(int index) => (Flags[index / 8] & (1 << (index % 8))) != 0;
        public void SetFlag(int index, bool value)
        {
            var mask = (byte)(1 << (index % 8));
            if (value)
                Flags[index / 8] |= mask;
            else
                Flags[index / 8] &= (byte)~mask;
        }
    }
}
