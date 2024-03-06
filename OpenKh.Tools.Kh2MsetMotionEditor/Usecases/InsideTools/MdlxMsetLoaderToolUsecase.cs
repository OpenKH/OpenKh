using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.InsideTools
{
    public class MdlxMsetLoaderToolUsecase : IToolRunnableProvider
    {
        private readonly ErrorMessages _errorMessages;
        private readonly SearchForKh2AssetFileUsecase _searchForKh2AssetFileUsecase;
        private readonly GetMdlxMsetPresets _getMdlxMsetPresets;
        private readonly LayoutOnMultiColumnsUsecase _layoutOnMultiColumnsUsecase;
        private readonly LoadMotionDataUsecase _loadMotionDataUsecase;
        private readonly LoadMotionUsecase _loadMotionUsecase;
        private readonly ManageKingdomTextureUsecase _manageKingdomTextureUsecase;
        private readonly PrintActionResultUsecase _printActionResultUsecase;
        private readonly LoadModelUsecase _loadModelUsecase;
        private readonly LoadedModel _loadedModel;

        public MdlxMsetLoaderToolUsecase(
            LoadedModel loadedModel,
            LoadModelUsecase loadModelUsecase,
            PrintActionResultUsecase printActionResultUsecase,
            ManageKingdomTextureUsecase manageKingdomTextureUsecase,
            LoadMotionUsecase loadMotionUsecase,
            LoadMotionDataUsecase loadMotionDataUsecase,
            LayoutOnMultiColumnsUsecase layoutOnMultiColumnsUsecase,
            GetMdlxMsetPresets getMdlxMsetPresets,
            SearchForKh2AssetFileUsecase searchForKh2AssetFileUsecase,
            ErrorMessages errorMessages
        )
        {
            _errorMessages = errorMessages;
            _searchForKh2AssetFileUsecase = searchForKh2AssetFileUsecase;
            _getMdlxMsetPresets = getMdlxMsetPresets;
            _layoutOnMultiColumnsUsecase = layoutOnMultiColumnsUsecase;
            _loadMotionDataUsecase = loadMotionDataUsecase;
            _loadMotionUsecase = loadMotionUsecase;
            _manageKingdomTextureUsecase = manageKingdomTextureUsecase;
            _printActionResultUsecase = printActionResultUsecase;
            _loadModelUsecase = loadModelUsecase;
            _loadedModel = loadedModel;
        }

        public Action CreateToolRunnable()
        {
            string? mdlxFile = Settings.Default.LastLoadedMdlxFile;
            string? msetFile = Settings.Default.LastLoadedMsetFile;
            int presetSelectedIndex = -1;
            ActionResult mdlxMsetResult = new ActionResult(ActionResultType.NotRun, "");
            var selectMotionVisible = false;
            var selectMotionCaption = "Select motion##motionSelector";
            ActionResult msetResult = new ActionResult(ActionResultType.NotRun, "");
            Action? sendBackMotionData = null;

            void LoadMotionAt(int index)
            {
                try
                {
                    sendBackMotionData = _loadMotionDataUsecase.LoadAt(
                        index
                    )
                        .SendBack;

                    _loadedModel.SelectedMotionIndex = index;

                    _loadedModel.OpenMotionPlayerOnce.TurnOn();

                    msetResult = ActionResult.Success;
                }
                catch (Exception ex)
                {
                    msetResult = new ActionResult(ActionResultType.Failure, ex.Message);
                }
            }

            return () =>
            {
                if (_loadedModel.SendBackMotionData.Consume())
                {
                    try
                    {
                        sendBackMotionData?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _errorMessages.Add(new Exception("Send back of motion data failed due to error.", ex));
                    }
                }

                ForHeader("MdlxMsetLoader", () =>
                {
                    var autoLoadOnce = false;
                    string? defaultMotionOnce = null;

                    var presets = _getMdlxMsetPresets();

                    if (presets.Any())
                    {
                        if (ImGui.BeginCombo(
                            "presets",
                            presetSelectedIndex == -1
                            ? "..."
                            : presets
                                .Skip(presetSelectedIndex)
                                .Select(it => it.Label)
                                .First()
                            )
                        )
                        {
                            foreach (var (one, index) in presets.SelectWithIndex())
                            {
                                if (ImGui.Selectable(one.Label, index == presetSelectedIndex))
                                {
                                    presetSelectedIndex = index;
                                    mdlxFile = one.Mdlx;
                                    msetFile = one.Mset;

                                    if (one.AutoLoad)
                                    {
                                        autoLoadOnce = true;
                                        defaultMotionOnce = one.DefaultMotion;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }
                    }

                    ImGui.InputText($"mdlxFile", ref mdlxFile, 256);
                    ImGui.InputText($"msetFile", ref msetFile, 256);

                    if (ImGui.Button("Load##loadMdlxMset") || autoLoadOnce)
                    {
                        try
                        {
                            _manageKingdomTextureUsecase.ClearCache();

                            var mdlxFull = _searchForKh2AssetFileUsecase.ResolveFilePath(mdlxFile!);
                            _loadModelUsecase.OpenModel(mdlxFull!);

                            if (string.IsNullOrEmpty(mdlxFile))
                            {
                                mdlxFile = mdlxFull;
                            }
                            Settings.Default.LastLoadedMdlxFile = mdlxFile;
                            Settings.Default.Save();

                            var msetFull = _searchForKh2AssetFileUsecase.ResolveFilePath(msetFile!);
                            _loadMotionUsecase.OpenMotion(msetFull!);

                            if (string.IsNullOrEmpty(msetFile))
                            {
                                msetFile = msetFull;
                            }
                            Settings.Default.LastLoadedMsetFile = msetFile;
                            Settings.Default.Save();

                            mdlxMsetResult = new ActionResult(ActionResultType.Success, "Success");
                        }
                        catch (Exception ex)
                        {
                            mdlxMsetResult = new ActionResult(ActionResultType.Failure, ex.Message);
                        }
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Save mset##saveMdlxMset"))
                    {
                        try
                        {
                            if (MessageBox.Show("Do you really want to save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                            {
                                mdlxMsetResult = new ActionResult(ActionResultType.Success, "Save cancelled");
                                return;
                            }

                            {
                                if (_loadedModel.AnbFile is string file)
                                {
                                    using var stream = File.Create(file);
                                    Bar.Write(stream, _loadedModel.AnbEntries);
                                    mdlxMsetResult = new ActionResult(ActionResultType.Success, "Anb wrote");
                                }
                            }
                            {
                                if (_loadedModel.MsetFile is string file)
                                {
                                    using var stream = File.Create(file);
                                    Bar.Write(stream, _loadedModel.MsetEntries);
                                    mdlxMsetResult = new ActionResult(ActionResultType.Success, "Mset wrote");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            mdlxMsetResult = new ActionResult(ActionResultType.Failure, ex.Message);
                        }
                    }

                    _printActionResultUsecase.Print(mdlxMsetResult);


                    if (defaultMotionOnce != null)
                    {
                        if (int.TryParse(defaultMotionOnce, out int index))
                        {
                            LoadMotionAt(index);
                        }
                    }

                    if (_loadedModel.MotionList.Any())
                    {
                        var selectedMotionName = _loadedModel.SelectedMotionIndex == -1
                            ? "..."
                            : _loadedModel.MotionList[_loadedModel.SelectedMotionIndex].Label;

                        if (ImGui.Button($"{selectedMotionName}##selectMotion"))
                        {
                            ImGui.OpenPopup(selectMotionCaption);
                            ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);
                            selectMotionVisible = true;
                        }
                        ImGui.SameLine();
                        ImGui.Text("motion");

                        if (ImGui.BeginPopupModal(selectMotionCaption, ref selectMotionVisible,
                            ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal))
                        {
                            var list = _loadedModel.MotionList;

                            var layout = _layoutOnMultiColumnsUsecase.Layout(
                                ImGui.GetWindowSize(),
                                170,
                                ImGui.GetTextLineHeightWithSpacing(),
                                list.Count()
                            );

                            ImGui.Columns(layout.NumColumns);

                            foreach (var cell in layout.Cells)
                            {
                                var index = cell.Index;

                                var display = list[index];

                                if (!display.Valid)
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.3f, 0.3f, 0.3f, 1));
                                }

                                if (ImGui.Selectable($"{display.Label}##motion{index}", _loadedModel.SelectedMotionIndex == index))
                                {
                                    LoadMotionAt(index);
                                }

                                if (!display.Valid)
                                {
                                    ImGui.PopStyleColor();
                                }

                                ImGui.NextColumn();
                            }

                            ImGui.EndPopup();
                        }

                        _printActionResultUsecase.Print(msetResult);
                    }
                    else
                    {
                        ImGui.Text("(No motion loaded yet)");

                        msetResult = ActionResult.NotRun;
                    }
                },
                    openByDefault: true
                );
            };
        }
    }
}
