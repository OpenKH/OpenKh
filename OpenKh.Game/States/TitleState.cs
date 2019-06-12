using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;
using Xe.Drawing;

namespace OpenKh.Game.States
{
    public class TitleState : IState
    {
        private ArchiveManager archiveManager;
        private MonoDrawing drawing;
        private Layout titleLayout;
        private IEnumerable<ISurface> titleImages;
        private LayoutRenderer layoutRenderer;

        public void Initialize(StateInitDesc initDesc)
        {
            archiveManager = initDesc.ArchiveManager;
            drawing = new MonoDrawing(initDesc.GraphicsDevice.GraphicsDevice);

            archiveManager.LoadArchive("menu/fm/title.2ld");
            titleLayout = archiveManager.Get<Layout>("titl");
            titleImages = archiveManager.Get<Imgz>("titl")?.Images?.Select(x => drawing.CreateSurface(x));
            layoutRenderer = new LayoutRenderer(titleLayout, drawing, titleImages);
        }

        public void Destroy()
        {
            throw new System.NotImplementedException();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            layoutRenderer.FrameIndex++;
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            layoutRenderer.Draw();
        }
    }
}
