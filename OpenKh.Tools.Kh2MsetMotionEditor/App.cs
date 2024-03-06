using Assimp;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenKh.AssimpUtils;
using OpenKh.Engine;
using OpenKh.Kh2;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using OpenKh.Tools.Kh2MsetMotionEditor.Usecases;
using OpenKh.Tools.Kh2MsetMotionEditor.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetMotionEditor
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
        private readonly NormalMessages _normalMessages;
        private readonly GlobalInfo _globalInfo;
        private readonly Engine.Camera _camera;
        private readonly LoadedModel _loadedModel;
        private readonly AskOpenFileNowUsecase _askOpenFileNowUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly IMExExcelUsecase _imexExcelUsecase;
        private readonly ReloadKh2PresetsUsecase _reloadKh2PresetsUsecase;
        private readonly Settings _settings;
        private readonly Action[] _windowRunnables;
        private readonly RenderModelUsecase _modelRenderer;
        private readonly IEnumerable<Action> _toolRunnables;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly MonoGameImGuiBootstrap _bootstrap;
        private bool _exitFlag = false;

        private readonly Dictionary<Keys, Action> _keyMapping = new Dictionary<Keys, Action>();
        private string _gamePath;
        private string _mapName;
        private string _region;
        private string _ardPath;
        private string _mapPath;
        private string _objPath;

        private xna.Point _previousMousePosition;

        public string Title
        {
            get
            {
                return $"{MonoGameImGuiBootstrap.ApplicationName}";
            }
        }

        public App(
            MonoGameImGuiBootstrap bootstrap,
            GraphicsDevice graphicsDevice,
            IEnumerable<IToolRunnableProvider> toolRunnables,
            RenderModelUsecase modelRenderer,
            IEnumerable<IWindowRunnableProvider> windowRunnables,
            Settings settings,
            ReloadKh2PresetsUsecase reloadKh2PresetsUsecase,
            IMExExcelUsecase imexExcelUsecase,
            ErrorMessages errorMessages,
            AskOpenFileNowUsecase askOpenFileNowUsecase,
            LoadedModel loadedModel,
            OpenKh.Engine.Camera camera,
            GlobalInfo globalInfo,
            NormalMessages normalMessages
        )
        {
            _normalMessages = normalMessages;
            _globalInfo = globalInfo;
            _camera = camera;
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
            _modelRenderer = modelRenderer;
            _toolRunnables = toolRunnables
                .Select(one => one.CreateToolRunnable())
                .ToArray();
            _graphicsDevice = graphicsDevice;
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;

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

                if (EditorSettings.ViewCamera)
                {
                    CameraWindow.Run(_camera);
                }
            });

            foreach (var runnable in _windowRunnables)
            {
                runnable();
            }

            ImGui.PopStyleColor();

            ++_globalInfo.Ticks;

            return _exitFlag;
        }

        public void Dispose()
        {
        }

        private void MainWindow()
        {
            _modelRenderer.Draw();
        }

        void MainMenu()
        {
            ForMenuBar(() =>
            {
                ForMenu("File", () =>
                {
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
                    ForMenuItem("Export current motion to FBX", ExportFBX);
                    ForMenuItem("Export current motion to COLLADA", ExportCOLLADA);
                    ImGui.Separator();
                    ForMenuItem("Exit", MenuFileExit);
                });
                ForMenu("View", () =>
                {
                    ForMenuCheck("Camera", () => EditorSettings.ViewCamera, x => EditorSettings.ViewCamera = x);

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
                    ForMenuCheck("Constraint", () => _settings.ViewConstraint, it =>
                    {
                        _settings.ViewConstraint = it;
                        _settings.Save();
                    });
                    ForMenuCheck("PrintDebugInfo", () => _settings.ViewDebugInfo, it =>
                    {
                        _settings.ViewDebugInfo = it;
                        _settings.Save();
                    });
                    ForMenuCheck("Joint", () => _settings.ViewJoint, it =>
                    {
                        _settings.ViewJoint = it;
                        _settings.Save();
                    });
                    ForMenuCheck("FCurvesForward", () => _settings.ViewFCurvesForward, it =>
                    {
                        _settings.ViewFCurvesForward = it;
                        _settings.Save();
                    });
                    ForMenuCheck("FCurvesInverse", () => _settings.ViewFCurvesInverse, it =>
                    {
                        _settings.ViewFCurvesInverse = it;
                        _settings.Save();
                    });
                    ForMenuCheck("FCurvesFkIk grid", () => _settings.ViewFCurvesGrid, it =>
                    {
                        _settings.ViewFCurvesGrid = it;
                        _settings.Save();
                    });
                    ForMenuCheck("FCurveKey", () => _settings.ViewFCurveKey, it =>
                    {
                        _settings.ViewFCurveKey = it;
                        _settings.Save();
                    });
                    ForMenuCheck("RootPosition", () => _settings.ViewRootPosition, it =>
                    {
                        _settings.ViewRootPosition = it;
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
                            var writer = new StringWriter();
                            writer.WriteLine("Import succeeded. The following lines are messages noted by importer.");
                            writer.WriteLine();

                            result.Results
                                .ForEach(it => writer.WriteLine(it.Message));

                            _normalMessages.Add(writer.ToString());

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

        private void ExportFBXCOLLADA(AssimpGeneric.FileFormat fileFormat)
        {
            if (_loadedModel.SelectedMotionIndex < 0)
                System.Windows.Forms.MessageBox.Show("No motion selected.", "No motion selected.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
            else
            {
                var selectedMotionName = _loadedModel.MotionList[_loadedModel.SelectedMotionIndex].Label;
                FileDialog.OnSave(
                    filename =>
                    {
                        try
                        {

                            if (_loadedModel.MdlxRenderableList.Count > 0 &&
                            _loadedModel.MdlxRenderableList[0].Model is Mdlx)
                            {
                                OpenKh.Engine.Parsers.MdlxParser mParser = new OpenKh.Engine.Parsers.MdlxParser(_loadedModel.MdlxRenderableList[0].Model);
                                Assimp.Scene scene = Kh2MdlxAssimp.getAssimpScene(mParser);

                                if (filename != "")
                                {
                                    string dirPath = Path.GetDirectoryName(filename);

                                    if (!Directory.Exists(dirPath))
                                        return;
                                    dirPath += "\\";

                                    var motionData = _loadedModel.MotionData;
                                    if (motionData != null)
                                    {
                                        Animation animation = new Animation();
                                        animation.Name = selectedMotionName;
                                        animation.TicksPerSecond = fileFormat == AssimpGeneric.FileFormat.fbx ? 24f : 30f;
                                        animation.DurationInTicks = (double)(motionData.InterpolatedMotionHeader.FrameCount / animation.TicksPerSecond);

                                        for (int b = 0; b < mParser.Bones.Count; b++)
                                        {
                                            NodeAnimationChannel nodeAnimationChannel = new NodeAnimationChannel();
                                            nodeAnimationChannel.NodeName = "Bone" + b;
                                            animation.NodeAnimationChannels.Add(nodeAnimationChannel);
                                        }

                                        for (int f = 0; f < motionData.InterpolatedMotionHeader.FrameCount; f++)
                                        {
                                            float timeKey = (float)((1 / animation.TicksPerSecond) * f);

                                            var fkIk = _loadedModel.PoseProvider?.Invoke((float)f);

                                            List<Microsoft.Xna.Framework.Matrix> matrices = new List<Microsoft.Xna.Framework.Matrix>(0);
                                            List<bool> dirtyMatrices = new List<bool>(0);

                                            if (fkIk != null && _loadedModel.MotionData?.IKHelpers is System.Collections.Generic.List<Motion.IKHelper> ikHelperList)
                                            {
                                                var numFk = fkIk.Fk.Length;
                                                var numIk = fkIk.Ik.Length;
                                                var length = fkIk.Fk.Length;

                                                for (int mIndex = 0; mIndex < length; mIndex++)
                                                {
                                                    matrices.Add(fkIk.Fk[mIndex].ToXnaMatrix());
                                                }
                                            }
                                            Kh2MdlxAssimp.ReverseMatrices(ref matrices, mParser);

                                            for (int b = 0; b < mParser.Bones.Count; b++)
                                            {
                                                NodeAnimationChannel channel = animation.NodeAnimationChannels[b];
                                                Microsoft.Xna.Framework.Vector3 scale;
                                                Microsoft.Xna.Framework.Quaternion rotate;
                                                Microsoft.Xna.Framework.Vector3 translate;
                                                matrices[b].Decompose(out scale, out rotate, out translate);

                                                channel.ScalingKeys.Add(new VectorKey(timeKey, new Vector3D(scale.X, scale.Y, scale.Z)));
                                                channel.RotationKeys.Add(new QuaternionKey(timeKey, new Assimp.Quaternion(rotate.W, rotate.X, rotate.Y, rotate.Z)));
                                                channel.PositionKeys.Add(new VectorKey(timeKey, new Vector3D(translate.X, translate.Y, translate.Z)));
                                            }
                                        }

                                        scene.Animations.Add(animation);
                                    }

                                    AssimpGeneric.ExportScene(scene, fileFormat, filename);
                                    OpenKh.Tools.Kh2MdlxEditor.Views.Main_Window.exportTextures(_loadedModel.MdlxRenderableList[0].Textures, dirPath);
                                    System.Windows.Forms.MessageBox.Show("Export complete.", "Complete", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _errorMessages.Add(ex);
                        }
                    },
                    FileDialogFilterComposer.Compose()
                        .AddExtensions("Autodesk FBX ", "fbx"),
                                _loadedModel.MdlxFile + "-" + selectedMotionName.Replace(" ", "_") + "." + AssimpGeneric.GetFormatFileExtension(fileFormat)
                );
            }
        }

        private void ExportFBX()
        {
            ExportFBXCOLLADA(AssimpGeneric.FileFormat.fbx);
        }

        private void ExportCOLLADA()
        {
            ExportFBXCOLLADA(AssimpGeneric.FileFormat.collada);
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

        private void MenuFileExit() => _exitFlag = true;

        private void UpdateTitle()
        {
            _bootstrap.Title = Title;
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

            var camera = _camera;
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
                var camera = _camera;
                var xSpeed = (_previousMousePosition.X - mouse.Position.X) * Speed;
                var ySpeed = (_previousMousePosition.Y - mouse.Position.Y) * Speed;
                camera.CameraRotationYawPitchRoll += new Vector3(1 * -xSpeed, 0, 0);
                camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * ySpeed);
            }

            _previousMousePosition = mouse.Position;
        }

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowAboutDialog() =>
            MessageBox.Show("OpenKH is amazing.");
    }
}
