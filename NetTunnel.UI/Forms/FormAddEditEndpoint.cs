using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.Localization;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEditEndpoint : Form
    {
        private readonly EndpointConfiguration? _existingEndpoint;
        private readonly NtDirection _direction = NtDirection.Undefined;
        private readonly ServiceClient? _client;
        private readonly TunnelDisplay? _tunnel;

        public EndpointConfiguration Endpoint { get; private set; } = new();

        /// <summary>
        /// Creates a form adding a new or editing an existing endpoint for a connected tunnel.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, TunnelDisplay tunnel, EndpointConfiguration existingEndpoint)
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _tunnel = tunnel;
            _existingEndpoint = existingEndpoint;
            _direction = existingEndpoint.Direction;

            PopulateForm();
        }

        /// <summary>
        /// Creates a form adding a new or editing an existing endpoint for a user account.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, EndpointConfiguration existingEndpoint)
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _existingEndpoint = existingEndpoint;
            _direction = existingEndpoint.Direction;

            PopulateForm();
        }

        /// <summary>
        /// Creates a form adding a new or editing an existing endpoint for a user account.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, NtDirection direction)
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _direction = direction;

            PopulateForm();
        }

        /// <summary>
        /// Creates a form for a adding a new endpoint.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, TunnelDisplay tunnel, NtDirection direction)
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _tunnel = tunnel;
            _direction = direction;

            PopulateForm();
        }

        private void PopulateForm()
        {
            Text = _existingEndpoint == null
                ? (_direction == NtDirection.Inbound ? Localize("NetTunnel : Add Inbound Endpoint") : Localize("NetTunnel : Add Outbound Endpoint"))
                : (_direction == NtDirection.Inbound ? Localize("NetTunnel : Edit Inbound Endpoint") : Localize("NetTunnel : Edit Outbound Endpoint"));

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelName, textBoxName],
                Localize("Name or description you want to use to identify this endpoint."));

            toolTips.AddControls([labelListenPort, textBoxListenPort],
                Localize("Port that will accept new connections at the inbound endpoint."));

            toolTips.AddControls([labelTerminationAddress, textBoxTerminationAddress],
                Localize("Host name, domain or IP address that the outbound endpoint will make a connection to."));

            toolTips.AddControls([labelTerminationPort, textBoxTerminationPort],
                Localize("Host name, domain or IP address that the outbound endpoint will make a connection to."));

            toolTips.AddControls([labelOutboundTrafficType, comboBoxOutboundTrafficType],
                Localize("Type of traffic the outbound endpoint sends to the destination. HTTPS re-encrypts the connection to the destination server; HTTP and Raw send plain traffic."));

            toolTips.AddControls([labelInboundTrafficType, comboBoxInboundTrafficType],
                Localize("Type of traffic accepted by the inbound endpoint. HTTPS terminates TLS using the certificate on the TLS tab. HTTP header rules apply to HTTP and HTTPS."));

            #endregion

            var trafficTypes = new List<ComboItem>
            {
                new ("Raw", NtTrafficType.Raw),
                new ("HTTP", NtTrafficType.Http),
                new ("HTTPS", NtTrafficType.Https)
            };

            comboBoxInboundTrafficType.DisplayMember = "Display";
            comboBoxInboundTrafficType.ValueMember = "Value";
            comboBoxInboundTrafficType.DataSource = trafficTypes;

            comboBoxOutboundTrafficType.DisplayMember = "Display";
            comboBoxOutboundTrafficType.ValueMember = "Value";
            comboBoxOutboundTrafficType.DataSource = new List<ComboItem>(trafficTypes);

            if (_existingEndpoint != null)
            {
                foreach (var rule in _existingEndpoint.HttpHeaderRules)
                {
                    dataGridViewHTTPHeaders.Rows.Add([$"{rule.Enabled}", $"{rule.HeaderType}",
                        $"{rule.Verb}", rule.Name, $"{rule.Action}", rule.Value]);
                }

                comboBoxInboundTrafficType.SelectedValue = _existingEndpoint.InboundTrafficType;
                comboBoxOutboundTrafficType.SelectedValue = _existingEndpoint.OutboundTrafficType;

                textBoxCertificatePath.Text = _existingEndpoint.TlsCertificatePath;
                textBoxCertificatePassword.Text = _existingEndpoint.TlsCertificatePassword;
                checkBoxIgnoreBackendCertificateErrors.Checked = _existingEndpoint.TlsIgnoreBackendCertificateErrors;

                textBoxName.Text = _existingEndpoint.Name;
                textBoxListenPort.Text = $"{_existingEndpoint.InboundPort:n0}";
                textBoxTerminationAddress.Text = _existingEndpoint.OutboundAddress;
                textBoxTerminationPort.Text = $"{_existingEndpoint.OutboundPort:n0}";
            }
            else
            {
                comboBoxInboundTrafficType.SelectedValue = NtTrafficType.Raw;
                comboBoxOutboundTrafficType.SelectedValue = NtTrafficType.Raw;
#if DEBUG

                textBoxName.Text = "Website Redirector Endpoint";
                textBoxListenPort.Text = "8080";
                textBoxTerminationAddress.Text = "127.0.0.1";
                textBoxTerminationPort.Text = "80";
#endif
            }
        }

        private void ButtonBrowseCertificate_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = Localize("Select TLS certificate"),
                Filter = Localize("PKCS#12 certificate (*.pfx;*.p12)|*.pfx;*.p12|All files (*.*)|*.*")
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                textBoxCertificatePath.Text = dialog.FileName;
            }
        }

        private void DataGridViewHTTPHeaders_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
        }

        public FormAddEditEndpoint()
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);
        }

        private void FormAddEndpoint_Load(object sender, EventArgs e)
        {
            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                textBoxName.GetAndValidateText(Localize("You must specify a name."));
                var listenPort = textBoxListenPort.GetAndValidateNumeric(1, 65535, Localize("You must specify a valid listen port between [min] and [max]."));
                textBoxTerminationAddress.GetAndValidateText(Localize("You must specify a termination address (ip, hostname or domain). "));
                var terminationPort = textBoxTerminationPort.GetAndValidateNumeric(1, 65535, Localize("You must specify a valid termination port between [min] and [max]."));

                var endpointHttpHeaderRules = new List<HttpHeaderRule>();

                foreach (DataGridViewRow row in dataGridViewHTTPHeaders.Rows)
                {
                    if (string.IsNullOrWhiteSpace($"{row.Cells[columnHeader.Index].Value}") == false)
                    {
                        var headerType = Enum.Parse<NtHttpHeaderType>($"{row.Cells[columnType.Index].Value}");
                        var verb = Enum.Parse<NtHttpVerb>($"{row.Cells[columnVerb.Index].Value}");

                        if (headerType == NtHttpHeaderType.Response && verb != NtHttpVerb.Any)
                        {
                            //A response does not carry the verb of the request it answers, so such a rule could never match.
                            tabControlBody.SelectedTab = tabPageHttpHeaders;
                            dataGridViewHTTPHeaders.CurrentCell = row.Cells[columnVerb.Index];
                            throw new Exception(Localize("The response header rule for '{0}' must use the verb 'Any'.", $"{row.Cells[columnHeader.Index].Value}"));
                        }

                        endpointHttpHeaderRules.Add(new HttpHeaderRule
                        {
                            Enabled = bool.Parse(row.Cells[columnEnabled.Index].Value?.ToString() ?? "True"),
                            HeaderType = headerType,
                            Action = Enum.Parse<NtHttpHeaderAction>($"{row.Cells[columnAction.Index].Value}"),
                            Name = $"{row.Cells[columnHeader.Index].Value}",
                            Value = $"{row.Cells[columnValue.Index].Value}",
                            Verb = verb
                        });
                    }
                }

                Endpoint = new EndpointConfiguration(
                    _existingEndpoint?.EndpointId ?? Guid.NewGuid(),
                    _direction,
                    textBoxName.Text,
                    textBoxTerminationAddress.Text,
                    listenPort,
                    terminationPort,
                    endpointHttpHeaderRules,
                    Enum.Parse<NtTrafficType>($"{comboBoxInboundTrafficType.SelectedValue}"),
                    Enum.Parse<NtTrafficType>($"{comboBoxOutboundTrafficType.SelectedValue}"))
                {
                    TlsCertificatePath = textBoxCertificatePath.Text.Trim(),
                    TlsCertificatePassword = textBoxCertificatePassword.Text,
                    TlsIgnoreBackendCertificateErrors = checkBoxIgnoreBackendCertificateErrors.Checked
                };

                if (Endpoint.InboundTrafficType == NtTrafficType.Https
                    && string.IsNullOrWhiteSpace(Endpoint.TlsCertificatePath))
                {
                    tabControlBody.SelectedTab = tabPageTls;
                    throw new Exception(Localize("You must specify a certificate (PFX) path to terminate HTTPS traffic."));
                }

                var progressForm = new ProgressForm(FriendlyName, Localize("Saving endpoint..."));

                if (_tunnel != null)
                {
                    //If _tunnel is != null, then we are editing an endpoint for a connected tunnel.
                    progressForm.Execute(() =>
                    {
                        try
                        {
                            _client.UIQueryDistributeUpsertEndpoint(_tunnel.TunnelKey, Endpoint);
                            this.InvokeClose(DialogResult.OK);
                        }
                        catch (Exception ex)
                        {
                            progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    });
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}
