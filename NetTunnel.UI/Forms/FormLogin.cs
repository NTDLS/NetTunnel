using NetTunnel.Library;
using NetTunnel.Library.ReliablePayloads.Notification.UI;
using NTDLS.Helpers;
using NTDLS.Persistence;
using NTDLS.ReliableMessaging;
using NTDLS.WinFormsHelpers;
using System.Security.Cryptography;
using System.Text;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public ServiceClient? ResultingClient { get; private set; } = null;
        private readonly DelegateLogger _messageBoxHandler = UIUtility.CreateActiveWindowMessageBoxLogger(NtLogSeverity.Exception);
        private readonly Utility.ServiceLogDelegate _serviceLogDelegate;
        private string? _savedPasswordHash;

        public FormLogin(Utility.ServiceLogDelegate serviceLogDelegate)
        {
            InitializeComponent();

            _serviceLogDelegate = serviceLogDelegate;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelAddress, textBoxAddress],
                "Host name, domain, or IP address of the NetTunnel service you want to connect to.");

            toolTips.AddControls([labelPort, textBoxPort],
                "Port of the NetTunnel service you want to connect to.");

            toolTips.AddControls([labelUsername, textBoxUsername],
                "Username to use when connecting to the NetTunnel service.");

            toolTips.AddControls([labelPassword, textBoxPassword],
                "Password for the user to use when connecting to the NetTunnel service.");

            #endregion

            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;

            Exceptions.Ignore(() =>
            {
                var preferences = LocalUserApplicationData.LoadFromDisk(FriendlyName, new UILoginPreferences());

                textBoxAddress.Text = preferences.Address;
                textBoxPort.Text = preferences.Port;
                textBoxUsername.Text = preferences.Username;

                if (preferences.Password != null)
                {
                    try
                    {
                        var entropy = Encoding.UTF8.GetBytes($"{preferences.Address}{preferences.Port}{preferences.Username}{Environment.MachineName.ToUpper()}");
                        _savedPasswordHash = Encoding.UTF8.GetString(ProtectedData.Unprotect(preferences.Password, entropy, DataProtectionScope.CurrentUser));
                        textBoxPassword.Text = _savedPasswordHash;
                    }
                    catch
                    {
                        _savedPasswordHash = null;
                        textBoxPassword.Text = string.Empty;
                    }
                }
                else
                {
                    _savedPasswordHash = null;
                    textBoxPassword.Text = string.Empty;
                }
            });

            //If there is a saved password, check the "Remember password" box. This doesn't necessarily
            // mean that the password is correct, but it's a hint to the user that their password is saved.
            checkBoxRememberPassword.Checked = _savedPasswordHash != null;

            textBoxAddress.TextChanged += (object? sender, EventArgs e) => ClearPassword();
            textBoxPort.TextChanged += (object? sender, EventArgs e) => ClearPassword();
            textBoxUsername.TextChanged += (object? sender, EventArgs e) => ClearPassword();
            textBoxPassword.TextChanged += (object? sender, EventArgs e) => ClearPassword();

            this.Shown += FormLogin_Shown   ;
        }

        private void FormLogin_Shown(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAddress.Text)) textBoxAddress.Focus();
            else if (string.IsNullOrEmpty(textBoxPort.Text)) textBoxPort.Focus();
            else if (string.IsNullOrEmpty(textBoxUsername.Text)) textBoxUsername.Focus();
            else textBoxPassword.Focus();
        }

        private void ClearPassword()
        {
            _savedPasswordHash = null;
            textBoxPassword.Text = string.Empty;
            checkBoxRememberPassword.Checked = false;
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var port = textBoxPort.GetAndValidateNumeric(1, 65535, "A port number between [min] and [max] is required.");
                var username = textBoxUsername.GetAndValidateText("A username is required.");
                var address = textBoxAddress.GetAndValidateText("A hostname, domain, or IP address is required.");
                string? passwordHash;

                if (_savedPasswordHash != null && textBoxPassword.Text == _savedPasswordHash)
                {
                    //Using saved password.
                    passwordHash = _savedPasswordHash;
                }
                else
                {
                    //Using given password.
                    passwordHash = Utility.ComputeSha256Hash(textBoxPassword.Text);
                }

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
                            Password = null
                        };

                        if (checkBoxRememberPassword.Checked)
                        {
                            try
                            {
                                var entropy = Encoding.UTF8.GetBytes($"{preferences.Address}{preferences.Port}{preferences.Username}{Environment.MachineName.ToUpper()}");
                                preferences.Password = ProtectedData.Protect(Encoding.UTF8.GetBytes(passwordHash), entropy, DataProtectionScope.CurrentUser);
                            }
                            catch
                            {
                                preferences.Password = null;
                            }
                        }

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
