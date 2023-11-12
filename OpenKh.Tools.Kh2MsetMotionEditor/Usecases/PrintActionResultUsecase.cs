using ImGuiNET;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class PrintActionResultUsecase
    {
        private static readonly Vector4 ErrorColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 SuccessColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

        internal void Print(ActionResult loadResult)
        {
            switch (loadResult.Type)
            {
                case ActionResultType.Success:
                    ImGui.TextColored(SuccessColor, loadResult.Message);
                    break;
                case ActionResultType.Failure:
                    ImGui.TextColored(ErrorColor, loadResult.Message);
                    break;
            }
        }
    }
}
