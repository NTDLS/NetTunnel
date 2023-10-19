namespace NetTunnel.UI.Forms
{
    public class BaseForm : Form
    {
        public void CloseFormWithResult(DialogResult result)
        {
            if (InvokeRequired)
            {
                Invoke(CloseFormWithResult, result);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public void EnableControl(Control ctrl, bool enable)
        {
            if (InvokeRequired)
            {
                Invoke(EnableControl, new object[] { ctrl, enable });
            }
            else
            {
                ctrl.Enabled = enable;
            }
        }
    }
}
