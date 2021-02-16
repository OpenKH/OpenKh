namespace OpenKh.Game.States.Title
{
    internal class Constants
    {
        public static readonly TitleLayout VanillaTitleLayout = new TitleLayout
        {
            Copyright = 8,
            Intro = 11,
            IntroSkip = 12,
            NewGame = 13,
            MenuOptionNewGame = 0,
            MenuOptionLoad = 1,
            MenuOptionTheater = 2,
            MenuOptionBack = 3,
            HasTheater = false,
            HasBack = false,
        };

        public static readonly TitleLayout FinalMixTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 12,
            IntroSkip = 14,
            NewGame = 10, // but also 15?
            MenuOptionNewGame = 0,
            MenuOptionLoad = 1,
            MenuOptionTheater = 4,
            HasTheater = false,
            HasBack = false,
        };

        public static readonly TitleLayout FinalMixTheaterTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 13,
            IntroSkip = 14,
            NewGame = 11, // but also 16?
            MenuOptionNewGame = 2,
            MenuOptionLoad = 3,
            MenuOptionTheater = 4,
            HasTheater = true,
            HasBack = false,
        };

        public static readonly TitleLayout ReMixTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 12,
            IntroSkip = 14,
            NewGame = 10, // but also 15?
            MenuOptionNewGame = 0,
            MenuOptionLoad = 1,
            MenuOptionTheater = 4,
            MenuOptionBack = 18,
            HasTheater = false,
            HasBack = true,
        };

        public static readonly TitleLayout ReMixTheaterTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 13,
            IntroSkip = 14,
            NewGame = 11, // but also 16?
            MenuOptionNewGame = 2,
            MenuOptionLoad = 3,
            MenuOptionTheater = 4,
            MenuOptionBack = 19,
            HasTheater = true,
            HasBack = true,
        };
    }
}
