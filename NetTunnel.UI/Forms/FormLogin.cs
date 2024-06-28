using NetTunnel.Library;
using NetTunnel.Library.ReliablePayloads.Notification.UI;
using NTDLS.Helpers;
using NTDLS.Persistence;
using NTDLS.ReliableMessaging;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public ServiceClient? ResultingClient { get; private set; } = null;
        private readonly DelegateLogger _messageBoxHandler = UIUtility.CreateActiveWindowMessageBoxLogger(NtLogSeverity.Exception);
        private readonly Utility.ServiceLogDelegate _serviceLogDelegate;

        public FormLogin(Utility.ServiceLogDelegate serviceLogDelegate)
        {
            InitializeComponent();

            _serviceLogDelegate = serviceLogDelegate;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelAddress, textBoxAddress],
                "The host name, domain or IP address of the NetTunnel service you want to connect to.");

            toolTips.AddControls([labelPort, textBoxPort],
                "The port of the NetTunnel service you want to connect to.");

            toolTips.AddControls([labelUsername, textBoxUsername],
                "The username to use when connecting to the NetTunnel service.");

            toolTips.AddControls([labelPassword, textBoxPassword],
                "The password for the user to use when connecting to the NetTunnel service.");

            #endregion

            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;

            Exceptions.Ignore(() =>
            {
                var preferences = LocalUserApplicationData.LoadFromDisk(FriendlyName, new UILoginPreferences());

                textBoxAddress.Text = preferences.Address;
                textBoxPort.Text = preferences.Port;
                textBoxUsername.Text = preferences.Username;
            });

#if DEBUG
            textBoxPassword.Text = "123456789";
#endif
            textBoxPassword.Focus();
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                int port = textBoxPort.GetAndValidateNumeric(1, 65535, "A port number between [min] and [max] is required.");
                string username = textBoxUsername.GetAndValidateText("A username is required.");
                string passwordHash = Utility.ComputeSha256Hash(textBoxPassword.Text);
                string address = textBoxAddress.GetAndValidateText("A hostname or IP address is required.");

                var progressForm = new ProgressForm(FriendlyName, "Logging in...");

                progressForm.Execute(() =>
                {
                    try
                    {
                        var client = ServiceClient.UICreateConnectAndLogin(_messageBoxHandler, address, port, username, passwordHash);

                        client.Client.OnNotificationReceived += (RmContext context, IRmNotification payload) =>
                        {
                            if (payload is UILoggerNotification logger)
                            {
                                _serviceLogDelegate?.Invoke(logger.Timestamp, logger.Severity, logger.Text);
                            }
                        };

                        var preferences = new UILoginPreferences()
                        {
                            Address = textBoxAddress.Text,
                            Port = textBoxPort.Text,
                            Username = textBoxUsername.Text,
                        };

                        LocalUserApplicationData.SaveToDisk(FriendlyName, preferences);

                        ResultingClient = client;
                        this.InvokeClose(DialogResult.OK);
                    }
                    catch (Exception ex)
                    {
                        progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonAbout_Click(object sender, EventArgs e)
        {
            using var form = new FormAbout();
            form.ShowDialog();
        }
    }
}
