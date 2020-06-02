using System.Collections.Generic;

namespace OpenKh.Tools.Kh2BattleEditor.Services
{
    public static class EnemyNameProvider
    {
        private static readonly Dictionary<int, string> _enemyNames = new Dictionary<int, string>
        {
            [0x0B] = "Morning Star",
            [0xA1] = "Luxord Data cards",
            [0xC8] = "Vexen Data",
            [0xD6] = "Lexaeus Data",
            [0xD7] = "Marluxia Data",
            [0xD9] = "Zexion Data",
            [0xDA] = "Vexen Anti Sora LV1",
            [0xDB] = "Vexen Anti Sora LV2",
            [0xDC] = "Vexen Anti Sora LV3",
            [0xDD] = "Vexen Anti Sora LV4",
            [0xDE] = "Vexen Anti Sora LV5",
            [0xDF] = "Zexion Data book illusion",
            [0xE0] = "Zexion Data book trap",
            [0xEF] = "Xemnas Data",
            [0xF0] = "Demyx Data",
            [0xF1] = "Demyx Data minions",
            [0xF2] = "Roxas Data",
            [0xF3] = "Luxord Data",
            [0xF4] = "Terra",
            [0xF5] = "Axel Data",
            [0xF6] = "Xaldin Data",
            [0xFC] = "Xigbar Data",
            [0xFD] = "Saix Data",
        };

        public static string GetEnemyName(int id)
        {
            _enemyNames.TryGetValue(id, out var name);
            
            return name;
        }
    }
}
