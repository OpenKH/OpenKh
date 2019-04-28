using kh.kh2;
using System.Collections.Generic;
using Xe.Drawing;

namespace kh.tools.layout
{
    public class LayoutRenderer
    {
        private class Context
        {

        }

        private readonly Layout layout;
        private readonly IDrawing drawing;
        private readonly IEnumerable<ISurface> surfaces;

        public LayoutRenderer(Layout layout, IDrawing drawing, IEnumerable<ISurface> surfaces)
        {
            this.layout = layout;
            this.drawing = drawing;
            this.surfaces = surfaces;
        }

        public void Draw()
        {
        }

        private void DrawLayoutGroup(Layout.L2 l2)
        {

        }

        private void DrawLayout(Layout.L1 l1)
        {

        }

        private void DrawAnimationGroup(Context context, Sequence.Q5 q5)
        {

        }

        private void DrawAnimation(Context context, Sequence.Q4 q4)
        {

        }

        private void DrawFrameGroup(Context context, Sequence.Q3 q3)
        {

        }

        private void DrawFrameExtended(Context context, Sequence.Q2 q2)
        {

        }

        private void DrawFrame(Context context, Sequence.Q1 q1)
        {

        }
    }
}
