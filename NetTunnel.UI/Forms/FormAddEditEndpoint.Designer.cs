namespace NetTunnel.UI.Forms
{
    partial class FormAddEditEndpoint
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddEditEndpoint));
            textBoxName = new TextBox();
            labelName = new Label();
            textBoxListenPort = new TextBox();
            labelListenPort = new Label();
            textBoxTerminationAddress = new TextBox();
            labelTerminationAddress = new Label();
            textBoxTerminationPort = new TextBox();
            labelTerminationPort = new Label();
            buttonSave = new Button();
            buttonCancel = new Button();
            tabControlBody = new TabControl();
            tabPageEndpoint = new TabPage();
            tabPageHttpHeaders = new TabPage();
            dataGridViewHTTPHeaders = new DataGridView();
            columnEnabled = new DataGridViewCheckBoxColumn();
            columnType = new DataGridViewComboBoxColumn();
            columnVerb = new DataGridViewComboBoxColumn();
            columnHeader = new DataGridViewTextBoxColumn();
            columnAction = new DataGridViewComboBoxColumn();
            columnValue = new DataGridViewTextBoxColumn();
            label1 = new Label();
            comboBoxInboundTrafficType = new ComboBox();
            labelInboundTrafficType = new Label();
            comboBoxOutboundTrafficType = new ComboBox();
            labelOutboundTrafficType = new Label();
            tabPageTls = new TabPage();
            labelTlsInfo = new Label();
            labelCertificatePath = new Label();
            textBoxCertificatePath = new TextBox();
            buttonBrowseCertificate = new Button();
            labelCertificatePassword = new Label();
            textBoxCertificatePassword = new TextBox();
            checkBoxIgnoreBackendCertificateErrors = new CheckBox();
            tabControlBody.SuspendLayout();
            tabPageEndpoint.SuspendLayout();
            tabPageHttpHeaders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewHTTPHeaders).BeginInit();
            tabPageTls.SuspendLayout();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(13, 29);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(408, 23);
            textBoxName.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(13, 11);
            labelName.Name = "labelName";
            labelName.Size = new Size(115, 15);
            labelName.TabIndex = 8;
            labelName.Text = "Name or description";
            // 
            // textBoxListenPort
            // 
            textBoxListenPort.Location = new Point(13, 82);
            textBoxListenPort.Name = "textBoxListenPort";
            textBoxListenPort.Size = new Size(74, 23);
            textBoxListenPort.TabIndex = 1;
            // 
            // labelListenPort
            // 
            labelListenPort.AutoSize = true;
            labelListenPort.Location = new Point(13, 64);
            labelListenPort.Name = "labelListenPort";
            labelListenPort.Size = new Size(63, 15);
            labelListenPort.TabIndex = 13;
            labelListenPort.Text = "Listen port";
            // 
            // textBoxTerminationAddress
            // 
            textBoxTerminationAddress.Location = new Point(113, 82);
            textBoxTerminationAddress.Name = "textBoxTerminationAddress";
            textBoxTerminationAddress.Size = new Size(191, 23);
            textBoxTerminationAddress.TabIndex = 2;
            // 
            // labelTerminationAddress
            // 
            labelTerminationAddress.AutoSize = true;
            labelTerminationAddress.Location = new Point(113, 64);
            labelTerminationAddress.Name = "labelTerminationAddress";
            labelTerminationAddress.Size = new Size(148, 15);
            labelTerminationAddress.TabIndex = 15;
            labelTerminationAddress.Text = "Termination address / host";
            // 
            // textBoxTerminationPort
            // 
            textBoxTerminationPort.Location = new Point(329, 82);
            textBoxTerminationPort.Name = "textBoxTerminationPort";
            textBoxTerminationPort.Size = new Size(92, 23);
            textBoxTerminationPort.TabIndex = 3;
            // 
            // labelTerminationPort
            // 
            labelTerminationPort.AutoSize = true;
            labelTerminationPort.Location = new Point(329, 64);
            labelTerminationPort.Name = "labelTerminationPort";
            labelTerminationPort.Size = new Size(96, 15);
            labelTerminationPort.TabIndex = 17;
            labelTerminationPort.Text = "Termination port";
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(533, 365);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 4;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += ButtonSave_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(452, 365);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // tabControlBody
            // 
            tabControlBody.Controls.Add(tabPageEndpoint);
            tabControlBody.Controls.Add(tabPageHttpHeaders);
            tabControlBody.Controls.Add(tabPageTls);
            tabControlBody.Location = new Point(10, 10);
            tabControlBody.Name = "tabControlBody";
            tabControlBody.SelectedIndex = 0;
            tabControlBody.Size = new Size(602, 349);
            tabControlBody.TabIndex = 29;
            // 
            // tabPageEndpoint
            // 
            tabPageEndpoint.Controls.Add(labelName);
            tabPageEndpoint.Controls.Add(textBoxName);
            tabPageEndpoint.Controls.Add(labelListenPort);
            tabPageEndpoint.Controls.Add(textBoxTerminationPort);
            tabPageEndpoint.Controls.Add(textBoxListenPort);
            tabPageEndpoint.Controls.Add(labelTerminationPort);
            tabPageEndpoint.Controls.Add(labelTerminationAddress);
            tabPageEndpoint.Controls.Add(textBoxTerminationAddress);
            tabPageEndpoint.Location = new Point(4, 24);
            tabPageEndpoint.Name = "tabPageEndpoint";
            tabPageEndpoint.Size = new Size(594, 321);
            tabPageEndpoint.TabIndex = 2;
            tabPageEndpoint.Text = "Endpoint";
            tabPageEndpoint.UseVisualStyleBackColor = true;
            // 
            // tabPageHttpHeaders
            // 
            tabPageHttpHeaders.Controls.Add(dataGridViewHTTPHeaders);
            tabPageHttpHeaders.Controls.Add(label1);
            tabPageHttpHeaders.Controls.Add(comboBoxInboundTrafficType);
            tabPageHttpHeaders.Controls.Add(labelInboundTrafficType);
            tabPageHttpHeaders.Controls.Add(comboBoxOutboundTrafficType);
            tabPageHttpHeaders.Controls.Add(labelOutboundTrafficType);
            tabPageHttpHeaders.Location = new Point(4, 24);
            tabPageHttpHeaders.Name = "tabPageHttpHeaders";
            tabPageHttpHeaders.Size = new Size(594, 321);
            tabPageHttpHeaders.TabIndex = 3;
            tabPageHttpHeaders.Text = "HTTP Headers";
            tabPageHttpHeaders.UseVisualStyleBackColor = true;
            // 
            // dataGridViewHTTPHeaders
            // 
            dataGridViewHTTPHeaders.BackgroundColor = SystemColors.ButtonFace;
            dataGridViewHTTPHeaders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewHTTPHeaders.Columns.AddRange(new DataGridViewColumn[] { columnEnabled, columnType, columnVerb, columnHeader, columnAction, columnValue });
            dataGridViewHTTPHeaders.Location = new Point(13, 87);
            dataGridViewHTTPHeaders.Name = "dataGridViewHTTPHeaders";
            dataGridViewHTTPHeaders.Size = new Size(572, 227);
            dataGridViewHTTPHeaders.TabIndex = 14;
            // 
            // columnEnabled
            // 
            columnEnabled.HeaderText = "Enabled";
            columnEnabled.Name = "columnEnabled";
            columnEnabled.Width = 55;
            // 
            // columnType
            // 
            columnType.HeaderText = "Type";
            columnType.Items.AddRange(new object[] { "None", "Request", "Response", "Any" });
            columnType.Name = "columnType";
            // 
            // columnVerb
            // 
            columnVerb.HeaderText = "Verb";
            columnVerb.Items.AddRange(new object[] { "Any", "Connect", "Delete", "Get", "Head", "Options", "Patch", "Post", "Put", "Trace" });
            columnVerb.Name = "columnVerb";
            columnVerb.Width = 75;
            // 
            // columnHeader
            // 
            columnHeader.HeaderText = "Header";
            columnHeader.Name = "columnHeader";
            // 
            // columnAction
            // 
            columnAction.HeaderText = "Action";
            columnAction.Items.AddRange(new object[] { "Insert", "Update", "Delete", "Upsert" });
            columnAction.Name = "columnAction";
            columnAction.Width = 75;
            // 
            // columnValue
            // 
            columnValue.HeaderText = "Value";
            columnValue.Name = "columnValue";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 68);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(312, 15);
            label1.TabIndex = 13;
            label1.Text = "HTTP header rules (applicable to HTTP and HTTPS traffic)";
            // 
            // comboBoxInboundTrafficType
            // 
            comboBoxInboundTrafficType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxInboundTrafficType.FormattingEnabled = true;
            comboBoxInboundTrafficType.Location = new Point(13, 28);
            comboBoxInboundTrafficType.Margin = new Padding(4, 3, 4, 3);
            comboBoxInboundTrafficType.Name = "comboBoxInboundTrafficType";
            comboBoxInboundTrafficType.Size = new Size(157, 23);
            comboBoxInboundTrafficType.TabIndex = 12;
            // 
            // labelInboundTrafficType
            // 
            labelInboundTrafficType.AutoSize = true;
            labelInboundTrafficType.Location = new Point(13, 10);
            labelInboundTrafficType.Margin = new Padding(4, 0, 4, 0);
            labelInboundTrafficType.Name = "labelInboundTrafficType";
            labelInboundTrafficType.Size = new Size(112, 15);
            labelInboundTrafficType.TabIndex = 11;
            labelInboundTrafficType.Text = "Inbound traffic type";
            // 
            // comboBoxOutboundTrafficType
            // 
            comboBoxOutboundTrafficType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOutboundTrafficType.FormattingEnabled = true;
            comboBoxOutboundTrafficType.Location = new Point(218, 28);
            comboBoxOutboundTrafficType.Margin = new Padding(4, 3, 4, 3);
            comboBoxOutboundTrafficType.Name = "comboBoxOutboundTrafficType";
            comboBoxOutboundTrafficType.Size = new Size(157, 23);
            comboBoxOutboundTrafficType.TabIndex = 14;
            // 
            // labelOutboundTrafficType
            // 
            labelOutboundTrafficType.AutoSize = true;
            labelOutboundTrafficType.Location = new Point(218, 10);
            labelOutboundTrafficType.Margin = new Padding(4, 0, 4, 0);
            labelOutboundTrafficType.Name = "labelOutboundTrafficType";
            labelOutboundTrafficType.Size = new Size(122, 15);
            labelOutboundTrafficType.TabIndex = 15;
            labelOutboundTrafficType.Text = "Outbound traffic type";
            // 
            // tabPageTls
            // 
            tabPageTls.Controls.Add(labelTlsInfo);
            tabPageTls.Controls.Add(labelCertificatePath);
            tabPageTls.Controls.Add(textBoxCertificatePath);
            tabPageTls.Controls.Add(buttonBrowseCertificate);
            tabPageTls.Controls.Add(labelCertificatePassword);
            tabPageTls.Controls.Add(textBoxCertificatePassword);
            tabPageTls.Controls.Add(checkBoxIgnoreBackendCertificateErrors);
            tabPageTls.Location = new Point(4, 24);
            tabPageTls.Name = "tabPageTls";
            tabPageTls.Size = new Size(594, 321);
            tabPageTls.TabIndex = 4;
            tabPageTls.Text = "TLS";
            tabPageTls.UseVisualStyleBackColor = true;
            // 
            // labelTlsInfo
            // 
            labelTlsInfo.Location = new Point(13, 10);
            labelTlsInfo.Name = "labelTlsInfo";
            labelTlsInfo.Size = new Size(570, 45);
            labelTlsInfo.TabIndex = 0;
            labelTlsInfo.Text = resources.GetString("labelTlsInfo.Text");
            // 
            // labelCertificatePath
            // 
            labelCertificatePath.AutoSize = true;
            labelCertificatePath.Location = new Point(13, 92);
            labelCertificatePath.Name = "labelCertificatePath";
            labelCertificatePath.Size = new Size(230, 15);
            labelCertificatePath.TabIndex = 1;
            labelCertificatePath.Text = "Certificate (PFX) path on the inbound host";
            // 
            // textBoxCertificatePath
            // 
            textBoxCertificatePath.Location = new Point(13, 110);
            textBoxCertificatePath.Name = "textBoxCertificatePath";
            textBoxCertificatePath.Size = new Size(481, 23);
            textBoxCertificatePath.TabIndex = 2;
            // 
            // buttonBrowseCertificate
            // 
            buttonBrowseCertificate.Location = new Point(500, 109);
            buttonBrowseCertificate.Name = "buttonBrowseCertificate";
            buttonBrowseCertificate.Size = new Size(75, 25);
            buttonBrowseCertificate.TabIndex = 3;
            buttonBrowseCertificate.Text = "Browse...";
            buttonBrowseCertificate.UseVisualStyleBackColor = true;
            buttonBrowseCertificate.Click += ButtonBrowseCertificate_Click;
            // 
            // labelCertificatePassword
            // 
            labelCertificatePassword.AutoSize = true;
            labelCertificatePassword.Location = new Point(13, 146);
            labelCertificatePassword.Name = "labelCertificatePassword";
            labelCertificatePassword.Size = new Size(114, 15);
            labelCertificatePassword.TabIndex = 4;
            labelCertificatePassword.Text = "Certificate password";
            // 
            // textBoxCertificatePassword
            // 
            textBoxCertificatePassword.Location = new Point(13, 164);
            textBoxCertificatePassword.Name = "textBoxCertificatePassword";
            textBoxCertificatePassword.Size = new Size(260, 23);
            textBoxCertificatePassword.TabIndex = 5;
            textBoxCertificatePassword.UseSystemPasswordChar = true;
            // 
            // checkBoxIgnoreBackendCertificateErrors
            // 
            checkBoxIgnoreBackendCertificateErrors.AutoSize = true;
            checkBoxIgnoreBackendCertificateErrors.Location = new Point(13, 208);
            checkBoxIgnoreBackendCertificateErrors.Name = "checkBoxIgnoreBackendCertificateErrors";
            checkBoxIgnoreBackendCertificateErrors.Size = new Size(293, 19);
            checkBoxIgnoreBackendCertificateErrors.TabIndex = 6;
            checkBoxIgnoreBackendCertificateErrors.Text = "Ignore certificate errors from the destination server";
            checkBoxIgnoreBackendCertificateErrors.UseVisualStyleBackColor = true;
            // 
            // FormAddEditEndpoint
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(621, 404);
            Controls.Add(tabControlBody);
            Controls.Add(buttonCancel);
            Controls.Add(buttonSave);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAddEditEndpoint";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Endpoint";
            Load += FormAddEndpoint_Load;
            tabControlBody.ResumeLayout(false);
            tabPageEndpoint.ResumeLayout(false);
            tabPageEndpoint.PerformLayout();
            tabPageHttpHeaders.ResumeLayout(false);
            tabPageHttpHeaders.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewHTTPHeaders).EndInit();
            tabPageTls.ResumeLayout(false);
            tabPageTls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox textBoxName;
        private Label labelName;
        private TextBox textBoxListenPort;
        private Label labelListenPort;
        private TextBox textBoxTerminationAddress;
        private Label labelTerminationAddress;
        private TextBox textBoxTerminationPort;
        private Label labelTerminationPort;
        private Button buttonSave;
        private Button buttonCancel;
        private TabControl tabControlBody;
        private TabPage tabPageEndpoint;
        private TabPage tabPageHttpHeaders;
        private TabPage tabPageTls;
        private Label labelTlsInfo;
        private Label labelCertificatePath;
        private TextBox textBoxCertificatePath;
        private Button buttonBrowseCertificate;
        private Label labelCertificatePassword;
        private TextBox textBoxCertificatePassword;
        private CheckBox checkBoxIgnoreBackendCertificateErrors;
        private Label label1;
        private ComboBox comboBoxInboundTrafficType;
        private Label labelInboundTrafficType;
        private ComboBox comboBoxOutboundTrafficType;
        private Label labelOutboundTrafficType;
        private DataGridView dataGridViewHTTPHeaders;
        private DataGridViewCheckBoxColumn columnEnabled;
        private DataGridViewComboBoxColumn columnType;
        private DataGridViewComboBoxColumn columnVerb;
        private DataGridViewTextBoxColumn columnHeader;
        private DataGridViewComboBoxColumn columnAction;
        private DataGridViewTextBoxColumn columnValue;
    }
}