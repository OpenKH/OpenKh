using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class BonesManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly ComputeSpriteIconUvUsecase _computeSpriteIconUvUsecase;
        private readonly ITextureBinder _textureBinder;
        private readonly Texture2D _spriteIcons;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly IntPtr _spriteIconsPtr;

        public BonesManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            CreateSpriteIconsTextureUsecase createSpriteIconsTextureUsecase,
            ITextureBinder textureBinder,
            ComputeSpriteIconUvUsecase computeSpriteIconUvUsecase
        )
        {
            _computeSpriteIconUvUsecase = computeSpriteIconUvUsecase;
            _textureBinder = textureBinder;
            _spriteIcons = createSpriteIconsTextureUsecase();
            _loadedModel = loadedModel;
            _settings = settings;
            _spriteIconsPtr = _textureBinder.BindTexture(_spriteIcons);
        }

        private record JointDef(int AbsIndex, string Display, int SpriteIconIndex)
        {

        }

        public Action CreateWindowRunnable()
        {
            var jointAge = _loadedModel.JointDescriptionsAge.Branch().MarkDirty();
            var jointDefs = new List<JointDef>();
            var configAge = _loadedModel.Kh2PresetsAge.Branch().MarkDirty();
            var scrollAge = _loadedModel.SelectedJointIndexAge.Branch(false);

            return () =>
            {
                if (_settings.ViewBones)
                {
                    var windowClosed = !ForWindow("Bones manager", () =>
                    {
                        ForEdit("showFk", () => _settings.ViewFkBones, it =>
                        {
                            _settings.ViewFkBones = it;
                            _settings.Save();
                        });
                        ImGui.SameLine();
                        ForEdit("showIk", () => _settings.ViewIkBones, it =>
                        {
                            _settings.ViewIkBones = it;
                            _settings.Save();
                        });

                        if (jointAge.NeedToCatchUpAnyOf(configAge))
                        {
                            var fkView = _loadedModel.GetActiveFkBoneViews?.Invoke();
                            var ikView = _loadedModel.GetActiveIkBoneViews?.Invoke();

                            int FindSpriteIconIndex(int index)
                            {
                                return fkView?
                                    .LastOrDefault(
                                        it => it.I == index
                                    )?
                                    .SpriteIcon ?? 0;
                            }

                            string FindFkJointName(int index)
                            {
                                return fkView?
                                    .LastOrDefault(
                                        it => it.I == index
                                    )?
                                    .Name ?? "";
                            }

                            string FindIkJointName(int absIndex)
                            {
                                return ikView?
                                    .LastOrDefault(
                                        it => it.I == absIndex
                                    )?
                                    .Name ?? "";
                            }

                            jointDefs.Clear();
                            jointDefs.AddRange(
                                _loadedModel.FKJointDescriptions
                                    .Select(
                                        (joint, index) => new JointDef(
                                            index,
                                            $"FK<{joint.Index}> {new string('.', joint.Depth)} {FindFkJointName(index)}",
                                            FindSpriteIconIndex(joint.Index)
                                        )
                                    )
                            );
                            jointDefs.AddRange(
                                _loadedModel.IKJointDescriptions
                                    .Select(
                                        (joint, index) => new JointDef(
                                            joint.Index,
                                            $"IK<{joint.Index}> {new string('.', joint.Depth)} {FindIkJointName(joint.Index)}",
                                            FindSpriteIconIndex(joint.Index)
                                        )
                                    )
                            );
                        }

                        var needToScroll = scrollAge.NeedToCatchUp();

                        if (ImGui.BeginChild("bonesList"))
                        {
                            foreach (var jointDef in jointDefs)
                            {
                                var isSelected = _loadedModel.SelectedJointIndex == jointDef.AbsIndex;

                                var uv = _computeSpriteIconUvUsecase.Compute(jointDef.SpriteIconIndex & 255);
                                ImGui.Image(_spriteIconsPtr, new Vector2(16, 16), uv.Uv0, uv.Uv1);

                                ImGui.SameLine();
                                if (ImGui.Selectable(jointDef.Display, isSelected))
                                {
                                    _loadedModel.SelectedJointIndex = jointDef.AbsIndex;
                                    _loadedModel.SelectedJointIndexAge.Bump();
                                    scrollAge.CatchUp();
                                }

                                if (isSelected && needToScroll)
                                {
                                    ImGui.SetScrollHereY();
                                }
                            }

                            ImGui.EndChild();
                        }
                    });

                    if (windowClosed)
                    {
                        _settings.ViewBones = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
