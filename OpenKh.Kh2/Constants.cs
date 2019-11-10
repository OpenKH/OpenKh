namespace OpenKh.Kh2
{
    public static class Constants
    {
        public const int FontEuropeanSystemWidth = 18;
        public const int FontEuropeanSystemHeight = 24;
        public const int FontEuropeanEventWidth = 24;
        public const int FontEuropeanEventHeight = 32;
        public const int FontIconWidth = 24;
        public const int FontIconHeight = 24;

        public const int WorldCount = (int)Worlds.WorldThatNeverWas + 1;

        public static readonly string[] WorldIds = new string[WorldCount]
        {
            "zz", "es", "tt", "di", "hb", "bb", "he", "al",
            "mu", "po", "lk", "lm", "dc", "wi", "nm", "wm",
            "ca", "tr", "eh"
        };

        public enum Worlds
        {
            Debug,
            DarkRealm,
            TwilightTown,
            DestinyIsland,
            HollowBastion,
            BeastCastle,
            TheUnderworld,
            Agrabah,
            LandOfDragons,
            HundredAcreWood,
            PrideLands,
            Atlantica,
            DisneyCastle,
            TimelessRiver,
            HalloweenTown,
            WorldMap,
            PortRoyal,
            SpaceParanoids,
            WorldThatNeverWas
        }
    }
}
