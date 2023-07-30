using ImGuiNET;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.IO;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class ErrorMessagesWindowUsecase : IWindowRunnableProvider
    {
        private readonly AskOpenFileNowUsecase _askOpenFileNowUsecase;
        private readonly string _caption;
        private readonly ErrorMessages _errorMessages;

        public ErrorMessagesWindowUsecase(
            ErrorMessages errorMessages,
            AskOpenFileNowUsecase askOpenFileNowUsecase
        )
        {
            _askOpenFileNowUsecase = askOpenFileNowUsecase;

            _caption = "ERROR";
            _errorMessages = errorMessages;
        }

        public Action CreateWindowRunnable()
        {
            var visible = false;

            return () =>
            {
                if (_errorMessages.Any() && !visible)
                {
                    ImGui.OpenPopup(_caption);
                    visible = true;
                }

                if (ImGui.BeginPopupModal(_caption, ref visible,
                    ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text($"There are {_errorMessages.Count} error(s):");

                    foreach (var one in _errorMessages.Messages)
                    {
                        ImGui.Separator();
                        ImGui.TextWrapped(one);
                    }

                    ImGui.Separator();

                    if (ImGui.Button("Close"))
                    {
                        _errorMessages.Clear();
                        ImGui.CloseCurrentPopup();
                        visible = false;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Save full messages to a text file"))
                    {
                        FileDialog.OnSave(
                            saveTo =>
                            {
                                try
                                {
                                    File.WriteAllText(saveTo, _errorMessages.GetFullMessages());

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
                            OperatingSystem.IsWindows()
                                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ErrorMessages.txt")
                                : "ErrorMessages.txt"
                        );
                    }

                    ImGui.EndPopup();
                }
                else
                {
                    // closed by user with X button, or already invisible
                    _errorMessages.Clear();
                }
            };
        }
    }
}
