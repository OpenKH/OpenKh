using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using OpenKh.AssimpUtils;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using SharpDX.DXGI;
using Assimp;
using System.Collections.Generic;

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
                        ImGui.SameLine();;
                        if (ImGui.Button($"Export selected motion as FBX##selectMotion"))
                        {
                            if (_loadedModel.SelectedMotionIndex<0)
                            {
                                System.Windows.Forms.MessageBox.Show("No motion selected.", "No motion selected.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                if (_loadedModel.MdlxRenderableList.Count>0&&
                                _loadedModel.MdlxRenderableList[0].Model is Mdlx)
                                {
                                    OpenKh.Engine.Parsers.MdlxParser mParser = new OpenKh.Engine.Parsers.MdlxParser(_loadedModel.MdlxRenderableList[0].Model);
                                    Assimp.Scene scene = Kh2MdlxAssimp.getAssimpScene(mParser);





                                    System.Windows.Forms.SaveFileDialog sfd;
                                    sfd = new System.Windows.Forms.SaveFileDialog();
                                    sfd.Title = "Export model";
                                    sfd.FileName = _loadedModel.MdlxFile + "-"+ selectedMotionName.Replace(" ","_")+"." + AssimpGeneric.GetFormatFileExtension(AssimpGeneric.FileFormat.fbx);
                                    sfd.ShowDialog();
                                    if (sfd.FileName != "")
                                    {
                                        string dirPath = Path.GetDirectoryName(sfd.FileName);

                                        if (!Directory.Exists(dirPath))
                                            return;
                                        dirPath += "\\";


                                        var motionData = _loadedModel.MotionData;

                                        if (motionData != null)
                                        {
                                            Animation animation = new Animation();
                                            animation.Name = selectedMotionName;
                                            animation.TicksPerSecond = 24f;
                                            animation.DurationInTicks = (double)(1 / animation.TicksPerSecond) * motionData.InterpolatedMotionHeader.FrameCount;

                                            for (int b=0;b< mParser.Bones.Count;b++)
                                            {
                                                NodeAnimationChannel nodeAnimationChannel = new NodeAnimationChannel();
                                                nodeAnimationChannel.NodeName = "Bone" + b;
                                                animation.NodeAnimationChannels.Add(nodeAnimationChannel);
                                            }
                                            for (int f = 0; f < motionData.InterpolatedMotionHeader.FrameCount; f++)
                                            {
                                                float timeKey = (float)((1/ animation.TicksPerSecond) * f);

                                                var fkIk = _loadedModel.PoseProvider?.Invoke((float)f);

                                                List<Microsoft.Xna.Framework.Matrix> matrices = new List<Microsoft.Xna.Framework.Matrix>(0);
                                                List<bool> dirtyMatrices = new List<bool>(0);

                                                if (fkIk != null && _loadedModel.MotionData?.IKHelpers is System.Collections.Generic.List<Motion.IKHelper> ikHelperList)
                                                {
                                                    var numFk = fkIk.Fk.Length;
                                                    var numIk = fkIk.Ik.Length;
                                                    var length = fkIk.Fk.Length;

                                                    for (int mIndex=0; mIndex < length; mIndex++)
                                                    {
                                                        matrices.Add(fkIk.Fk[mIndex].ToXnaMatrix());
                                                        dirtyMatrices.Add(true);
                                                    }
                                                }

                                                int dirtyCount;
                                                do
                                                {
                                                    dirtyCount = mParser.Bones.Count;
                                                    for (int b = 0; b < mParser.Bones.Count; b++)
                                                    {
                                                        if (dirtyMatrices[b])
                                                        {
                                                            int childrenDirtyCount = 0;
                                                            for (int j = 0; j < mParser.Bones.Count; j++)
                                                            {
                                                                if (mParser.Bones[j].Parent == b && dirtyMatrices[j])
                                                                    childrenDirtyCount++;
                                                            }
                                                            if (childrenDirtyCount == 0) /* Children's children are all calculated already. */
                                                            {
                                                                if (mParser.Bones[b].Parent >-1)
                                                                {
                                                                    matrices[b] *= Microsoft.Xna.Framework.Matrix.Invert(matrices[mParser.Bones[b].Parent]);
                                                                }
                                                                dirtyMatrices[b] = false;
                                                                dirtyCount--;
                                                            }
                                                        }
                                                        else
                                                            dirtyCount--;
                                                    }
                                                }
                                                while (dirtyCount > 0);

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

                                        AssimpGeneric.ExportScene(scene, AssimpGeneric.FileFormat.fbx, sfd.FileName);
                                        OpenKh.Tools.Kh2MdlxEditor.Views.Main_Window.exportTextures(_loadedModel.MdlxRenderableList[0].Textures, dirPath);
                                        System.Windows.Forms.MessageBox.Show("Export complete.", "Complete", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                                    }
                                }
                            }
                        }

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
