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
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers.HandyEditorSpec;
using MathNet.Numerics.LinearAlgebra.Factorization;

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

        public Action CreateWindowRunnable()
        {
            var jointAge = _loadedModel.SelectedJointIndexAge.Branch(true);
            var msetAge = _loadedModel.JointDescriptionsAge.Branch(false);

            return () =>
            {
                if (_settings.ViewIKHelper)
                {
                    var windowClosed = !ForWindow("IKHelper manager", () =>
                    {
                        var saved = false;

                        var absIndex = _loadedModel.SelectedJointIndex;
                        var numFk = _loadedModel.FKJointDescriptions.Count;

                        if (false)
                        { }
                        else if (_loadedModel.InternalFkBones?.GetAtOrNull(absIndex) is Mdlx.Bone fk)
                        {
                            ForEdit("joint index", () => fk.Index, it => { });
                            ForEdit("parent index", () => fk.Parent, it => { });
                            ForEdit3("scale",
                                () => new Vector3(fk.ScaleX, fk.ScaleY, fk.ScaleZ),
                                it =>
                                {
                                }
                            );
                            ForEdit3("rotate",
                                () => new Vector3(fk.RotationX, fk.RotationY, fk.RotationZ),
                                it =>
                                {
                                }
                            );
                            ForEdit3("translation",
                                () => new Vector3(fk.TranslationX, fk.TranslationY, fk.TranslationZ),
                                it =>
                                {
                                }
                            );
                        }
                        else if (_loadedModel.MotionData?.IKHelpers.GetAtOrNull(absIndex - numFk) is Motion.IKHelper ikRef)
                        {
                            ForEdit("joint index", () => ikRef.Index, it => { ikRef.Index = it; saved = true; });
                            ForEdit("parent index", () => ikRef.ParentId, it => { ikRef.ParentId = it; saved = true; });
                            ForEdit("Unknown", () => ikRef.Unknown, it => { ikRef.Unknown = it; saved = true; });
                            ForEdit("Terminate", () => ikRef.Terminate, it => { ikRef.Terminate = it; saved = true; });
                            ForEdit("Below", () => ikRef.Below, it => { ikRef.Below = it; saved = true; });
                            ForEdit("EnableBias", () => ikRef.EnableBias, it => { ikRef.EnableBias = it; saved = true; });
                            ForEdit3("scale",
                                () => new Vector3(ikRef.ScaleX, ikRef.ScaleY, ikRef.ScaleZ),
                                it =>
                                {
                                    ikRef.ScaleX = it.X;
                                    ikRef.ScaleY = it.Y;
                                    ikRef.ScaleZ = it.Z;
                                    saved = true;
                                }
                            );
                            ForEdit3("rotate",
                                () => new Vector3(ikRef.RotateX, ikRef.RotateY, ikRef.RotateZ),
                                it =>
                                {
                                    ikRef.RotateX = it.X;
                                    ikRef.RotateY = it.Y;
                                    ikRef.RotateZ = it.Z;
                                    saved = true;
                                }
                            );
                            ForEdit3("translation",
                                () => new Vector3(ikRef.TranslateX, ikRef.TranslateY, ikRef.TranslateZ),
                                it =>
                                {
                                    ikRef.TranslateX = it.X;
                                    ikRef.TranslateY = it.Y;
                                    ikRef.TranslateZ = it.Z;
                                    saved = true;
                                }
                            );
                        }
                        else
                        {
                            ImGui.Text("(Not available)");
                        }

                        if (saved)
                        {
                            _loadedModel.SendBackMotionData.TurnOn();
                        }
                    });

                    if (windowClosed)
                    {
                        _settings.ViewIKHelper = false;
                    }
                }
            };
        }
    }
}
