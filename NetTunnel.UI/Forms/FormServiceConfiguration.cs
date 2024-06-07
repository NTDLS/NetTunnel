using NetTunnel.ClientAPI;

namespace NetTunnel.UI.Forms
{
    public partial class FormServiceConfiguration : Form
    {
        private readonly NtClient? _client;

        public FormServiceConfiguration()
        {
            InitializeComponent();
        }

        public FormServiceConfiguration(NtClient client)
        {
            InitializeComponent();

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }
    }
}
