using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormTunnelProperties : Form
    {
        private readonly ServiceClient? _client;
        private readonly DirectionalKey _tunnelKey;
        private bool _firstShown = true;

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

            Text = $"{FriendlyName} : Tunnel Properties";

            Shown += FormTunnelProperties_Shown;

            listViewProperties.MouseUp += ListViewProperties_MouseUp;
        }

        private void ListViewProperties_MouseUp(object? sender, MouseEventArgs e)
        {
            var menu = new ContextMenuStrip();

            var selectedItem = listViewProperties.SelectedItems.Count == 0 ? listViewProperties.SelectedItems[0] : null;

            if (selectedItem != null)
            {
                menu.Items.Add("Copy to clipboard");
            }

            menu.Show(listViewProperties, new Point(e.X, e.Y));

            menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
            {
                menu.Hide();

                if (selectedItem != null && e.ClickedItem?.Text == "Copy to clipboard")
                {
                    Clipboard.SetText(selectedItem.SubItems[0].Text);

                }
            };
        }

        private void FormTunnelProperties_Shown(object? sender, EventArgs e)
        {
            if (!_firstShown)
            {
                return;
            }
            _firstShown = false;

            var progressForm = new ProgressForm(FriendlyName, "Getting properties...");

            progressForm.Execute(() =>
            {
                try
                {
                    var result = _client.EnsureNotNull().QueryGetTunnelProperties(_tunnelKey);

                    this.InvokeSetText($"{FriendlyName} : {result.Properties.Name} Properties");

                    PopulateListView(result.Properties);
                }
                catch (Exception ex)
                {
                    progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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

            listViewProperties.BeginUpdate();

            foreach (var property in typeof(TunnelStatisticsProperties).GetProperties())
            {
                var item = new ListViewItem(NTDLS.Helpers.Text.SeperateCamelCase(property.Name));
                string value = property?.GetValue(tunnelStats, null)?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    item.SubItems.Add(value);
                    listViewProperties.Items.Add(item);
                }
            }

            listViewProperties.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProperties.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);

            listViewProperties.EndUpdate();
        }
    }
}
