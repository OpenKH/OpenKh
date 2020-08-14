namespace OpenKh.Tools.Kh2MapStudio
{
    static class EditorSettings
    {
        public static float MoveSpeed
        {
            get => Settings.Default.MoveSpeed;
            set
            {
                Settings.Default.MoveSpeed = value;
                Settings.Default.Save();
            }
        }

        public static float MoveSpeedShift
        {
            get => Settings.Default.MoveSpeedShift;
            set
            {
                Settings.Default.MoveSpeedShift = value;
                Settings.Default.Save();
            }
        }
    }
}
