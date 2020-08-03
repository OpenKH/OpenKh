using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using OpenKh.Tools.Common.CustomImGui;
using System;
using System.Collections.Generic;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio
{

    public class App : IDisposable
    {
        private readonly MonoGameImGuiBootstrap _bootstrap;
        private bool _exitFlag = false;

        private Dictionary<Keys, Action> _keyMapping = new Dictionary<Keys, Action>();
        private string _fileName;

        public string Title
        {
            get
            {
                return $"{FileName ?? "unloaded"} | {MonoGameImGuiBootstrap.ApplicationName}";
            }
        }

        private string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                UpdateTitle();
            }
        }

        private bool IsFileOpen => !string.IsNullOrEmpty(FileName);

        public App(MonoGameImGuiBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;
            AddKeyMapping(Keys.O, MenuFileOpen);
            AddKeyMapping(Keys.S, MenuFileSave);
        }

        public bool MainLoop()
        {
            ProcessKeyMapping();

            ImGuiEx.MainWindow(() =>
            {
                MainMenu();
                MainWindow();
            });

            return _exitFlag;
        }

        public void Dispose()
        {
        }

        private void MainWindow()
        {
            ImGui.Text("No files loaded at the moment");
        }

        void MainMenu()
        {
            ForMenuBar(() =>
            {
                ForMenu("File", () =>
                {
                    ForMenuItem("Open...", "CTRL+O", MenuFileOpen);
                    ForMenuItem("Save", "CTRL+S", MenuFileSave, IsFileOpen);
                    ForMenuItem("Save as...", MenuFileSaveAs, IsFileOpen);
                    ImGui.Separator();
                    ForMenu("Preferences", () =>
                    {
                    });
                    ImGui.Separator();
                    ForMenuItem("Exit", MenuFileExit);
                });
                ForMenu("Help", () =>
                {
                    ForMenuItem("About", ShowAboutDialog);
                });
            });
        }

        private void MenuFileOpen()
        {
            //FileDialog.OnOpen(fileName =>
            //{
            //    OpenFile(fileName);
            //}, Filters);
        }

        private void MenuFileSave()
        {
            if (!string.IsNullOrEmpty(FileName))
                SaveFile(FileName, FileName);
            else
                MenuFileSaveAs();
        }

        private void MenuFileSaveAs()
        {
            //FileDialog.OnSave(fileName =>
            //{
            //    SaveFile(FileName, fileName);
            //    FileName = fileName;
            //}, Filters);
        }

        private void MenuFileExit() => _exitFlag = true;


        public void OpenFile(string fileName)
        {
            try
            {
                bool isSuccess = false;

                if (isSuccess)
                    FileName = fileName;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        public void SaveFile(string previousFileName, string fileName)
        {
        }

        private void UpdateTitle()
        {
            _bootstrap.Title = Title;
        }

        private void AddKeyMapping(Keys key, Action action)
        {
            _keyMapping[key] = action;
        }

        private void ProcessKeyMapping()
        {
            var k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.LeftControl))
            {
                var keys = k.GetPressedKeys();
                foreach (var key in keys)
                {
                    if (_keyMapping.TryGetValue(key, out var action))
                        action();
                }
            }
        }

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowAboutDialog() =>
            MessageBox.Show("OpenKH is amazing.");
    }
}
