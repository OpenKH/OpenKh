using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers.HandyEditorSpec
{
    public static class HandyEditorExtensions
    {
        public static void LoadAll(this List<HandyEditorController> list) =>
            list.ForEach(it => it.Load());

        public static void RenderAll(this List<HandyEditorController> list) =>
            list.ForEach(it => it.Render());

        public static void SaveAll(this List<HandyEditorController> list) =>
            list.ForEach(it => it.Save());
    }
}
