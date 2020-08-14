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

        public static bool ViewCamera
        {
            get => Settings.Default.ViewCamera;
            set
            {
                Settings.Default.ViewCamera = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewLayerControl
        {
            get => Settings.Default.ViewLayerControl;
            set
            {
                Settings.Default.ViewLayerControl = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewSpawnPoint
        {
            get => Settings.Default.ViewSpawnPoint;
            set
            {
                Settings.Default.ViewSpawnPoint = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewMeshGroup
        {
            get => Settings.Default.ViewMeshGroup;
            set
            {
                Settings.Default.ViewMeshGroup = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewBobDescriptor
        {
            get => Settings.Default.ViewBobDescriptors;
            set
            {
                Settings.Default.ViewBobDescriptors = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewSpawnScriptMap
        {
            get => Settings.Default.ViewSpawnScriptMap;
            set
            {
                Settings.Default.ViewSpawnScriptMap = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewSpawnScriptBattle
        {
            get => Settings.Default.ViewSpawnScriptBattle;
            set
            {
                Settings.Default.ViewSpawnScriptBattle = value;
                Settings.Default.Save();
            }
        }

        public static bool ViewSpawnScriptEvent
        {
            get => Settings.Default.ViewSpawnScriptEvent;
            set
            {
                Settings.Default.ViewSpawnScriptEvent = value;
                Settings.Default.Save();
            }
        }
    }
}
