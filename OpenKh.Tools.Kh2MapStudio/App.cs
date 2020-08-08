using ImGuiNET;
using xna = Microsoft.Xna.Framework;
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

namespace OpenKh.Tools.Kh2MapStudio
{
    class App : IDisposable
    {
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

        public App(MonoGameImGuiBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;
            _mapRenderer = new MapRenderer(bootstrap.Content, bootstrap.GraphicsDeviceManager);
            AddKeyMapping(Keys.O, MenuFileOpen);
            AddKeyMapping(Keys.S, MenuFileSave);
            OpenFolder(@"D:\Hacking\KH2\export_fm");
            
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
            CameraWindow.Run(_mapRenderer.Camera);
            LayerControllerWindow.Run(_mapRenderer);
            MeshGroupWindow.Run(_mapRenderer.MapMeshGroups);
            BobDescriptorWindow.Run(_mapRenderer.BobDescriptors, _mapRenderer.BobMeshGroups.Count);

            ImGui.PopStyleColor();

            return _exitFlag;
        }

        public void Dispose()
        {
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
                    ForMenuItem("Save map", "CTRL+S", MenuFileSave, IsOpen);
                    ForMenuItem("Save as...", MenuFileSaveAs, IsOpen);
                    ImGui.Separator();
                    ForMenu("Preferences", () =>
                    {
                    });
                    ImGui.Separator();
                    ForMenuItem("Exit", MenuFileExit);
                });
                ForMenu("Help", () =>
                {
                    ForMenuItem("About", ShowAboutDialog);
                });
            });
        }

        private void MenuFileOpen() => FileDialog.OnFolder(OpenFolder);

        private void MenuFileSave()
        {
            //if (!string.IsNullOrEmpty(FileName))
            //    SaveFile(FileName, FileName);
            //else
            //    MenuFileSaveAs();
        }

        private void MenuFileSaveAs()
        {
            //FileDialog.OnSave(fileName =>
            //{
            //    SaveFile(FileName, fileName);
            //    FileName = fileName;
            //}, Filters);
        }

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

        public void SaveFile(string previousFileName, string fileName)
        {
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
            const float Speed = 50.0f;
            var speed = (float)(deltaTime * Speed);
            if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
                speed *= 2.5f;

            var camera = _mapRenderer.Camera;
            if (keyboard.IsKeyDown(Keys.W)) camera.CameraPosition += xna.Vector3.Multiply(camera.CameraLookAtX, speed * 5);
            if (keyboard.IsKeyDown(Keys.S)) camera.CameraPosition -= xna.Vector3.Multiply(camera.CameraLookAtX, speed * 5);
            if (keyboard.IsKeyDown(Keys.A)) camera.CameraPosition -= xna.Vector3.Multiply(camera.CameraLookAtY, speed * 5);
            if (keyboard.IsKeyDown(Keys.D)) camera.CameraPosition += xna.Vector3.Multiply(camera.CameraLookAtY, speed * 5);

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

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowAboutDialog() =>
            MessageBox.Show("OpenKH is amazing.");
    }
}
