namespace NetTunnel.UI
{
    public static class ExtentionMethods
    {
        public static void CloseFormWithResult(this Form form, DialogResult result)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(() => form.CloseFormWithResult(result));
            }
            else
            {
                form.DialogResult = result;
                form.Close();
            }
        }

        public static void EnableControl(this Form form, Control ctrl, bool enable)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(EnableControl, new object[] { ctrl, enable });
            }
            else
            {
                ctrl.Enabled = enable;
            }
        }
    }
}
