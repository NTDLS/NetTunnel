﻿using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Service;

namespace NetTunnel.UI.Forms
{
    public partial class FormChangeUserPassword : Form
    {
        private readonly NtClient? _client;
        public NtUser? User { get; private set; }

        public FormChangeUserPassword()
        {
            InitializeComponent();
        }

        public FormChangeUserPassword(NtClient? client, NtUser user)
        {
            InitializeComponent();

            _client = client;

            User = user;

            textBoxUsername.Text = user.Username;

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

            try
            {
                if (textBoxUsername.Text.Length == 0)
                    throw new Exception("You must specify a username.");
                if (textBoxPassword.Text.Length == 0)
                    throw new Exception("You must specify a password.");
                if (textBoxPassword.Text != textBoxConfirmPassword.Text)
                    throw new Exception("The password and confirm-passwords must match.");

                var user = new NtUser(textBoxUsername.Text, Utility.CalculateSHA256(textBoxPassword.Text));

                this.EnableControl(buttonSave, false);

                _client.Security.ChangeUserPassword(user).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        this.EnableControl(buttonSave, true);
                        throw new Exception("Failed to change user password.");
                    }

                    this.CloseFormWithResult(DialogResult.OK);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
