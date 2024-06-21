using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddUser : Form
    {
        private readonly ServiceClient? _client;
        public User? CreatedUser { get; set; }

        public FormAddUser()
        {
            InitializeComponent();
        }

        public FormAddUser(ServiceClient? client)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelUsername, textBoxUsername],
                "The user name which the user will use when logging in to the NetTunnel service.");

            toolTips.AddControls([labelPassword, textBoxPassword, labelConfirmPassword, textBoxConfirmPassword],
                "The password which the user will use when logging in to the NetTunnel service.");

            #endregion

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                string username = textBoxUsername.GetAndValidateText("You must specify a username.");
                string password = Utility.ComputeSha256Hash(textBoxPassword.GetAndValidateText("You must specify a password."));
                string passwordConfirm = Utility.ComputeSha256Hash(textBoxPassword.GetAndValidateText("You must specify a confirm-password."));

                if (password != passwordConfirm)
                {
                    throw new Exception("The password and confirm-passwords must match.");
                }

                var user = new User(username, password);

                var progressForm = new ProgressForm(Constants.FriendlyName, "Creating user...");

                progressForm.Execute(() =>
                {
                    try
                    {
                        _client.QueryCreateUser(user);
                        CreatedUser = user;
                        this.InvokeClose(DialogResult.OK);
                    }
                    catch (Exception ex)
                    {
                        progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
    }
}
