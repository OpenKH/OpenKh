using ImGuiNET;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers.HandyEditorSpec;
using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class InitialPoseManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly string _popupCaption;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public InitialPoseManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            ErrorMessages errorMessages,
            EditCollectionNoErrorUsecase editCollectionNoErrorUsecase
        )
        {
            _editCollectionNoErrorUsecase = editCollectionNoErrorUsecase;
            _errorMessages = errorMessages;
            _popupCaption = "Select initialPose";
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var list = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);

            return () =>
            {
                if (_settings.ViewInitialPose)
                {
                    ForWindow("InitialPose manager", () =>
                    {
                        var sourceList = _loadedModel.MotionData?.InitialPoses;

                        var saved = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Insert", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.InsertAt(sourceList, selectedIndex, new Motion.InitialPose());
                            });
                            ForMenuItem("Append", () =>
                            {
                                if (saved = _editCollectionNoErrorUsecase.Append(sourceList, new Motion.InitialPose()))
                                {
                                    selectedIndex = sourceList!.Count - 1;
                                }
                            });
                            ForMenuItem("Delete", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.DeleteAt(sourceList, selectedIndex);
                            });
                        });

                        if (saved | nextTimeRefresh.Consume() | age.NeedToCatchUp())
                        {
                            try
                            {
                                list.Clear();
                                list.AddRange(
                                    sourceList!
                                        .Select(it => $"{it}")
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("InitialPoses has error of ToString().", ex));
                            }
                        }

                        if (list.Any())
                        {
                            if (ImGui.DragInt("index", ref selectedIndex, 0.2f, 0, list.Count - 1))
                            {

                            }

                            if (ImGui.BeginCombo($"InitialPose", list.GetAtOrNull(selectedIndex) ?? "..."))
                            {
                                foreach (var (item, index) in list.SelectWithIndex())
                                {
                                    if (ImGui.Selectable(list[index], index == selectedIndex))
                                    {
                                        selectedIndex = index;
                                    }
                                }
                                ImGui.EndCombo();
                            }

                            if (_loadedModel.MotionData?.InitialPoses.GetAtOrNull(selectedIndex) is Motion.InitialPose pose)
                            {
                                ForEdit("BoneId", () => pose.BoneId, it => { pose.BoneId = it; saved = true; });
                                ForEdit("Channel", () => pose.Channel, it => { pose.Channel = (short)Math.Max(0, Math.Min(8, it)); saved = true; });
                                ForEdit("Value", () => pose.Value, it => { pose.Value = it; saved = true; });
                            }
                            else
                            {
                                ImGui.Text("(Editor will appear after selection)");
                            }
                        }
                        else
                        {
                            ImGui.Text("(Collection is empty)");
                        }

                        if (saved)
                        {
                            nextTimeRefresh.TurnOn();
                            _loadedModel.SendBackMotionData.TurnOn();
                        }
                    },
                        menuBar: true
                    );
                }
            };
        }
    }
}
