namespace OpenKh.Tools.Kh2MsetMotionEditor
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

        public static bool ViewCamera
        {
            get => Settings.Default.ViewCamera;
            set
            {
                Settings.Default.ViewCamera = value;
                Settings.Default.Save();
            }
        }
    }
}
