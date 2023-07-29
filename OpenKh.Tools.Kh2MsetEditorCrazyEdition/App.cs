using Assimp;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenKh.Engine;
using OpenKh.Kh2;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.InsideTools.Old;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition
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
        private readonly LoadedModel _loadedModel;
        private readonly AskOpenFileNowUsecase _askOpenFileNowUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly IMExExcelUsecase _imexExcelUsecase;
        private readonly ReloadKh2PresetsUsecase _reloadKh2PresetsUsecase;
        private readonly Settings _settings;
        private readonly Action[] _windowRunnables;
        private readonly Action _motionLoaderTool;
        private readonly RenderModelUsecase _modelRenderer;
        private readonly IEnumerable<Action> _toolRunnables;
        private readonly GraphicsDevice _graphicsDevice;
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
                    _graphicsDevice,
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

        public App(
            MonoGameImGuiBootstrap bootstrap,
            GetGamePathUsecase getGamePathUsecase,
            MapRenderer mapRenderer,
            GraphicsDevice graphicsDevice,
            IEnumerable<IToolRunnableProvider> toolRunnables,
            RenderModelUsecase modelRenderer,
            MotionLoaderToolUsecase motionLoaderToolUsecase,
            IEnumerable<IWindowRunnableProvider> windowRunnables,
            Settings settings,
            ReloadKh2PresetsUsecase reloadKh2PresetsUsecase,
            IMExExcelUsecase imexExcelUsecase,
            ErrorMessages errorMessages,
            AskOpenFileNowUsecase askOpenFileNowUsecase,
            LoadedModel loadedModel
        )
        {
            var gamePath = getGamePathUsecase();

            _loadedModel = loadedModel;
            _askOpenFileNowUsecase = askOpenFileNowUsecase;
            _errorMessages = errorMessages;
            _imexExcelUsecase = imexExcelUsecase;
            _reloadKh2PresetsUsecase = reloadKh2PresetsUsecase;
            _reloadKh2PresetsUsecase();
            _settings = settings;
            _windowRunnables = windowRunnables
                .Select(one => one.CreateWindowRunnable())
                .ToArray();
            _motionLoaderTool = motionLoaderToolUsecase.CreateToolRunnable();
            _modelRenderer = modelRenderer;
            _toolRunnables = toolRunnables
                .Select(one => one.CreateToolRunnable())
                .ToArray();
            _graphicsDevice = graphicsDevice;
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;
            _mapRenderer = mapRenderer;

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
            _graphicsDevice.Clear(xna.Color.CornflowerBlue);
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
                foreach (var runnable in _toolRunnables)
                {
                    runnable();
                }

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
                if (EditorSettings.ViewBobDescriptor)
                    BobDescriptorWindow.Run(_mapRenderer.BobDescriptors, _mapRenderer.BobMeshGroups.Count);
                if (EditorSettings.ViewSpawnScriptMap)
                    SpawnScriptWindow.Run("map", _mapRenderer.SpawnScriptMap);
                if (EditorSettings.ViewSpawnScriptBattle)
                    SpawnScriptWindow.Run("btl", _mapRenderer.SpawnScriptBattle);
                if (EditorSettings.ViewSpawnScriptEvent)
                    SpawnScriptWindow.Run("evt", _mapRenderer.SpawnScriptEvent);

                if (_mapRenderer.EventScripts != null)
                {
                    foreach (var eventScript in _mapRenderer.EventScripts)
                    {
                        EventScriptWindow.Run(eventScript.Name, eventScript);
                    }
                }
            });

            foreach (var runnable in _windowRunnables)
            {
                runnable();
            }

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

            //_motionLoaderTool();

            //ForControl(() =>
            //{
            //    var nextPos = ImGui.GetCursorPos();
            //    var ret = ImGui.Begin("MapList",
            //        ImGuiWindowFlags.NoDecoration |
            //        ImGuiWindowFlags.NoCollapse |
            //        ImGuiWindowFlags.NoMove);
            //    ImGui.SetWindowPos(nextPos);
            //    ImGui.SetWindowSize(new Vector2(64, 0));
            //    return ret;
            //}, () => { }, () =>
            //{
            //    foreach (var map in _mapList)
            //    {
            //        if (ImGui.Selectable(map, MapName == map))
            //        {
            //            MapName = map;
            //        }
            //    }
            //});

            _modelRenderer.Draw();

            if (!IsMapOpen)
            {
                //ImGui.Text("Please select a map to edit.");
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
                    ForMenuItem("Reload Kh2Presets", ReloadKh2Presets);
                    ImGui.Separator();
                    ForMenuItem("Export current motion to Excel", ExportExcel);
                    ForMenuItem("Import current motion from Excel", ImportExcel);
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

                    ForMenuCheck("Bones", () => _settings.ViewBones, it =>
                    {
                        _settings.ViewBones = it;
                        _settings.Save();
                    });
                    ForMenuCheck("IKHelper", () => _settings.ViewIKHelper, it =>
                    {
                        _settings.ViewIKHelper = it;
                        _settings.Save();
                    });
                    ForMenuCheck("InitialPose", () => _settings.ViewInitialPose, it =>
                    {
                        _settings.ViewInitialPose = it;
                        _settings.Save();
                    });
                    ForMenuCheck("Expression", () => _settings.ViewExpression, it =>
                    {
                        _settings.ViewExpression = it;
                        _settings.Save();
                    });
                });
                ForMenu("Help", () =>
                {
                    ForMenuItem("About", ShowAboutDialog);
                });
            });
        }

        private void ExportExcel()
        {
            FileDialog.OnSave(
                saveTo =>
                {
                    try
                    {
                        _imexExcelUsecase.ExportTo(saveTo);

                        _askOpenFileNowUsecase.AskAndOpen(saveTo);
                    }
                    catch (Exception ex)
                    {
                        _errorMessages.Add(ex);
                    }
                },
                FileDialogFilterComposer.Compose()
                    .AddExtensions("Excel xlsx", "xlsx")
                    .AddAllFiles(),
                _loadedModel.PreferredMotionExportXlsx ?? "Export.xlsx"
            );
        }

        private void ImportExcel()
        {
            FileDialog.OnOpen(
                loadFrom =>
                {
                    try
                    {
                        var result = _imexExcelUsecase.ImportFrom(loadFrom);

                        if (result.Errors.Any())
                        {
                            _errorMessages.Add(
                                new AggregateException(
                                    $"{result.Errors.Count} error(s) detected while import from Excel file.",
                                    result.Errors
                                )
                            );
                        }
                        else
                        {
                            _loadedModel.SendBackMotionData.TurnOn();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorMessages.Add(ex);
                    }
                },
                FileDialogFilterComposer.Compose()
                    .AddExtensions("Excel xlsx", "xlsx")
                    .AddAllFiles(),
                _loadedModel.PreferredMotionExportXlsx ?? "Export.xlsx"
            );
        }

        private void ReloadKh2Presets()
        {
            try
            {
                _reloadKh2PresetsUsecase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + "");
            }
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

            _mapList.Clear();
            _mapList.AddRange(mapFiles.Select(Path.GetFileNameWithoutExtension));
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
            MessageBox.Show("OpenKH is amazing.");
    }
}
