using ImGuiNET;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using OpenKh.Kh2;
using System.IO;
using OpenKh.Tools.Common.CustomImGui;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace OpenKh.Tools.LayoutEditor
{
    public class AppLayoutEditor : IApp, ISaveBar, ITextureBinder, IDisposable
    {
        private readonly MonoGameImGuiBootstrap _bootStrap;
        private readonly Layout _layout;
        private readonly List<Imgd> _images;

        public object SelectedLayoutEntry { get; private set; }

        public AppLayoutEditor(
            MonoGameImGuiBootstrap bootstrap,
            IEditorSettings settings,
            Layout layout,
            IEnumerable<Imgd> images)
        {
            _bootStrap = bootstrap;
            _layout = layout;
            _images = images.ToList();
        }

        public void Menu()
        {
        }

        public bool Run()
        {
            ImGui.Text("This is the layout editor!");

            return true;
        }

        public Bar.Entry SaveAnimation(string name)
        {
            var stream = new MemoryStream();
            _layout.Write(stream);

            return new Bar.Entry
            {
                Name = name,
                Stream = stream,
                Type = Bar.EntryType.Layout
            };
        }

        public Bar.Entry SaveTexture(string name)
        {
            var stream = new MemoryStream();
            Imgz.Write(stream, _images);

            return new Bar.Entry
            {
                Name = name,
                Stream = stream,
                Type = Bar.EntryType.Imgz
            };
        }

        public void Dispose()
        {
        }

        public IntPtr BindTexture(Texture2D texture) =>
            _bootStrap.BindTexture(texture);

        public void UnbindTexture(IntPtr id) =>
            _bootStrap.UnbindTexture(id);

        public void RebindTexture(IntPtr id, Texture2D texture) =>
            _bootStrap.RebindTexture(id, texture);
    }
}
