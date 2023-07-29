using ImGuiNET;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Kh2;
using System.Numerics;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class IKHelperManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public IKHelperManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var jointAge = _loadedModel.SelectedJointIndexAge.Branch(true);

            var editorAvail = false;
            var jointIndex = 0;
            var parentIndex = 0;
            var unknown = 0;
            var terminate = false;
            var below = false;
            var enableBias = false;
            Motion.IKHelper? ikHelper = null;
            Vector3 scale = Vector3.Zero;
            Vector3 rotate = Vector3.Zero;
            Vector3 translate = Vector3.Zero;

            return () =>
            {
                if (jointAge.NeedToCatchUp())
                {
                    // load

                    var absIndex = _loadedModel.SelectedJointIndex;
                    var numFk = _loadedModel.FKJointDescriptions.Count;
                    if (numFk <= absIndex)
                    {
                        ikHelper = _loadedModel.MotionData!.IKHelpers[absIndex - numFk];
                        jointIndex = ikHelper.Index;
                        parentIndex = ikHelper.ParentId;
                        unknown = ikHelper.Unknown;
                        terminate = ikHelper.Terminate;
                        below = ikHelper.Below;
                        enableBias = ikHelper.EnableBias;
                        scale = new Vector3(ikHelper.ScaleX, ikHelper.ScaleY, ikHelper.ScaleZ);
                        rotate = new Vector3(ikHelper.RotateX, ikHelper.RotateY, ikHelper.RotateZ);
                        translate = new Vector3(ikHelper.TranslateX, ikHelper.TranslateY, ikHelper.TranslateZ);

                        editorAvail = true;
                    }
                    else
                    {
                        editorAvail = false;
                    }
                }

                if (_settings.ViewIKHelper)
                {
                    ForWindow("IKHelper manager", () =>
                    {
                        ForMenuBar(() =>
                        {
                            ForMenuItem("Apply", () =>
                            {
                                if (ikHelper != null)
                                {
                                    // save

                                    ikHelper.Index = (short)jointIndex;
                                    ikHelper.ParentId = (short)parentIndex;
                                    ikHelper.Unknown = unknown;
                                    ikHelper.Terminate = terminate;
                                    ikHelper.Below = below;
                                    ikHelper.EnableBias = enableBias;
                                    ikHelper.ScaleX = scale.X;
                                    ikHelper.ScaleY = scale.Y;
                                    ikHelper.ScaleZ = scale.Z;
                                    ikHelper.RotateX = rotate.X;
                                    ikHelper.RotateY = rotate.Y;
                                    ikHelper.RotateZ = rotate.Z;
                                    ikHelper.TranslateX = translate.X;
                                    ikHelper.TranslateY = translate.Y;
                                    ikHelper.TranslateZ = translate.Z;

                                    _loadedModel.MotionDataAge.Bump();
                                }
                            });
                        });

                        if (editorAvail)
                        {
                            ImGui.InputInt("joint index", ref jointIndex);
                            ImGui.InputInt("parent index", ref parentIndex);
                            ImGui.InputInt("unknown", ref unknown);
                            ImGui.Checkbox("terminate", ref terminate);
                            ImGui.Checkbox("below", ref below);
                            ImGui.Checkbox("enableBias", ref enableBias);
                            ForEdit3("scale", () => scale, it => scale = it);
                        }
                    },
                        menuBar: true
                    );
                }
            };
        }
    }
}
