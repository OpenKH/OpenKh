using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;
using OpenKh.Kh2.SystemData;
using FileAccess = Godot.FileAccess;

namespace OpenKh.Godot.Helpers
{
    public static class GlobalData
    {
        public static readonly Cmd Commands;
        
        //PREF
        //public static readonly List<Plyr> PlayerPreferences;
        public static readonly List<Fmab> FormAbilitiesPreferences;
        public static readonly List<Prty> PartyPreferences;
        public static readonly List<Sstm> SystemPreferences;
        public static readonly List<Magi> MagicPreferences;

        static GlobalData()
        {
            var systemData = Bar.Read(new MemoryStream(FileAccess.GetFileAsBytes("res://Imported/kh2/03system.bin")));

            var preferences = systemData.FirstOrDefault(i => i.Name.Equals("pref", System.StringComparison.OrdinalIgnoreCase));
            if (preferences is not null)
            {
                var preferencesBar = Bar.Read(preferences.Stream);
                //TODO
            }
        }
    }
}
