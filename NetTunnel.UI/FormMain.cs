using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;

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
            Shown += FormMain_Shown;
        }

        private void FormMain_Shown(object? sender, EventArgs e)
        {
            using (var formLogin = new FormLogin())
            {
                if (formLogin.ShowDialog() == DialogResult.OK)
                {
                    _client = new NtClient(formLogin.Address, formLogin.Username, formLogin.Password);

                    _client.Endpoint.List().ContinueWith((o) =>
                    {
                        PopulateEndpointGrid(o.Result.Collection);
                    });
                }
                else
                {
                    Close();
                }
            }
        }

        private void PopulateEndpointGrid(List<NtEndpoint> endpoints)
        {
            foreach (var endpoint in endpoints)
            {
                AddEndpointToGrid(endpoint);
            }
        }

        void AddEndpointToGrid(NtEndpoint endpoint)
        {
            if (listViewEndpoints.InvokeRequired)
            {
                listViewEndpoints.Invoke(AddEndpointToGrid, endpoint);
            }
            else
            {
                // This code will execute on the UI thread.
                ListViewItem item = new ListViewItem(endpoint.Name);
                item.SubItems.Add(endpoint.Direction.ToString());
                item.SubItems.Add($"{endpoint.Port}");

                listViewEndpoints.Items.Add(item);
            }
        }

        public new void Dispose()
        {
            _client?.Dispose();
            base.Dispose();
        }
    }
}