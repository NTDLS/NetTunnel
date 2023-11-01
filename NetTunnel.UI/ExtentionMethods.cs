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

        public static void InvokeDeleteItem(this ListView grid, ListViewItem item)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke(InvokeDeleteItem, new object[] { grid, item });
            }
            else
            {
                grid.Items.Remove(item);
            }
        }

        public static void InvokeClearRows(this ListView grid)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke(InvokeClearRows, grid);
            }
            else
            {
                grid.Items.Clear();
            }
        }
    }
}
