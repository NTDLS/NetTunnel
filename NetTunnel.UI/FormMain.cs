using NetTunnel.ClientAPI;

namespace NetTunnel.UI
{

    public partial class FormMain : Form
    {
        public NtClient? _client { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _client = new NtClient("http://localhost:52845/");

            _client.Configuration.Login("admin", "abcdefg");
        }

        public new void Dispose()
        {
            _client?.Dispose();
            base.Dispose();
        }
    }
}