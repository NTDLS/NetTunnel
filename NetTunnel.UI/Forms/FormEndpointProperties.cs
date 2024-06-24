using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormEndpointProperties : Form
    {
        private readonly ServiceClient? _client;
        private readonly DirectionalKey _tunnelKey;
        private readonly DirectionalKey _endpointKey;
        private bool _firstShown = true;
        private bool _formClosing = false;
        private bool _inTimerTick = false;
        private System.Windows.Forms.Timer? _timer;

        public FormEndpointProperties()
        {
            InitializeComponent();
            _tunnelKey = new DirectionalKey();
            _endpointKey = new DirectionalKey();
        }

        public FormEndpointProperties(ServiceClient client, DirectionalKey tunnelKey, DirectionalKey endpointKey)
        {
            InitializeComponent();

            _client = client;
            _endpointKey = endpointKey;
            _tunnelKey = tunnelKey;

            Text = $"{FriendlyName} : Endpoint Properties";

            Shown += FormEndpointProperties_Shown;
            FormClosing += FormEndpointProperties_FormClosing;

            listViewProperties.MouseUp += ListViewProperties_MouseUp;

            _timer = new System.Windows.Forms.Timer()
            {
                Enabled = true,
                Interval = 1000,
            };

            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void FormEndpointProperties_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            if (_inTimerTick)
            {
                e.Cancel = true;

                new Thread(() => //Delayed form close.
                {
                    for (int i = 0; _inTimerTick == true; i++)
                    {
                        if (i == 1000)
                        {
                            this.InvokeMessageBox("A timeout has occurred while waiting on the timer to terminate.",
                                FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            this.InvokeClose(DialogResult.OK);
                            return;
                        }

                        Thread.Sleep(100);
                    }

                    this.InvokeClose(DialogResult.OK);
                }).Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_inTimerTick) return;
            _inTimerTick = true;

            new Thread(() =>
            {
                try
                {
                    try
                    {
                        var result = _client.EnsureNotNull().QueryGetEndpointProperties(_tunnelKey, _endpointKey);
                        listViewProperties.Invoke(PopulateListView, result.Properties);
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }
                finally
                {
                    _inTimerTick = false;
                }
            }).Start();
        }

        private void ListViewProperties_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var menu = new ContextMenuStrip();

                var selectedItem = listViewProperties.SelectedItems.Count == 1 ? listViewProperties.SelectedItems[0] : null;

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
                        Clipboard.SetText(selectedItem.SubItems[1].Text);
                    }
                };
            }
        }

        private void FormEndpointProperties_Shown(object? sender, EventArgs e)
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
                    var result = _client.EnsureNotNull().QueryGetEndpointProperties(_tunnelKey, _endpointKey);
                    this.InvokeSetText($"{FriendlyName} : {result.Properties.Name} Properties");

                    listViewProperties.Invoke(PopulateListView, result.Properties);

                    AutoResizeColumns();
                }
                catch (Exception ex)
                {
                    progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            });
        }

        public void AutoResizeColumns()
        {
            if (listViewProperties.InvokeRequired)
            {
                listViewProperties.Invoke(AutoResizeColumns);
                return;
            }

            listViewProperties.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProperties.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        public void PopulateListView(EndpointPropertiesDisplay props)
        {
            listViewProperties.BeginUpdate();

            var nameIndexes = new Dictionary<string, int>();

            foreach (ListViewItem item in listViewProperties.Items)
            {
                nameIndexes.Add(item.Text, item.Index);
            }

            foreach (var property in typeof(EndpointPropertiesDisplay).GetProperties())
            {
                string name = NTDLS.Helpers.Text.SeperateCamelCase(property.Name);
                string value = property?.GetValue(props, null)?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(name) == false && string.IsNullOrWhiteSpace(value) == false)
                {
                    if (nameIndexes.TryGetValue(name, out var index))
                    {
                        listViewProperties.Items[index].SubItems[1].Text = value;
                    }
                    else
                    {
                        var item = new ListViewItem(name);
                        item.SubItems.Add(value);
                        listViewProperties.Items.Add(item);
                    }
                }
            }

            listViewProperties.EndUpdate();
        }
    }
}
