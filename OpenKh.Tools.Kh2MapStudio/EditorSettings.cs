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

        public static bool ViewCollision
        {
            get => Settings.Default.ViewCollision;
            set
            {
                Settings.Default.ViewCollision = value;
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

        public static bool ViewEventScript
        {
            get => Settings.Default.ViewEventScript;
            set
            {
                Settings.Default.ViewEventScript = value;
                Settings.Default.Save();
            }
        }

        public static bool SeparateCamera
        {
            get => Settings.Default.SeparateCamera;
            set
            {
                Settings.Default.SeparateCamera = value;
                Settings.Default.Save();
            }
        }
        public static float OpacityLevel
        {
            get => Settings.Default.OpacityLevel;
            set
            {
                Settings.Default.OpacityLevel = value;
                Settings.Default.Save();
            }
        }

        public static float RedValue
        {
            get => Settings.Default.RedValue;
            set
            {
                Settings.Default.RedValue = value;
                Settings.Default.Save();
            }
        }

        public static float GreenValue
        {
            get => Settings.Default.GreenValue;
            set
            {
                Settings.Default.GreenValue = value;
                Settings.Default.Save();
            }
        }

        public static float BlueValue
        {
            get => Settings.Default.BlueValue;
            set
            {
                Settings.Default.BlueValue = value;
                Settings.Default.Save();
            }
        }

        public static float OpacityEntranceLevel
        {
            get => Settings.Default.OpacityEntranceLevel;
            set
            {
                Settings.Default.OpacityEntranceLevel = value;
                Settings.Default.Save();
            }
        }

        public static float RedValueEntrance
        {
            get => Settings.Default.RedValueEntrance;
            set
            {
                Settings.Default.RedValueEntrance = value;
                Settings.Default.Save();
            }
        }

        public static float GreenValueEntrance
        {
            get => Settings.Default.GreenValueEntrance;
            set
            {
                Settings.Default.GreenValueEntrance = value;
                Settings.Default.Save();
            }
        }

        public static float BlueValueEntrance
        {
            get => Settings.Default.BlueValueEntrance;
            set
            {
                Settings.Default.BlueValueEntrance = value;
                Settings.Default.Save();
            }
        }

        public static int InitialWindowWidth
        {
            get => Settings.Default.InitialWindowWidth;
            set
            {
                Settings.Default.InitialWindowWidth = value;
                Settings.Default.Save();

            }
        }

        public static int InitialWindowHeight
        {
            get => Settings.Default.InitialWindowHeight;
            set
            {
                Settings.Default.InitialWindowHeight = value;
                Settings.Default.Save();

            }
        }

    }
}
