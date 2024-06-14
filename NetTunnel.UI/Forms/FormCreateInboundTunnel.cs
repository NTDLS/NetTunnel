using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormCreateInboundTunnel : Form
    {
        private readonly NtServiceClient? _client;

        public FormCreateInboundTunnel()
        {
            InitializeComponent();
        }

        public FormCreateInboundTunnel(NtServiceClient client)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelName, textBoxName],
                    "The user friendly name of this tunnel.");

            #endregion

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

#if DEBUG
            textBoxName.Text = "My First Tunnel";
#endif
        }

        private void FormCreateInboundTunnel_Load(object sender, EventArgs e) { }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            buttonAdd.InvokeDisable();
            buttonCancel.InvokeDisable();

            try
            {
                textBoxName.GetAndValidateText("You must specify a name. This is for your identification only.");

                var tunnelId = Guid.NewGuid(); //We generate the GUIDs locally.

                var inboundTunnel = new NtTunnelInboundConfiguration(tunnelId, textBoxName.Text);

                _client.CreateInboundTunnel(inboundTunnel).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        this.InvokeClose(DialogResult.OK);
                    }
                    else
                    {
                        this.InvokeMessageBox(t.Exception?.Message ?? "An unknown error occured.",
                            Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                        buttonAdd.InvokeEnable();
                        buttonCancel.InvokeEnable();
                    }
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);

                buttonAdd.InvokeEnable();
                buttonCancel.InvokeEnable();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
