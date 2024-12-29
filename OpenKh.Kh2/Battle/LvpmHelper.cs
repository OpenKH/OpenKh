using System.Collections.Generic;

namespace OpenKh.Kh2.Battle
{
    public class LvpmHelper
    {
        public ushort Level { get; set; } //Dummy ID to make lvpm listpatchable
        public ushort HpMultiplier { get; set; } // (Hp * HpMultiplier + 99) / 100
        public ushort Strength { get; set; }
        public ushort Defense { get; set; }
        public ushort MaxStrength { get; set; }
        public ushort MinStrength { get; set; }
        public ushort Experience { get; set; }

        public LvpmHelper() { }

        public LvpmHelper(ushort level, ushort hpMultiplier, ushort strength, ushort defense, ushort maxStrength, ushort minStrength, ushort experience)
        {
            Level = level;
            HpMultiplier = hpMultiplier;
            Strength = strength;
            Defense = defense;
            MaxStrength = maxStrength;
            MinStrength = minStrength;
            Experience = experience;
        }

        public static Lvpm ConvertLvpmHelperToLvpm(LvpmHelper lvl)
        {
            return new Lvpm(lvl.HpMultiplier, lvl.Strength, lvl.Defense, lvl.MaxStrength, lvl.MinStrength, lvl.Experience);
        }

        public static List<LvpmHelper> ConvertLvpmListToHelper(List<Lvpm> lvpmList)
        {
            ushort Id = 0;
            var helperList = new List<LvpmHelper>();
            foreach (var lvpm in lvpmList)
            {
                helperList.Add(new LvpmHelper(Id, lvpm.HpMultiplier, lvpm.Strength, lvpm.Defense, lvpm.MaxStrength, lvpm.MinStrength, lvpm.Experience));
                Id++;
            }
            return helperList;
        }
    }
}
