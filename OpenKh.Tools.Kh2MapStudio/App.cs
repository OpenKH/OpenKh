using Assimp;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using OpenKh.Engine;
using OpenKh.Kh2;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MapStudio.Models;
using OpenKh.Tools.Kh2MapStudio.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

        private const string SelectArdFilesCaption = "Select ard files";

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
        private List<MapArdsBefore> _mapArdsList = new List<MapArdsBefore>();
        private ObjEntryController _objEntryController;

        private xna.Point _previousMousePosition;
        private MapArdsBefore _before;
        private MapArdsAfter _after;
        private SelectArdFilesState _selectArdFilesState = new SelectArdFilesState();

        private record MapArdsBefore(string MapName, string MapFile, IEnumerable<string> ArdFilesRelative)
        {

        }

        private record MapArdsAfter(string MapName, string MapFile, string ArdFileRelativeInput, IEnumerable<string> ArdFilesRelativeOutput)
        {

        }

        private class SelectArdFilesState
        {
            public string InputArd { get; set; }
            public List<string> OutputArds { get; set; } = new List<string>();

            public void Reset()
            {
                InputArd = null;
                OutputArds.Clear();
            }
        }

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

                // Determine the objentry file to use
                var objEntryFileName = Path.Combine(_gamePath, "mapstudio", "00objentry.bin");
                if (!File.Exists(objEntryFileName))
                {
                    objEntryFileName = Path.Combine(_gamePath, "00objentry.bin");
                }

                _objEntryController = new ObjEntryController(
                    _bootstrap.GraphicsDevice,
                    _objPath,
                    objEntryFileName);
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
            }
        }

        private void LoadMapArd(MapArdsAfter after)
        {
            MapName = after.MapName;

            _mapRenderer.Close();
            _mapRenderer.OpenMap(after.MapFile);
            _mapRenderer.OpenArd(Path.Combine(_ardPath, after.ArdFileRelativeInput));
            _after = after;
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
            if (!_bootstrap.ImGuiWantTextInput)
                ProcessKeyboardInput(Keyboard.GetState(), 1f / 60);
            if (!_bootstrap.ImGuiWantCaptureMouse)
                ProcessMouseInput(Mouse.GetState());

            ImGui.PushStyleColor(ImGuiCol.WindowBg, BgUiColor);
            ForControl(ImGui.BeginMainMenuBar, ImGui.EndMainMenuBar, MainMenu);

            MainWindow();

            ForWindow("Tools", () =>
            {
                if (_mapRenderer.CurrentArea.AreaSettingsMask is int areaSettingsMask)
                {
                    ImGui.Text($"AreaSettings 0 -1");

                    for (int x = 0; x < 32; x++)
                    {
                        if ((areaSettingsMask & (1 << x)) != 0)
                        {
                            ImGui.Text($"AreaSettings {x} -1");
                        }
                    }
                }

                
                if (EditorSettings.ViewCamera)
                    CameraWindow.Run(_mapRenderer.Camera);
                if (EditorSettings.ViewLayerControl)
                    LayerControllerWindow.Run(_mapRenderer);
                if (EditorSettings.ViewSpawnPoint)
                    SpawnPointWindow.Run(_mapRenderer);
                if (EditorSettings.ViewMeshGroup)
                    MeshGroupWindow.Run(_mapRenderer.MapMeshGroups);
                if (EditorSettings.ViewCollision && _mapRenderer.MapCollision != null)
                    CollisionWindow.Run(_mapRenderer.MapCollision.Coct);
                if (EditorSettings.ViewBobDescriptor)
                    BobDescriptorWindow.Run(_mapRenderer.BobDescriptors, _mapRenderer.BobMeshGroups.Count);
                if (EditorSettings.ViewSpawnScriptMap)
                    SpawnScriptWindow.Run("map", _mapRenderer.SpawnScriptMap);
                if (EditorSettings.ViewSpawnScriptBattle)
                    SpawnScriptWindow.Run("btl", _mapRenderer.SpawnScriptBattle);
                if (EditorSettings.ViewSpawnScriptEvent)
                    SpawnScriptWindow.Run("evt", _mapRenderer.SpawnScriptEvent);


                if (EditorSettings.ViewEventScript && _mapRenderer.EventScripts != null)
                {
                    foreach (var eventScript in _mapRenderer.EventScripts)
                    {
                        EventScriptWindow.Run(eventScript.Name, eventScript);
                    }
                }
            });

            //Add separate Camera Window if setting is toggled on.
            if (EditorSettings.SeparateCamera)
            {
                SeparateWindow.Run(_mapRenderer.Camera);
            };

            SelectArdFilesPopup();

            ImGui.PopStyleColor();

            return _exitFlag;
        }

        public void Dispose()
        {
            _objEntryController?.Dispose();
        }

        private void SelectArdFilesPopup()
        {
            var dummy = true;
            if (ImGui.BeginPopupModal(SelectArdFilesCaption, ref dummy,
                ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Select one ard file:");
                ImGui.Separator();
                ImGui.Text("Load from ard:");
                ForChild("loadFromArds", 120, 120, true, () =>
                {
                    foreach (var ard in _before.ArdFilesRelative)
                    {
                        if (ImGui.Selectable(ard, _selectArdFilesState.InputArd == ard, ImGuiSelectableFlags.DontClosePopups))
                        {
                            _selectArdFilesState.InputArd = ard;
                        }
                    }
                });
                ImGui.Separator();
                ImGui.Text("Save to ards:");
                ForChild("saveToArds", 120, 120, true, () =>
                {
                    foreach (var ard in _before.ArdFilesRelative)
                    {
                        if (ImGui.Selectable($"{ard}##save", _selectArdFilesState.OutputArds.Contains(ard), ImGuiSelectableFlags.DontClosePopups))
                        {
                            if (!_selectArdFilesState.OutputArds.Remove(ard))
                            {
                                _selectArdFilesState.OutputArds.Add(ard);
                            }
                        }
                    }
                });
                if (ImGui.Button("Select all"))
                {
                    _selectArdFilesState.OutputArds.Clear();
                    _selectArdFilesState.OutputArds.AddRange(_before.ArdFilesRelative);
                }
                ImGui.Separator();
                ImGui.BeginDisabled(_selectArdFilesState.InputArd == null || !_selectArdFilesState.OutputArds.Any());
                if (ImGui.Button("Proceed"))
                {
                    LoadMapArd(
                        new MapArdsAfter(
                            _before.MapName,
                            _before.MapFile,
                            _selectArdFilesState.InputArd,
                            _selectArdFilesState.OutputArds
                        )
                    );

                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndDisabled();
                ImGui.EndPopup();
            }
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
                var ret = ImGui.Begin("Map List", //List of all the maps, the left-side bar.
                    //ImGuiWindowFlags.NoDecoration | //Removes the scroll-bar
                    ImGuiWindowFlags.NoCollapse | //Prevents it from being collapsible
                    ImGuiWindowFlags.AlwaysAutoResize | //NEW: Resizes the window to accomodate maps of various name lengths.
                    ImGuiWindowFlags.NoMove); //Prevents it from being moved around
                ImGui.SetWindowPos(nextPos);
                ImGui.SetWindowSize(new Vector2(64, 0));
                return ret;
            }, () => { }, (Action)(() =>
            {
                foreach (var mapArds in _mapArdsList)
                {
                    if (ImGui.Selectable(mapArds.MapName, MapName == mapArds.MapName))
                    {
                        if (mapArds.ArdFilesRelative.Count() == 1)
                        {
                            LoadMapArd(
                                new MapArdsAfter(
                                    mapArds.MapName,
                                    mapArds.MapFile,
                                    mapArds.ArdFilesRelative.Single(),
                                    mapArds.ArdFilesRelative
                                )
                            );
                        }
                        else
                        {
                            _before = mapArds;

                            _selectArdFilesState.Reset();

                            ImGui.OpenPopup(SelectArdFilesCaption);
                        }
                    }
                }
            }));
            //ImGui.SameLine();

            if (!IsMapOpen)
            {
                //ImGui.Text("Select a map to edit."); //Text. Appears at the bottom of the list, commented out.
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
                    ForMenuItem("Exit", MenuFileExit);
                });
                ForMenu("View", () =>
                {
                    ForMenuCheck("Camera", () => EditorSettings.ViewCamera, x => EditorSettings.ViewCamera = x);
                    ForMenuCheck("Separate Camera Window", () => EditorSettings.SeparateCamera, x => EditorSettings.SeparateCamera = x);
                    ForMenuCheck("Layer control", () => EditorSettings.ViewLayerControl, x => EditorSettings.ViewLayerControl = x);
                    ForMenuCheck("Spawn points", () => EditorSettings.ViewSpawnPoint, x => EditorSettings.ViewSpawnPoint = x);
                    ForMenuCheck("BOB descriptors", () => EditorSettings.ViewBobDescriptor, x => EditorSettings.ViewBobDescriptor = x);
                    ForMenuCheck("Mesh group", () => EditorSettings.ViewMeshGroup, x => EditorSettings.ViewMeshGroup = x);
                    ForMenuCheck("Collision (Experimental)", () => EditorSettings.ViewCollision, x => EditorSettings.ViewCollision = x);
                    ForMenuCheck("Spawn script MAP", () => EditorSettings.ViewSpawnScriptMap, x => EditorSettings.ViewSpawnScriptMap = x);
                    ForMenuCheck("Spawn script BTL", () => EditorSettings.ViewSpawnScriptBattle, x => EditorSettings.ViewSpawnScriptBattle = x);
                    ForMenuCheck("Spawn script EVT", () => EditorSettings.ViewSpawnScriptEvent, x => EditorSettings.ViewSpawnScriptEvent = x);
                    ForMenuCheck("Event script", () => EditorSettings.ViewEventScript, x => EditorSettings.ViewEventScript = x);
                });

                ForMenu("Preferences", () =>
                {
                    ForMenu("Movement Speed", () =>
                    {
                        ForEdit("Default Speed", () => EditorSettings.MoveSpeed, x => EditorSettings.MoveSpeed = x);
                        ForEdit("Accelerated Speed (hold shift)", () => EditorSettings.MoveSpeedShift, x => EditorSettings.MoveSpeedShift = x);
                    });
                    ForMenu("Event Activator Colors & Opacity", () =>
                    {
                        ForEdit5("Opacity", () => EditorSettings.OpacityLevel, x => EditorSettings.OpacityLevel = x);
                        ForEdit5("Red", () => EditorSettings.RedValue, x => EditorSettings.RedValue = x);
                        ForEdit5("Green", () => EditorSettings.GreenValue, x => EditorSettings.GreenValue = x);
                        ForEdit5("Blue", () => EditorSettings.BlueValue, x => EditorSettings.BlueValue = x);
                    });
                    ForMenu("Event Activator Entrance Colors & Opacity", () =>
                    {
                        ForEdit5("Opacity", () => EditorSettings.OpacityEntranceLevel, x => EditorSettings.OpacityEntranceLevel = x);
                        ForEdit5("Red", () => EditorSettings.RedValueEntrance, x => EditorSettings.RedValueEntrance = x);
                        ForEdit5("Green", () => EditorSettings.GreenValueEntrance, x => EditorSettings.GreenValueEntrance = x);
                        ForEdit5("Blue", () => EditorSettings.BlueValueEntrance, x => EditorSettings.BlueValueEntrance = x);
                    });
                    ForMenu("Default Window Size", () =>
                    {
                        ForEdit("Window Width", () => EditorSettings.InitialWindowWidth, x => EditorSettings.InitialWindowWidth = x);
                        ForEdit("Window Height", () => EditorSettings.InitialWindowHeight, x => EditorSettings.InitialWindowHeight = x);

                    });
                });

                ForMenu("Help", () =>
                {
                    ForMenuItem("About", ShowAboutDialog);
                    ForMenuItem("Preference Info", ShowPrefDialog);
                    ForMenuItem("Controls", ShowControlsDialog);
                });
            });
        }

        private void MenuFileOpen() => FileDialog.OnFolder(OpenFolder);
        private void MenuFileUnload() => _mapRenderer.Close();
        private void MenuFileOpenMap() => FileDialog.OnOpen(_mapRenderer.OpenMap, MapFilter);
        private void MenuFileOpenArd() => FileDialog.OnOpen(_mapRenderer.OpenArd, ArdFilter);

        private void MenuFileSave()
        {
            _mapRenderer.SaveMap(_after.MapFile);

            foreach (var ard in _after.ArdFilesRelativeOutput)
            {
                _mapRenderer.SaveArd(Path.Combine(_ardPath, ard));
            }
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
            var mapFiles = Array.Empty<string>();
            foreach (var region in Constants.Regions)
            {
                var testPath = Path.Combine(_mapPath, region);
                if (Directory.Exists(testPath))
                {
                    mapFiles = Directory.GetFiles(testPath, "*.map");
                    if (mapFiles.Length != 0)
                    {
                        _mapPath = testPath;
                        _region = region;
                        break;
                    }
                }
            }

            _mapArdsList.Clear();

            foreach (var mapFile in mapFiles)
            {
                var mapName = Path.GetFileNameWithoutExtension(mapFile);

                var ardFiles = Constants.Regions
                    .Select(region => Path.Combine(region, $"{mapName}.ard"))
                    .Where(it => File.Exists(Path.Combine(_ardPath, it)))
                    .ToArray();

                _mapArdsList.Add(new MapArdsBefore(mapName, mapFile, ardFiles));
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
            if (keyboard.IsKeyDown(Keys.W))
                camera.CameraPosition += Vector3.Multiply(camera.CameraLookAtX, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.S))
                camera.CameraPosition -= Vector3.Multiply(camera.CameraLookAtX, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.D))
                camera.CameraPosition -= Vector3.Multiply(camera.CameraLookAtY, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.A))
                camera.CameraPosition += Vector3.Multiply(camera.CameraLookAtY, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.Q))
                camera.CameraPosition += Vector3.Multiply(camera.CameraLookAtZ, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.E))
                camera.CameraPosition -= Vector3.Multiply(camera.CameraLookAtZ, moveSpeed * 5);
            if (keyboard.IsKeyDown(Keys.Space))
                camera.CameraPosition += new Vector3(0, 1 * moveSpeed * 5, 0);
            if (keyboard.IsKeyDown(Keys.LeftControl))
                camera.CameraPosition += new Vector3(0, -1 * moveSpeed * 5, 0);
            if (keyboard.IsKeyDown(Keys.Up))
                camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * speed);
            if (keyboard.IsKeyDown(Keys.Down))
                camera.CameraRotationYawPitchRoll -= new Vector3(0, 0, 1 * speed);
            if (keyboard.IsKeyDown(Keys.Left))
                camera.CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
            if (keyboard.IsKeyDown(Keys.Right))
                camera.CameraRotationYawPitchRoll -= new Vector3(1 * speed, 0, 0);
        }

        private void ProcessMouseInput(MouseState mouse)
        {
            const float Speed = 0.25f;
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                var camera = _mapRenderer.Camera;
                var xSpeed = (_previousMousePosition.X - mouse.Position.X) * Speed;
                var ySpeed = (_previousMousePosition.Y - mouse.Position.Y) * Speed;
                camera.CameraRotationYawPitchRoll += new Vector3(1 * -xSpeed, 0, 0);
                camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * ySpeed);
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
            //MessageBox.Show("OpenKH is amazing.");
            MessageBox.Show("Welcome to OpenKH MapStudio." +
            "\n\nThis tool allows you to view .map files along with their associated .ard files." +
            "\n\nThe .map and .ard files are loaded using the extracted game data. " +
            "\n\nEntities are loaded from the extracted 00objentry.bin & obj folder. New maps and entities can be added into the extracted game data to have them usable in MapStudio." +
            "\n\nAlternatively, you can create a folder named mapstudio in your extracted game folder. Placing a modified 00objentry.bin as well as any MDLXs inside that folder will cause MapStudio to prioritize loading from that folder instead." +
            "\n\nA .map file contains the geometry and collision of the map." +
            "\n\nAn .ard file controls what spawns inside of a map." +
            "\n\nSpawn points, where you can encounter enemies, cutscenes, cutscene triggers, chest locations, etc. are all handled by .ard files." +
            "\n\nView documentation on openkh.dev to learn more about the file."
            );

        private void ShowPrefDialog() =>
            MessageBox.Show("Movement speed will alter how fast you can move through the map." +
            "\n\nEvent Activator Opacity & Red, Green, and Blue Values all control how the triggers that send you to different areas & spawn enemies will look." +
            "\n\nEntrance Marker will mark the entrances of warp points with that color, so that you can properly orient the entrance in the map." +
            "\n\nValues for RGBA are floats. For the most accurate representation of warps the values should be set between 0 and 1, though values below 0 and above 1 will work." +
            "\n\nValues for Opacity are floats between 0 and 1.");

        private void ShowControlsDialog() =>
             MessageBox.Show("W/A/S/D/E/Q: Moves in any direction, influenced by the camera's rotation." +
             "\n\nLeft Control/Space: Move directly down/up, regardless of camera's rotation." +
             "\n\nShift: Increase movement speed (can be changed under Preferences)." +
             "\n\nLeft Click/Arrow Keys: Rotate Camera");
    }
}
