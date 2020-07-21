namespace OpenKh.Kh2
{
    public enum World
    {
        WorldZz,
        EndOfSea,
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

    public enum JiminyWorld
    {
        TwilightTown,
        HollowBastion,
        BeastCastle,
        OlympusColiseum,
        Agrabah,
        LandOfDragons,
        HundredAcreWood,
        PrideLands,
        Atlantica,
        DisneyCastle,
        TimelessRiver,
        HalloweenTown,
        PortRoyal,
        SpaceParanoids,
        WorldThatNeverWas,
        Collection,

        //used in the character section
        Other = 0x10,
        Heartless = 0x11,
        Nobody = 0x12,
    }

    public static class Constants
    {
        public const int FontEuropeanSystemWidth = 18;
        public const int FontEuropeanSystemHeight = 24;
        public const int FontEuropeanEventWidth = 24;
        public const int FontEuropeanEventHeight = 32;
        public const int FontJapaneseSystemWidth = 18;
        public const int FontJapaneseSystemHeight = 18;
        public const int FontJapaneseEventWidth = 24;
        public const int FontJapaneseEventHeight = 24;
        public const int FontTableSystemHeight = 256;
        public const int FontTableEventHeight = 512;
        public const int FontIconWidth = 24;
        public const int FontIconHeight = 24;
        public const int RegionFinalMix = 7;

        public const int PaletteCount = 9;
        public const int WorldCount = (int)World.WorldThatNeverWas + 1;
        public const int JiminyWorldCount = (int)JiminyWorld.Nobody + 1;

        public static readonly string[] WorldIds = new string[WorldCount]
        {
            "zz", "es", "tt", "di", "hb", "bb", "he", "al",
            "mu", "po", "lk", "lm", "dc", "wi", "nm", "wm",
            "ca", "tr", "eh"
        };

        public static readonly string[] Regions = new string[]
        {
            "jp", "us", "uk", "it", "sp", "gr", "fr", "fm"
        };

        public static readonly string[] WorldNames = new string[WorldCount]
        {
            "World ZZ",
            "End of Sea",
            "Twilight Town",
            "Destiny Islands",
            "Hollow Bastion",
            "Beast's Castle",
            "Olympus Coliseum",
            "Agrabah",
            "The Land of Dragons",
            "100 Acre Wood",
            "Pride Lands",
            "Atlantica",
            "Disney Castle",
            "Timeless River",
            "Halloween Town",
            "World Map",
            "Port Royal",
            "Space Paranoids",
            "World That Never Was"
        };
    }
}
