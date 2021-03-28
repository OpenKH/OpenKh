using System.Collections.Generic;

namespace OpenKh.Tools.Kh2BattleEditor.Services
{
    public static class CharacterNameProvider
    {
        private static readonly Dictionary<int, string> _characterNames = new Dictionary<int, string>
        {
            [0x01] = "Sora",
            [0x02] = "Donald",
            [0x03] = "Goofy",
            [0x04] = "Mickey",
            [0x05] = "Auron",
            [0x06] = "Mulan",
            [0x07] = "Aladdin",
            [0x08] = "Jack Sparrow",
            [0x09] = "Beast",
            [0x0A] = "Jack",
            [0x0B] = "Simba",
            [0x0C] = "Tron",
            [0x0D] = "Riku",
            [0x0E] = "Roxas",
            [0x0F] = "Ping",
        };

        public static string GetCharacterName(int id)
        {
            _characterNames.TryGetValue(id, out var name);

            return name;
        }
    }
}
