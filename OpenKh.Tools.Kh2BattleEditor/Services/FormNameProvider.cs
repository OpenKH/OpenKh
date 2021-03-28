using System.Collections.Generic;

namespace OpenKh.Tools.Kh2BattleEditor.Services
{
    public static class FormNameProvider
    {
        private static readonly Dictionary<int, string> _formNamesVanilla = new Dictionary<int, string>
        {
            [0x00] = "Summon",
            [0x01] = "Valor",
            [0x02] = "Wisdom",
            [0x03] = "Master",
            [0x04] = "Final",
            [0x05] = "Anti",
        };

        private static readonly Dictionary<int, string> _formNamesFm = new Dictionary<int, string>
        {
            [0x00] = "Summon",
            [0x01] = "Valor",
            [0x02] = "Wisdom",
            [0x03] = "Limit",
            [0x04] = "Master",
            [0x05] = "Final",
            [0x06] = "Anti",
        };

        public static string GetFormName(int id, bool fm)
        {
            string name;
            if (fm)
                _formNamesFm.TryGetValue(id, out name);
            else
                _formNamesVanilla.TryGetValue(id, out name);

            return name;
        }
    }
}
