using NetTunnel.ClientAPI;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEndpoint : Form
    {
        private readonly NtClient? _client;

        public FormAddEndpoint(NtClient client)
        {
            InitializeComponent();

            _client = client;

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxName.Text = "Website Redirector Endpoint";
            textBoxListenPort.Text = "8080";
            textBoxTerminationAddress.Text = "127.0.0.1";
            textBoxTerminationPort.Text = "80";
        }

        public FormAddEndpoint()
        {
            InitializeComponent();
        }

        private void FormAddEndpoint_Load(object sender, EventArgs e) { }
    }
}
