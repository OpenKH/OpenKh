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

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class JointsManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public JointsManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
            _settings = settings;
        }

        private record JointDef(int AbsIndex, string Display)
        {

        }

        public Action CreateWindowRunnable()
        {
            var jointAge = _loadedModel.JointDescriptionsAge.Branch().MarkDirty();
            var jointDefs = new List<JointDef>();

            string FindFkJointName(int index)
            {
                return _loadedModel.ActiveFkBoneViews?
                    .LastOrDefault(
                        it => it.I == index
                    )?
                    .Name ?? "";
            }

            string FindIkJointName(int absIndex)
            {
                return _loadedModel.ActiveIkBoneViews?
                    .LastOrDefault(
                        it => it.I == absIndex
                    )?
                    .Name ?? "";
            }

            return () =>
            {
                if (_settings.ViewJoints)
                {
                    ForWindow("Joints manager", () =>
                    {
                        if (jointAge.NeedToCatchUp())
                        {
                            jointDefs.Clear();
                            jointDefs.AddRange(
                                _loadedModel.FKJointDescriptions
                                    .Select(
                                        (joint, index) => new JointDef(
                                            index,
                                            $"FK<{joint.Index}> {new string('.', joint.Depth)} {FindFkJointName(index)}"
                                        )
                                    )
                            );
                            jointDefs.AddRange(
                                _loadedModel.IKJointDescriptions
                                    .Select(
                                        (joint, index) => new JointDef(
                                            joint.Index,
                                            $"IK<{joint.Index}> {new string('.', joint.Depth)} {FindIkJointName(joint.Index)}"
                                        )
                                    )
                            );
                        }

                        foreach (var jointDef in jointDefs)
                        {
                            if (ImGui.Selectable(jointDef.Display, _loadedModel.SelectedJointIndex == jointDef.AbsIndex))
                            {
                                _loadedModel.SelectedJointIndex = jointDef.AbsIndex;
                                _loadedModel.SelectedJointIndexAge.Bump();
                            }
                        }
                    });
                }
            };
        }
    }
}
