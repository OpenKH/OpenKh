using ImGuiNET;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.IO;
using System.Linq;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class NormalMessagesWindowUsecase : IWindowRunnableProvider
    {
        private readonly ErrorMessages _errorMessages;
        private readonly AskOpenFileNowUsecase _askOpenFileNowUsecase;
        private readonly NormalMessages _normalMessages;

        public NormalMessagesWindowUsecase(
            NormalMessages normalMessages,
            ErrorMessages errorMessages,
            AskOpenFileNowUsecase askOpenFileNowUsecase
        )
        {
            _errorMessages = errorMessages;
            _normalMessages = normalMessages;
            _askOpenFileNowUsecase = askOpenFileNowUsecase;
        }

        public Action CreateWindowRunnable()
        {
            return () =>
            {
                foreach (var pair in _normalMessages.GetPairs().ToArray())
                {
                    ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 400), ImGuiCond.FirstUseEver);

                    var caption = $"Message #{pair.Item1}";

                    var windowClosed = !ForWindow(caption, () =>
                    {
                        ImGui.TextWrapped(pair.Item2);

                        ImGui.Separator();

                        if (ImGui.Button("Close"))
                        {
                            _normalMessages.Close(pair.Item1);
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Save full messages to a text file"))
                        {
                            FileDialog.OnSave(
                                saveTo =>
                                {
                                    try
                                    {
                                        File.WriteAllText(saveTo, pair.Item2);

                                        _askOpenFileNowUsecase.AskAndOpen(saveTo);
                                    }
                                    catch (Exception ex)
                                    {
                                        _errorMessages.Add(ex);
                                    }
                                },
                                FileDialogFilterComposer.Compose()
                                    .AddExtensions("Text", "txt")
                                    .AddAllFiles(),
                                $"{caption}.txt"
                            );
                        }
                    });

                    if (windowClosed)
                    {
                        _normalMessages.Close(pair.Item1);
                    }
                }
            };
        }
    }
}
