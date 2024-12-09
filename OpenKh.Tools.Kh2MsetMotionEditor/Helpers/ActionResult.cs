namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record ActionResult(ActionResultType Type, string Message)
    {
        public static readonly ActionResult NotRun = new ActionResult(ActionResultType.NotRun, "");
        public static readonly ActionResult Success = new ActionResult(ActionResultType.Success, "Success");
    }
}
