using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormCreateInboundTunnel : Form
    {
        private readonly ClientWrapper? _client;

        public FormCreateInboundTunnel()
        {
            InitializeComponent();
        }

        public FormCreateInboundTunnel(ClientWrapper client)
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

            try
            {
                textBoxName.GetAndValidateText("You must specify a name. This is for your identification only.");

                var tunnelId = Guid.NewGuid(); //The TunnelId is the same on both services.

                var outboundTunnel = new NtTunnelInboundConfiguration(tunnelId, textBoxName.Text);

                buttonAdd.InvokeEnableControl(false);
                buttonCancel.InvokeEnableControl(false);

                try
                {
                    //ConfigureTunnelPair(remoteClient, outboundTunnel, inboundTunnel);

                    this.InvokeClose(DialogResult.OK);

                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to login to the remote tunnel: {ex.Message}.");
                }
            }
            catch (Exception ex)
            {
                buttonAdd.InvokeEnableControl(true);
                buttonCancel.InvokeEnableControl(true);

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
