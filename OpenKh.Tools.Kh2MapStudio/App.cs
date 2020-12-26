using Assimp;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using OpenKh.Kh2;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MapStudio.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MapStudio
{
    class App : IDisposable
    {
        private static readonly List<FileDialogFilter> MapFilter =
            FileDialogFilterComposer.Compose()
            .AddExtensions("MAP file", "map")
            .AddAllFiles();
        private static readonly List<FileDialogFilter> ArdFilter =
            FileDialogFilterComposer.Compose()
            .AddExtensions("ARD file", "ard")
            .AddAllFiles();
        private static readonly List<FileDialogFilter> ModelFilter =
            FileDialogFilterComposer.Compose()
            .AddExtensions("glTF file (GL Transmission Format)", "gltf")
            .AddExtensions("FBX file", "fbx")
            .AddExtensions("DAE file (Collada)  (might be unaccurate)", "dae")
            .AddExtensions("OBJ file (Wavefront)  (might lose some information)", "obj")
            .AddAllFiles();

        private readonly Vector4 BgUiColor = new Vector4(0.0f, 0.0f, 0.0f, 0.5f);
        private readonly MonoGameImGuiBootstrap _bootstrap;
        private bool _exitFlag = false;

        private readonly Dictionary<Keys, Action> _keyMapping = new Dictionary<Keys, Action>();
        private readonly MapRenderer _mapRenderer;
        private string _gamePath;
        private string _mapName;
        private string _region;
        private string _ardPath;
        private string _mapPath;
        private string _objPath;
        private List<string> _mapList = new List<string>();
        private ObjEntryController _objEntryController;

        private xna.Point _previousMousePosition;

        public string Title
        {
            get
            {
                var mapName = _mapName != null ? $"{_mapName}@" : string.Empty;
                return $"{mapName}{_gamePath ?? "unloaded"} | {MonoGameImGuiBootstrap.ApplicationName}";
            }
        }

        private string GamePath
        {
            get => _gamePath;
            set
            {
                _gamePath = value;
                UpdateTitle();
                EnumerateMapList();

                _objEntryController?.Dispose();
                _objEntryController = new ObjEntryController(
                    _bootstrap.GraphicsDevice,
                    _objPath,
                    Path.Combine(_gamePath, "00objentry.bin"));
                _mapRenderer.ObjEntryController = _objEntryController;

                Settings.Default.GamePath = value;
                Settings.Default.Save();

            }
        }

        private string MapName
        {
            get => _mapName;
            set
            {
                _mapName = value;
                UpdateTitle();

                _mapRenderer.Close();
                _mapRenderer.OpenMap(Path.Combine(_mapPath, $"{_mapName}.map"));
                _mapRenderer.OpenArd(Path.Combine(_ardPath, $"{_mapName}.ard"));
            }
        }

        private bool IsGameOpen => !string.IsNullOrEmpty(_gamePath);
        private bool IsMapOpen => !string.IsNullOrEmpty(_mapName);
        private bool IsOpen => IsGameOpen && IsMapOpen;

        public App(MonoGameImGuiBootstrap bootstrap, string gamePath = null)
        {
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;
            _mapRenderer = new MapRenderer(bootstrap.Content, bootstrap.GraphicsDeviceManager);
            AddKeyMapping(Keys.O, MenuFileOpen);
            AddKeyMapping(Keys.S, MenuFileSave);
            AddKeyMapping(Keys.Q, MenuFileUnload);

            if (string.IsNullOrEmpty(gamePath))
                gamePath = Settings.Default.GamePath;

            if (!string.IsNullOrEmpty(gamePath))
                OpenFolder(gamePath);

            ImGui.PushStyleColor(ImGuiCol.MenuBarBg, BgUiColor);
        }

        public bool MainLoop()
        {
            _bootstrap.GraphicsDevice.Clear(xna.Color.CornflowerBlue);
            ProcessKeyMapping();
            if (!_bootstrap.ImGuiWantTextInput) ProcessKeyboardInput(Keyboard.GetState(), 1f / 60);
            if (!_bootstrap.ImGuiWantCaptureMouse) ProcessMouseInput(Mouse.GetState());

            ImGui.PushStyleColor(ImGuiCol.WindowBg, BgUiColor);
            ForControl(ImGui.BeginMainMenuBar, ImGui.EndMainMenuBar, MainMenu);

            MainWindow();

            ForWindow("Tools", () =>
            {
                if (EditorSettings.ViewCamera) CameraWindow.Run(_mapRenderer.Camera);
                if (EditorSettings.ViewLayerControl) LayerControllerWindow.Run(_mapRenderer);
                if (EditorSettings.ViewSpawnPoint) SpawnPointWindow.Run(_mapRenderer);
                if (EditorSettings.ViewMeshGroup) MeshGroupWindow.Run(_mapRenderer.MapMeshGroups);
                if (EditorSettings.ViewBobDescriptor) BobDescriptorWindow.Run(_mapRenderer.BobDescriptors, _mapRenderer.BobMeshGroups.Count);
                if (EditorSettings.ViewSpawnScriptMap) SpawnScriptWindow.Run("map", _mapRenderer.SpawnScriptMap);
                if (EditorSettings.ViewSpawnScriptBattle) SpawnScriptWindow.Run("btl", _mapRenderer.SpawnScriptBattle);
                if (EditorSettings.ViewSpawnScriptEvent) SpawnScriptWindow.Run("evt", _mapRenderer.SpawnScriptEvent);
            });

            ImGui.PopStyleColor();

            return _exitFlag;
        }

        public void Dispose()
        {
            _objEntryController?.Dispose();
        }

        private void MainWindow()
        {
            if (!IsGameOpen)
            {
                ImGui.Text("Game content not loaded.");
                return;
            }

            ForControl(() =>
            {
                var nextPos = ImGui.GetCursorPos();
                var ret = ImGui.Begin("MapList",
                    ImGuiWindowFlags.NoDecoration |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoMove);
                ImGui.SetWindowPos(nextPos);
                ImGui.SetWindowSize(new Vector2(64, 0));
                return ret;
            }, () => { }, () =>
            {
                foreach (var map in _mapList)
                {
                    if (ImGui.Selectable(map, MapName == map))
                    {
                        MapName = map;
                    }
                }
            });
            ImGui.SameLine();

            if (!IsMapOpen)
            {
                ImGui.Text("Please select a map to edit.");
                return;
            }

            _mapRenderer.Update(1f / 60);
            _mapRenderer.Draw();
        }

        void MainMenu()
        {
            ForMenuBar(() =>
            {
                ForMenu("File", () =>
                {
                    ForMenuItem("Open extracted game folder...", "CTRL+O", MenuFileOpen);
                    ForMenuItem("Unload current map+ard", "CTRL+Q", MenuFileUnload, IsOpen);
                    ForMenuItem("Import extern MAP file", MenuFileOpenMap, IsGameOpen);
                    ForMenuItem("Import extern ARD file", MenuFileOpenArd, IsGameOpen);
                    ForMenuItem("Save map+ard", "CTRL+S", MenuFileSave, IsOpen);
                    ForMenuItem("Save map as...", MenuFileSaveMapAs, IsOpen);
                    ForMenuItem("Save ard as...", MenuFileSaveArdAs, IsOpen);
                    ImGui.Separator();
                    ForMenu("Export", () =>
                    {
                        ForMenuItem("Map Collision", ExportMapCollision, _mapRenderer.ShowMapCollision.HasValue);
                        ForMenuItem("Camera Collision", ExportCameraCollision, _mapRenderer.ShowCameraCollision.HasValue);
                        ForMenuItem("Light Collision", ExportLightCollision, _mapRenderer.ShowLightCollision.HasValue);
                    });
                    ImGui.Separator();
                    ForMenu("Preferences", () =>
                    {
                        ForEdit("Movement speed", () => EditorSettings.MoveSpeed, x => EditorSettings.MoveSpeed = x);
                        ForEdit("Movement speed (shift)", () => EditorSettings.MoveSpeedShift, x => EditorSettings.MoveSpeedShift = x);
                    });
                    ImGui.Separator();
                    ForMenuItem("Exit", MenuFileExit);
                });
                ForMenu("View", () =>
                {
                    ForMenuCheck("Camera", () => EditorSettings.ViewCamera, x => EditorSettings.ViewCamera = x);
                    ForMenuCheck("Layer control", () => EditorSettings.ViewLayerControl, x => EditorSettings.ViewLayerControl = x);
                    ForMenuCheck("Spawn points", () => EditorSettings.ViewSpawnPoint, x => EditorSettings.ViewSpawnPoint = x);
                    ForMenuCheck("BOB descriptors", () => EditorSettings.ViewBobDescriptor, x => EditorSettings.ViewBobDescriptor = x);
                    ForMenuCheck("Mesh group", () => EditorSettings.ViewMeshGroup, x => EditorSettings.ViewMeshGroup = x);
                    ForMenuCheck("Spawn script MAP", () => EditorSettings.ViewSpawnScriptMap, x => EditorSettings.ViewSpawnScriptMap = x);
                    ForMenuCheck("Spawn script BTL", () => EditorSettings.ViewSpawnScriptBattle, x => EditorSettings.ViewSpawnScriptBattle = x);
                    ForMenuCheck("Spawn script EVT", () => EditorSettings.ViewSpawnScriptEvent, x => EditorSettings.ViewSpawnScriptEvent = x);
                });
                ForMenu("Help", () =>
                {
                    ForMenuItem("About", ShowAboutDialog);
                });
            });
        }

        private void MenuFileOpen() => FileDialog.OnFolder(OpenFolder);
        private void MenuFileUnload() => _mapRenderer.Close();
        private void MenuFileOpenMap() => FileDialog.OnOpen(_mapRenderer.OpenMap, MapFilter);
        private void MenuFileOpenArd() => FileDialog.OnOpen(_mapRenderer.OpenArd, ArdFilter);

        private void MenuFileSave()
        {
            _mapRenderer.SaveMap(Path.Combine(_mapPath, MapName + ".map"));
            _mapRenderer.SaveArd(Path.Combine(_ardPath, MapName + ".ard"));
        }

        private void MenuFileSaveMapAs()
        {
            var defaultName = MapName + ".map";
            FileDialog.OnSave(_mapRenderer.SaveMap, MapFilter, defaultName);
        }

        private void MenuFileSaveArdAs()
        {
            var defaultName = MapName + ".ard";
            FileDialog.OnSave(_mapRenderer.SaveArd, ArdFilter, defaultName);
        }

        private void ExportMapCollision() => FileDialog.OnSave(fileName =>
        {
            ExportScene(fileName, _mapRenderer.MapCollision.Scene);
        }, ModelFilter, $"{MapName}_map-collision.dae");

        private void ExportCameraCollision() => FileDialog.OnSave(fileName =>
        {
            ExportScene(fileName, _mapRenderer.CameraCollision.Scene);
        }, ModelFilter, $"{MapName}_camera-collision.dae");

        private void ExportLightCollision() => FileDialog.OnSave(fileName =>
        {
            ExportScene(fileName, _mapRenderer.LightCollision.Scene);
        }, ModelFilter, $"{MapName}_light-collision.dae");

        private void MenuFileExit() => _exitFlag = true;

        public void OpenFolder(string gamePath)
        {
            try
            {
                if (!Directory.Exists(_ardPath = Path.Combine(gamePath, "ard")) ||
                    !Directory.Exists(_mapPath = Path.Combine(gamePath, "map")) ||
                    !Directory.Exists(_objPath = Path.Combine(gamePath, "obj")))
                    throw new DirectoryNotFoundException(
                        "The specified directory must contain the full extracted copy of the game.");

                GamePath = gamePath;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void UpdateTitle()
        {
            _bootstrap.Title = Title;
        }

        private void EnumerateMapList()
        {
            var mapFiles = Directory.GetFiles(_mapPath, "*.map");
            if (mapFiles.Length == 0)
            {
                foreach (var region in Constants.Regions)
                {
                    var testPath = Path.Combine(_mapPath, region);
                    if (Directory.Exists(testPath))
                    {
                        _mapPath = testPath;
                        _region = region;
                        break;
                    }
                }
                mapFiles = Directory.GetFiles(_mapPath, "*.map");
            }

            _mapList.Clear();
            foreach (var mapFile in mapFiles)
            {
                var mapName = Path.GetFileNameWithoutExtension(mapFile);
                if (File.Exists(Path.Combine(_ardPath, $"{mapName}.ard")))
                    _mapList.Add(mapName);
            }
        }

        private void AddKeyMapping(Keys key, Action action)
        {
            _keyMapping[key] = action;
        }

        private void ProcessKeyMapping()
        {
            var k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.LeftControl))
            {
                var keys = k.GetPressedKeys();
                foreach (var key in keys)
                {
                    if (_keyMapping.TryGetValue(key, out var action))
                        action();
                }
            }
        }

        private void ProcessKeyboardInput(KeyboardState keyboard, float deltaTime)
        {
            var speed = (float)(deltaTime * EditorSettings.MoveSpeed);
            var moveSpeed = speed;
            if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
                moveSpeed = (float)(deltaTime * EditorSettings.MoveSpeedShift);

            var camera = _mapRenderer.Camera;
            if (keyboard.IsKeyDown(Keys.W)) camera.CameraPosition += xna.Vector3.Multiply(camera.CameraLookAtX, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.S)) camera.CameraPosition -= xna.Vector3.Multiply(camera.CameraLookAtX, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.A)) camera.CameraPosition -= xna.Vector3.Multiply(camera.CameraLookAtY, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.D)) camera.CameraPosition += xna.Vector3.Multiply(camera.CameraLookAtY, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.Q)) camera.CameraPosition += xna.Vector3.Multiply(camera.CameraLookAtZ, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.E)) camera.CameraPosition -= xna.Vector3.Multiply(camera.CameraLookAtZ, moveSpeed * 5);

            if (keyboard.IsKeyDown(Keys.Up)) camera.CameraRotationYawPitchRoll += new xna.Vector3(0, 0, 1 * speed);
            if (keyboard.IsKeyDown(Keys.Down)) camera.CameraRotationYawPitchRoll -= new xna.Vector3(0, 0, 1 * speed);
            if (keyboard.IsKeyDown(Keys.Left)) camera.CameraRotationYawPitchRoll += new xna.Vector3(1 * speed, 0, 0);
            if (keyboard.IsKeyDown(Keys.Right)) camera.CameraRotationYawPitchRoll -= new xna.Vector3(1 * speed, 0, 0);
        }

        private void ProcessMouseInput(MouseState mouse)
        {
            const float Speed = 0.25f;
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                var camera = _mapRenderer.Camera;
                var xSpeed = (_previousMousePosition.X - mouse.Position.X) * Speed;
                var ySpeed = (_previousMousePosition.Y - mouse.Position.Y) * Speed;
                camera.CameraRotationYawPitchRoll += new xna.Vector3(1 * xSpeed, 0, 0);
                camera.CameraRotationYawPitchRoll += new xna.Vector3(0, 0, 1 * ySpeed);
            }

            _previousMousePosition = mouse.Position;
        }

        private static void ExportScene(string fileName, Scene scene)
        {
            using var ctx = new AssimpContext();
            var extension = Path.GetExtension(fileName).ToLower();
            var exportFormat = ctx.GetSupportedExportFormats();
            foreach (var format in exportFormat)
            {
                if ($".{format.FileExtension}" == extension)
                {
                    var material = new Material();
                    material.Clear();

                    scene.Materials.Add(material);
                    ctx.ExportFile(scene, fileName, format.FormatId);
                    return;
                }
            }

            ShowError($"Unable to export with '{extension}' extension.");
        }

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowAboutDialog() =>
            MessageBox.Show("OpenKH is amazing.");
    }
}
