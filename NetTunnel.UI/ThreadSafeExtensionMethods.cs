namespace NetTunnel.UI
{
    public static class ThreadSafeExtensionMethods
    {
        public static void ThreadSafeClose(this Form form, DialogResult result)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(() => form.ThreadSafeClose(result));
            }
            else
            {
                form.DialogResult = result;
                form.Close();
            }
        }

        public static DialogResult ThreadSafeMessageBox(this Form form, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (form.InvokeRequired)
            {
                return form.Invoke(new Func<DialogResult>(() => form.ThreadSafeMessageBox(message, title, buttons, icon)));
            }
            else
            {
                return MessageBox.Show(form, message, title, buttons, icon);
            }
        }

        public static DialogResult ThreadSafeMessageBox(this Form form, string message, string title, MessageBoxButtons buttons)
        {
            if (form.InvokeRequired)
            {
                return form.Invoke(new Func<DialogResult>(() => form.ThreadSafeMessageBox(message, title, buttons)));
            }
            else
            {
                return MessageBox.Show(form, message, title, buttons);
            }
        }

        public static DialogResult ThreadSafeMessageBox(this Form form, string message, string title)
        {
            if (form.InvokeRequired)
            {
                return form.Invoke(new Func<DialogResult>(() => form.ThreadSafeMessageBox(message, title)));
            }
            else
            {
                return MessageBox.Show(form, message, title);
            }
        }

        public static void ThreadSafeEnable(this Control control, bool enabled)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => control.Enabled = enabled));
            }
            else
            {
                control.Enabled = enabled;
            }
        }

        public static void ThreadSafeDeleteItem(this ListView grid, ListViewItem item)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke(new Action<ListViewItem>(grid.ThreadSafeDeleteItem), item);
            }
            else
            {
                grid.Items.Remove(item);
            }
        }

        public static void ThreadSafeClearRows(this ListView grid)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke(new Action(grid.ThreadSafeClearRows));
            }
            else
            {
                grid.Items.Clear();
            }
        }
    }
}
