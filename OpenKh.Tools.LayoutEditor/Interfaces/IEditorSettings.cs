using OpenKh.Engine.Renders;

namespace OpenKh.Tools.LayoutEditor.Interfaces
{
    public interface IEditorSettings
    {
        delegate void ChangeBackground(object sender, IEditorSettings settings);
        public event ChangeBackground OnChangeBackground;

        //public bool CheckerboardBackground { get; }
        ColorF EditorBackground { get; }
        bool ShowViewportOriginal { get; }
        bool ShowViewportRemix { get; }
        bool IsViewportOnTop { get; }
    }
}
