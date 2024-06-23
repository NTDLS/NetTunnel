using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using System.Reflection;

namespace NetTunnel.UI.Forms
{
    public partial class FormTunnelProperties : Form
    {
        private readonly ServiceClient? _client;
        private bool _firstShown = true;
        public DirectionalKey _tunnelKey { get; set; }

        public FormTunnelProperties()
        {
            InitializeComponent();
            _tunnelKey = new DirectionalKey();
        }

        public FormTunnelProperties(ServiceClient client, DirectionalKey tunnelKey)
        {
            InitializeComponent();

            _client = client;
            _tunnelKey = tunnelKey;

            Shown += FormTunnelProperties_Shown;
        }

        private void FormTunnelProperties_Shown(object? sender, EventArgs e)
        {
            if (!_firstShown)
            {
                return;
            }
            _firstShown = false;

            var progressForm = new ProgressForm(Constants.FriendlyName, "Getting properties...");

            progressForm.Execute(() =>
            {
                try
                {
                    var result = _client.EnsureNotNull().QueryGetTunnelProperties(_tunnelKey);

                    PopulateListView(result.Properties);
                }
                catch (Exception ex)
                {
                    progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            });
        }

        public void PopulateListView(TunnelStatisticsProperties tunnelStats)
        {
            if (listViewProperties.InvokeRequired)
            {
                listViewProperties.Invoke(PopulateListView, tunnelStats);
                return;
            }

            listViewProperties.Items.Clear();

            foreach (PropertyInfo property in typeof(TunnelStatisticsProperties).GetProperties())
            {
                ListViewItem item = new ListViewItem(property.Name);
                string value = property?.GetValue(tunnelStats, null)?.ToString() ?? string.Empty;
                item.SubItems.Add(value != null ? value.ToString() : "null");
                listViewProperties.Items.Add(item);
            }
        }
    }
}
