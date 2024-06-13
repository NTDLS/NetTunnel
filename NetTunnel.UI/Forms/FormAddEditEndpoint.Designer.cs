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
            textBoxInboundPort = new TextBox();
            labelListenPort = new Label();
            textBoxOutboundAddress = new TextBox();
            labelTerminationAddress = new Label();
            textBoxOutboundPort = new TextBox();
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
            comboBoxTrafficType = new ComboBox();
            label2 = new Label();
            tabControlBody.SuspendLayout();
            tabPageEndpoint.SuspendLayout();
            tabPageHttpHeaders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewHTTPHeaders).BeginInit();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(13, 29);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(369, 23);
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
            // textBoxInboundPort
            // 
            textBoxInboundPort.Location = new Point(13, 82);
            textBoxInboundPort.Name = "textBoxInboundPort";
            textBoxInboundPort.Size = new Size(74, 23);
            textBoxInboundPort.TabIndex = 1;
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
            // textBoxOutboundAddress
            // 
            textBoxOutboundAddress.Location = new Point(93, 82);
            textBoxOutboundAddress.Name = "textBoxOutboundAddress";
            textBoxOutboundAddress.Size = new Size(191, 23);
            textBoxOutboundAddress.TabIndex = 2;
            // 
            // labelTerminationAddress
            // 
            labelTerminationAddress.AutoSize = true;
            labelTerminationAddress.Location = new Point(93, 64);
            labelTerminationAddress.Name = "labelTerminationAddress";
            labelTerminationAddress.Size = new Size(147, 15);
            labelTerminationAddress.TabIndex = 15;
            labelTerminationAddress.Text = "Termination address / host";
            // 
            // textBoxOutboundPort
            // 
            textBoxOutboundPort.Location = new Point(290, 82);
            textBoxOutboundPort.Name = "textBoxOutboundPort";
            textBoxOutboundPort.Size = new Size(92, 23);
            textBoxOutboundPort.TabIndex = 3;
            // 
            // labelTerminationPort
            // 
            labelTerminationPort.AutoSize = true;
            labelTerminationPort.Location = new Point(290, 64);
            labelTerminationPort.Name = "labelTerminationPort";
            labelTerminationPort.Size = new Size(92, 15);
            labelTerminationPort.TabIndex = 17;
            labelTerminationPort.Text = "Destination Port";
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(533, 365);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 4;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
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
            tabPageEndpoint.Controls.Add(textBoxOutboundPort);
            tabPageEndpoint.Controls.Add(textBoxInboundPort);
            tabPageEndpoint.Controls.Add(labelTerminationPort);
            tabPageEndpoint.Controls.Add(labelTerminationAddress);
            tabPageEndpoint.Controls.Add(textBoxOutboundAddress);
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
            tabPageHttpHeaders.Controls.Add(comboBoxTrafficType);
            tabPageHttpHeaders.Controls.Add(label2);
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
            label1.Size = new Size(276, 15);
            label1.TabIndex = 13;
            label1.Text = "HTTP header rules (only applicable for HTTP traffic)";
            // 
            // comboBoxTrafficType
            // 
            comboBoxTrafficType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxTrafficType.FormattingEnabled = true;
            comboBoxTrafficType.Location = new Point(13, 28);
            comboBoxTrafficType.Margin = new Padding(4, 3, 4, 3);
            comboBoxTrafficType.Name = "comboBoxTrafficType";
            comboBoxTrafficType.Size = new Size(157, 23);
            comboBoxTrafficType.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 10);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(65, 15);
            label2.TabIndex = 11;
            label2.Text = "Traffic type";
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
            ResumeLayout(false);
        }

        #endregion

        private TextBox textBoxName;
        private Label labelName;
        private TextBox textBoxInboundPort;
        private Label labelListenPort;
        private TextBox textBoxOutboundAddress;
        private Label labelTerminationAddress;
        private TextBox textBoxOutboundPort;
        private Label labelTerminationPort;
        private Button buttonSave;
        private Button buttonCancel;
        private TabControl tabControlBody;
        private TabPage tabPageEndpoint;
        private TabPage tabPageHttpHeaders;
        private Label label1;
        private ComboBox comboBoxTrafficType;
        private Label label2;
        private DataGridView dataGridViewHTTPHeaders;
        private DataGridViewCheckBoxColumn columnEnabled;
        private DataGridViewComboBoxColumn columnType;
        private DataGridViewComboBoxColumn columnVerb;
        private DataGridViewTextBoxColumn columnHeader;
        private DataGridViewComboBoxColumn columnAction;
        private DataGridViewTextBoxColumn columnValue;
    }
}