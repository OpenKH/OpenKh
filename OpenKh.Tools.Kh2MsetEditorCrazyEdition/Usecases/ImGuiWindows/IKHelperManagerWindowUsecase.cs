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

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class IKHelperManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly MakeHandyEditorUsecase _makeHandyEditorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public IKHelperManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            ErrorMessages errorMessages,
            MakeHandyEditorUsecase makeHandyEditorUsecase
        )
        {
            _makeHandyEditorUsecase = makeHandyEditorUsecase;
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
            var msetAge = _loadedModel.JointDescriptionsAge.Branch(false);

            var editor = EditorType.No;

            Func<Motion.IKHelper>? getIkHelper = null;
            Motion.IKHelper? ikRef = null;
            var ikEditors = new List<HandyEditorController>();
            ikEditors.Add(_makeHandyEditorUsecase.InputInt("joint index", () => ikRef!.Index, it => ikRef!.Index = (short)it));
            ikEditors.Add(_makeHandyEditorUsecase.InputInt("parent index", () => ikRef!.ParentId, it => ikRef!.ParentId = (short)it));
            ikEditors.Add(_makeHandyEditorUsecase.InputInt("Unknown", () => ikRef!.Unknown, it => ikRef!.Unknown = it));
            ikEditors.Add(_makeHandyEditorUsecase.Checkbox("Terminate", () => ikRef!.Terminate, it => ikRef!.Terminate = it));
            ikEditors.Add(_makeHandyEditorUsecase.Checkbox("Below", () => ikRef!.Below, it => ikRef!.Below = it));
            ikEditors.Add(_makeHandyEditorUsecase.Checkbox("EnableBias", () => ikRef!.EnableBias, it => ikRef!.EnableBias = it));
            ikEditors.Add(_makeHandyEditorUsecase.ForEdit3("scale", () => new Vector3(ikRef!.ScaleX, ikRef!.ScaleY, ikRef!.ScaleZ), it =>
            {
                ikRef!.ScaleX = it.X;
                ikRef!.ScaleY = it.Y;
                ikRef!.ScaleZ = it.Z;
            }));
            ikEditors.Add(_makeHandyEditorUsecase.ForEdit3("rotate", () => new Vector3(ikRef!.RotateX, ikRef!.RotateY, ikRef!.RotateZ), it =>
            {
                ikRef!.RotateX = it.X;
                ikRef!.RotateY = it.Y;
                ikRef!.RotateZ = it.Z;
            }));
            ikEditors.Add(_makeHandyEditorUsecase.ForEdit3("translation", () => new Vector3(ikRef!.TranslateX, ikRef!.TranslateY, ikRef!.TranslateZ), it =>
            {
                ikRef!.TranslateX = it.X;
                ikRef!.TranslateY = it.Y;
                ikRef!.TranslateZ = it.Z;
            }));

            Func<Mdlx.Bone>? getFkBone = null;
            Mdlx.Bone? fkBone = null;
            var fkEditors = new List<HandyEditorController>();
            fkEditors.Add(_makeHandyEditorUsecase.InputInt("joint index", () => fkBone!.Index, it => fkBone!.Index = it));
            fkEditors.Add(_makeHandyEditorUsecase.InputInt("parent index", () => fkBone!.Parent, it => fkBone!.Parent = it));
            fkEditors.Add(_makeHandyEditorUsecase.ForEdit3("scale",
                () => new Vector3(fkBone!.ScaleX, fkBone!.ScaleY, fkBone!.ScaleZ),
                it =>
                {
                    fkBone!.ScaleX = it.X;
                    fkBone!.ScaleY = it.Y;
                    fkBone!.ScaleZ = it.Z;
                }
            ));
            fkEditors.Add(_makeHandyEditorUsecase.ForEdit3("rotate",
                () => new Vector3(fkBone!.RotationX, fkBone!.RotationY, fkBone!.RotationZ),
                it =>
                {
                    fkBone!.RotationX = it.X;
                    fkBone!.RotationY = it.Y;
                    fkBone!.RotationZ = it.Z;
                }
            ));
            fkEditors.Add(_makeHandyEditorUsecase.ForEdit3("translation",
                () => new Vector3(fkBone!.TranslationX, fkBone!.TranslationY, fkBone!.TranslationZ),
                it =>
                {
                    fkBone!.TranslationX = it.X;
                    fkBone!.TranslationY = it.Y;
                    fkBone!.TranslationZ = it.Z;
                }
            ));

            return () =>
            {
                if (jointAge.NeedToCatchUpAnyOf(msetAge))
                {
                    // load

                    var absIndex = _loadedModel.SelectedJointIndex;
                    var numFk = _loadedModel.FKJointDescriptions.Count;
                    if (numFk <= absIndex)
                    {
                        // ik
                        getIkHelper = () => _loadedModel.MotionData!.IKHelpers[absIndex - numFk];
                        ikRef = getIkHelper();
                        ikEditors.LoadAll();
                        editor = EditorType.Ik;
                    }
                    else if (0 <= absIndex)
                    {
                        // fk
                        getFkBone = () => _loadedModel.InternalFkBones[absIndex];
                        fkBone = getFkBone();
                        fkEditors.LoadAll();
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
                                if (editor == EditorType.Ik)
                                {
                                    ikRef = getIkHelper!();
                                    ikEditors.SaveAll();

                                    _loadedModel.SendBackMotionData.TurnOn();
                                }
                                if (editor == EditorType.Fk)
                                {
                                    _errorMessages.Add(new Exception("fkBone is read only"));
                                }
                            });
                        });

                        if (editor == EditorType.Fk)
                        {
                            fkEditors.RenderAll();
                        }

                        if (editor == EditorType.Ik)
                        {
                            ikEditors.RenderAll();
                        }
                    },
                        menuBar: true
                    );
                }
            };
        }
    }
}
