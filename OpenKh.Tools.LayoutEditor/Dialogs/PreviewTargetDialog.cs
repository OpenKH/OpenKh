using Binarysharp.MSharp;
using Binarysharp.MSharp.Native;
using ImGuiNET;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using OpenKh.Common;
using System.IO;

using OpenKh.Tools.LayoutEditor;

namespace OpenKh.Tools.LayoutEditor.Dialogs
{
    public class PreviewTargetDialog : IDisposable
    {
        private byte[] _nameBuff;

        public ulong FetchedPointer;
        public Bar FetchedFile;

        private Dictionary<string, ulong> _pointMap = new Dictionary<string, ulong>()
        {
            { "field2d/us/zz0command.2dd", 0x9B8348 }
        };

        public PreviewTargetDialog()
        {
            FetchedFile = null;
            _nameBuff = new byte[32];
        }

        public void Run()
        {
            ImGui.Text("Please enter the name of the file you want to target for preview.");
            ImGui.InputText("", _nameBuff, 32);

            if (ImGui.Button("Open"))
            {
                var _findProcess = Process.GetProcessesByName("KINGDOM HEARTS II FINAL MIX");

                if (_findProcess == null || _findProcess.Length == 0x00)
                    MessageBox.Show("Kingdom Hearts II was not detected\nPlease ensure that it is running!", "Error 405: Process not found!", MessageBoxButton.OK, MessageBoxImage.Warning);

                else
                {
                    Hypervisor.AttachProcess(_findProcess[0], 0x0000000);
                    var _sharpHook = new MemorySharp(_findProcess[0]);

                    var _fileName = Encoding.ASCII.GetString(_nameBuff).Replace("\0", "");

                    var _findPoint = _pointMap.FirstOrDefault(x => x.Key == _fileName);
                    var _fileFind = _sharpHook[(IntPtr)0x39A820].Execute<int>(Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions.MicrosoftX64, _fileName, -1);

                    if (_fileFind != 0x00 || _findPoint.Key != null)
                    {
                        FetchedPointer = _findPoint.Key == null ? Hypervisor.Read<ulong>(Hypervisor.MemoryOffset + (ulong)(_fileFind + 0x58), true) : Hypervisor.Read<ulong>(_findPoint.Value);
                        var _fileSize = _sharpHook[(IntPtr)0x39E2F0].Execute<int>(_fileName);

                        var _barRead = Hypervisor.ReadArray(FetchedPointer, _fileSize, true);

                        var _barCount = BitConverter.ToInt32(_barRead, 0x04);
                        var _barOffset = BitConverter.ToInt32(_barRead, 0x08);

                        using (var _stream = new MemoryStream(_barRead, true))
                        {
                            _stream.Position = 0x10;

                            for (int i = 0; i < _barCount; i++)
                            {
                                var _streamRead = BitConverter.ToUInt32(_barRead, 0x10 + (0x10 * i) + 0x08);
                                var _newValue = _streamRead - _barOffset;

                                _stream.Position = 0x10 + (0x10 * i) + 0x08;
                                _stream.Write(_newValue);
                            }

                            _stream.Position = 0x00;
                            FetchedFile = Bar.Read(_stream);
                        }

                        ImGui.CloseCurrentPopup();
                    }

                    else
                        MessageBox.Show("File is not present in the game cache!\nPlease enter the correct name!", "Error 404: File not found!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
