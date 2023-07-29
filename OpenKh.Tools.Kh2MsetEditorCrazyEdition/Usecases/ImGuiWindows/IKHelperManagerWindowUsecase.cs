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
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public IKHelperManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            ErrorMessages errorMessages
        )
        {
            _errorMessages = errorMessages;
            _loadedModel = loadedModel;
            _settings = settings;
        }

        private enum EditorType
        {
            No,
            Fk,
            Ik,
        }

        public Action CreateWindowRunnable()
        {
            var jointAge = _loadedModel.SelectedJointIndexAge.Branch(true);

            var editor = EditorType.No;
            var jointIndex = 0;
            var parentIndex = 0;
            var unknown = 0;
            var terminate = false;
            var below = false;
            var enableBias = false;
            Func<Motion.IKHelper>? getIkHelper = null;
            Vector3 scale = Vector3.Zero;
            Vector3 rotate = Vector3.Zero;
            Vector3 translate = Vector3.Zero;
            Mdlx.Bone? fkBone = null;

            return () =>
            {
                if (jointAge.NeedToCatchUp())
                {
                    // load

                    var absIndex = _loadedModel.SelectedJointIndex;
                    var numFk = _loadedModel.FKJointDescriptions.Count;
                    if (numFk <= absIndex)
                    {
                        // ik
                        getIkHelper = () => _loadedModel.MotionData!.IKHelpers[absIndex - numFk];
                        var ikHelper = getIkHelper();
                        jointIndex = ikHelper.Index;
                        parentIndex = ikHelper.ParentId;
                        unknown = ikHelper.Unknown;
                        terminate = ikHelper.Terminate;
                        below = ikHelper.Below;
                        enableBias = ikHelper.EnableBias;
                        scale = new Vector3(ikHelper.ScaleX, ikHelper.ScaleY, ikHelper.ScaleZ);
                        rotate = new Vector3(ikHelper.RotateX, ikHelper.RotateY, ikHelper.RotateZ);
                        translate = new Vector3(ikHelper.TranslateX, ikHelper.TranslateY, ikHelper.TranslateZ);

                        editor = EditorType.Ik;
                    }
                    else if (0 <= absIndex)
                    {
                        // fk
                        fkBone = _loadedModel.InternalFkBones![absIndex];
                        jointIndex = fkBone.Index;
                        parentIndex = fkBone.Parent;
                        scale = new Vector3(fkBone.ScaleX, fkBone.ScaleY, fkBone.ScaleZ);
                        rotate = new Vector3(fkBone.RotationX, fkBone.RotationY, fkBone.RotationZ);
                        translate = new Vector3(fkBone.TranslationX, fkBone.TranslationY, fkBone.TranslationZ);

                        editor = EditorType.Fk;
                    }
                    else
                    {
                        editor = EditorType.No;
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
                                if (true
                                    && editor == EditorType.Ik
                                    && getIkHelper?.Invoke() is Motion.IKHelper ikHelper
                                )
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

                                    _loadedModel.SendBackMotionData.TurnOn();
                                }
                                if (editor == EditorType.Fk && fkBone != null)
                                {
                                    // save

                                    fkBone.Index = (short)jointIndex;
                                    fkBone.Parent = (short)parentIndex;
                                    fkBone.ScaleX = scale.X;
                                    fkBone.ScaleY = scale.Y;
                                    fkBone.ScaleZ = scale.Z;
                                    fkBone.RotationX = rotate.X;
                                    fkBone.RotationY = rotate.Y;
                                    fkBone.RotationZ = rotate.Z;
                                    fkBone.TranslationX = translate.X;
                                    fkBone.TranslationY = translate.Y;
                                    fkBone.TranslationZ = translate.Z;

                                    _errorMessages.Add(new Exception("fkBone is read only"));
                                }
                            });
                        });

                        if (editor == EditorType.Fk)
                        {
                            ImGui.InputInt("joint index", ref jointIndex);
                            ImGui.InputInt("parent index", ref parentIndex);
                            ForEdit3("scale", () => scale, it => scale = it);
                            ForEdit3("rotate", () => rotate, it => rotate = it);
                            ForEdit3("translate", () => translate, it => translate = it);
                        }

                        if (editor == EditorType.Ik)
                        {
                            ImGui.InputInt("joint index", ref jointIndex);
                            ImGui.InputInt("parent index", ref parentIndex);
                            ImGui.InputInt("unknown", ref unknown);
                            ImGui.Checkbox("terminate", ref terminate);
                            ImGui.Checkbox("below", ref below);
                            ImGui.Checkbox("enableBias", ref enableBias);
                            ForEdit3("scale", () => scale, it => scale = it);
                            ForEdit3("rotate", () => rotate, it => rotate = it);
                            ForEdit3("translate", () => translate, it => translate = it);
                        }
                    },
                        menuBar: true
                    );
                }
            };
        }
    }
}
