using NetTunnel.Library;
using NTDLS.Persistence;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public ServiceClient? ResultingClient { get; private set; } = null;
        private readonly DelegateLogger _delegateLogger = UIUtility.CreateActiveWindowMessageBoxLogger(NtLogSeverity.Exception);

        public FormLogin()
        {
            InitializeComponent();

            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;

            var preferences = LocalUserApplicationData.LoadFromDisk(Constants.FriendlyName, new UILoginPreferences());

            textBoxAddress.Text = preferences.Address;
            textBoxPort.Text = preferences.Port;
            textBoxUsername.Text = preferences.Username;

#if DEBUG
            textBoxPassword.Text = "123456789";
#endif
            textBoxPassword.Focus();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            buttonLogin.InvokeEnableControl(false);
            buttonCancel.InvokeEnableControl(false);

            try
            {
                int port = textBoxPort.GetAndValidateNumeric(1, 65535, "A port number between [min] and [max] is required.");
                string username = textBoxUsername.GetAndValidateText("A username is required.");
                string passwordHash = Utility.ComputeSha256Hash(textBoxPassword.Text);
                string address = textBoxAddress.GetAndValidateText("A hostname or IP address is required.");

                ServiceClient.CreateConnectAndLogin(_delegateLogger, address, port, username, passwordHash).ContinueWith(x =>
                {
                    if (!x.IsCompletedSuccessfully)
                    {
                        this.InvokeMessageBox(x.Exception?.Message ?? "An unknown exception occurred.",
                            Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                        buttonLogin.InvokeEnableControl(true);
                        buttonCancel.InvokeEnableControl(true);
                        return;
                    }

                    var preferences = new UILoginPreferences()
                    {
                        Address = textBoxAddress.Text,
                        Port = textBoxPort.Text,
                        Username = textBoxUsername.Text,
                    };

                    LocalUserApplicationData.SaveToDisk(Constants.FriendlyName, preferences);

                    ResultingClient = x.Result;

                    this.InvokeClose(DialogResult.OK);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
                buttonLogin.InvokeEnableControl(true);
                buttonCancel.InvokeEnableControl(true);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            using var form = new FormAbout();
            form.ShowDialog();
        }
    }
}
