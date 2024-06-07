namespace NetTunnel.UI.Helpers
{
    internal static class ToolTipHelpers
    {
        internal static void SetToolTip(ToolTip tooltip, Control[] controls, string text)
        {
            foreach (var control in controls)
            {
                tooltip.SetToolTip(control, TextHelpers.InsertLineBreaks(text, 55));
            }
        }

        internal static ToolTip CreateToolTipControl()
        {
            return new ToolTip
            {
                InitialDelay = 100,
                ReshowDelay = 100,
                AutoPopDelay = 5000,
                ShowAlways = true
            };
        }
    }
}
