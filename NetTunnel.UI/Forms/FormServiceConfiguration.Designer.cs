namespace NetTunnel.UI.Forms
{
    partial class FormServiceConfiguration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormServiceConfiguration));
            buttonCancel = new Button();
            buttonSave = new Button();
            labelManagementPort = new Label();
            labelManagementPortRSASize = new Label();
            labelEndpointBufferSize = new Label();
            labelMessageQueryTimeoutMs = new Label();
            labelTunnelAndEndpointHeartbeatDelayMs = new Label();
            labelTunnelEncryptionKeySize = new Label();
            labelStaleEndpointExpirationMs = new Label();
            textBoxManagementPort = new TextBox();
            textBoxManagementPortRSASize = new TextBox();
            textBoxEndpointBufferSize = new TextBox();
            textBoxMessageQueryTimeoutMs = new TextBox();
            textBoxTunnelAndEndpointHeartbeatDelayMs = new TextBox();
            textBoxTunnelEncryptionKeySize = new TextBox();
            textBoxStaleEndpointExpirationMs = new TextBox();
            checkBoxDebugLogging = new CheckBox();
            checkBoxManagementUseSSL = new CheckBox();
            labelInitialReceiveBufferSize = new Label();
            labelMaxReceiveBufferSize = new Label();
            labelReceiveBufferGrowthRate = new Label();
            textBoxInitialReceiveBufferSize = new TextBox();
            textBoxMaxReceiveBufferSize = new TextBox();
            textBoxReceiveBufferGrowthRate = new TextBox();
            tabControlBody = new TabControl();
            tabPageManagement = new TabPage();
            tabPageTunnels = new TabPage();
            tabPageEndpoints = new TabPage();
            tabPageMisc = new TabPage();
            tabControlBody.SuspendLayout();
            tabPageManagement.SuspendLayout();
            tabPageTunnels.SuspendLayout();
            tabPageEndpoints.SuspendLayout();
            tabPageMisc.SuspendLayout();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(433, 428);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(350, 428);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 1;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // labelManagementPort
            // 
            labelManagementPort.AutoSize = true;
            labelManagementPort.Location = new Point(29, 23);
            labelManagementPort.Name = "labelManagementPort";
            labelManagementPort.Size = new Size(103, 15);
            labelManagementPort.TabIndex = 3;
            labelManagementPort.Text = "Management port";
            // 
            // labelManagementPortRSASize
            // 
            labelManagementPortRSASize.AutoSize = true;
            labelManagementPortRSASize.Location = new Point(8, 52);
            labelManagementPortRSASize.Name = "labelManagementPortRSASize";
            labelManagementPortRSASize.Size = new Size(124, 15);
            labelManagementPortRSASize.TabIndex = 4;
            labelManagementPortRSASize.Text = "Management RSA size";
            // 
            // labelEndpointBufferSize
            // 
            labelEndpointBufferSize.AutoSize = true;
            labelEndpointBufferSize.Location = new Point(88, 84);
            labelEndpointBufferSize.Name = "labelEndpointBufferSize";
            labelEndpointBufferSize.Size = new Size(112, 15);
            labelEndpointBufferSize.TabIndex = 5;
            labelEndpointBufferSize.Text = "Endpoint buffer size";
            // 
            // labelMessageQueryTimeoutMs
            // 
            labelMessageQueryTimeoutMs.AutoSize = true;
            labelMessageQueryTimeoutMs.Location = new Point(20, 137);
            labelMessageQueryTimeoutMs.Name = "labelMessageQueryTimeoutMs";
            labelMessageQueryTimeoutMs.Size = new Size(158, 15);
            labelMessageQueryTimeoutMs.TabIndex = 8;
            labelMessageQueryTimeoutMs.Text = "Message query timeout (ms)";
            // 
            // labelTunnelAndEndpointHeartbeatDelayMs
            // 
            labelTunnelAndEndpointHeartbeatDelayMs.AutoSize = true;
            labelTunnelAndEndpointHeartbeatDelayMs.Location = new Point(3, 21);
            labelTunnelAndEndpointHeartbeatDelayMs.Name = "labelTunnelAndEndpointHeartbeatDelayMs";
            labelTunnelAndEndpointHeartbeatDelayMs.Size = new Size(197, 15);
            labelTunnelAndEndpointHeartbeatDelayMs.TabIndex = 9;
            labelTunnelAndEndpointHeartbeatDelayMs.Text = "Tunnel and endpoint heartbeat (ms)";
            // 
            // labelTunnelEncryptionKeySize
            // 
            labelTunnelEncryptionKeySize.AutoSize = true;
            labelTunnelEncryptionKeySize.Location = new Point(30, 21);
            labelTunnelEncryptionKeySize.Name = "labelTunnelEncryptionKeySize";
            labelTunnelEncryptionKeySize.Size = new Size(148, 15);
            labelTunnelEncryptionKeySize.TabIndex = 10;
            labelTunnelEncryptionKeySize.Text = "Tunnel encryption key-size";
            // 
            // labelStaleEndpointExpirationMs
            // 
            labelStaleEndpointExpirationMs.AutoSize = true;
            labelStaleEndpointExpirationMs.Location = new Point(34, 50);
            labelStaleEndpointExpirationMs.Name = "labelStaleEndpointExpirationMs";
            labelStaleEndpointExpirationMs.Size = new Size(166, 15);
            labelStaleEndpointExpirationMs.TabIndex = 11;
            labelStaleEndpointExpirationMs.Text = "Stale endpoint expiration (ms)";
            // 
            // textBoxManagementPort
            // 
            textBoxManagementPort.Location = new Point(138, 20);
            textBoxManagementPort.Name = "textBoxManagementPort";
            textBoxManagementPort.Size = new Size(100, 23);
            textBoxManagementPort.TabIndex = 12;
            // 
            // textBoxManagementPortRSASize
            // 
            textBoxManagementPortRSASize.Location = new Point(138, 49);
            textBoxManagementPortRSASize.Name = "textBoxManagementPortRSASize";
            textBoxManagementPortRSASize.Size = new Size(100, 23);
            textBoxManagementPortRSASize.TabIndex = 13;
            // 
            // textBoxEndpointBufferSize
            // 
            textBoxEndpointBufferSize.Location = new Point(206, 81);
            textBoxEndpointBufferSize.Name = "textBoxEndpointBufferSize";
            textBoxEndpointBufferSize.Size = new Size(100, 23);
            textBoxEndpointBufferSize.TabIndex = 14;
            // 
            // textBoxMessageQueryTimeoutMs
            // 
            textBoxMessageQueryTimeoutMs.Location = new Point(184, 134);
            textBoxMessageQueryTimeoutMs.Name = "textBoxMessageQueryTimeoutMs";
            textBoxMessageQueryTimeoutMs.Size = new Size(100, 23);
            textBoxMessageQueryTimeoutMs.TabIndex = 16;
            // 
            // textBoxTunnelAndEndpointHeartbeatDelayMs
            // 
            textBoxTunnelAndEndpointHeartbeatDelayMs.Location = new Point(206, 18);
            textBoxTunnelAndEndpointHeartbeatDelayMs.Name = "textBoxTunnelAndEndpointHeartbeatDelayMs";
            textBoxTunnelAndEndpointHeartbeatDelayMs.Size = new Size(100, 23);
            textBoxTunnelAndEndpointHeartbeatDelayMs.TabIndex = 17;
            // 
            // textBoxTunnelEncryptionKeySize
            // 
            textBoxTunnelEncryptionKeySize.Location = new Point(184, 18);
            textBoxTunnelEncryptionKeySize.Name = "textBoxTunnelEncryptionKeySize";
            textBoxTunnelEncryptionKeySize.Size = new Size(100, 23);
            textBoxTunnelEncryptionKeySize.TabIndex = 18;
            // 
            // textBoxStaleEndpointExpirationMs
            // 
            textBoxStaleEndpointExpirationMs.Location = new Point(206, 47);
            textBoxStaleEndpointExpirationMs.Name = "textBoxStaleEndpointExpirationMs";
            textBoxStaleEndpointExpirationMs.Size = new Size(100, 23);
            textBoxStaleEndpointExpirationMs.TabIndex = 19;
            // 
            // checkBoxDebugLogging
            // 
            checkBoxDebugLogging.AutoSize = true;
            checkBoxDebugLogging.Location = new Point(14, 21);
            checkBoxDebugLogging.Name = "checkBoxDebugLogging";
            checkBoxDebugLogging.Size = new Size(110, 19);
            checkBoxDebugLogging.TabIndex = 20;
            checkBoxDebugLogging.Text = "Debug logging?";
            checkBoxDebugLogging.UseVisualStyleBackColor = true;
            // 
            // checkBoxManagementUseSSL
            // 
            checkBoxManagementUseSSL.AutoSize = true;
            checkBoxManagementUseSSL.Location = new Point(244, 22);
            checkBoxManagementUseSSL.Name = "checkBoxManagementUseSSL";
            checkBoxManagementUseSSL.Size = new Size(71, 19);
            checkBoxManagementUseSSL.TabIndex = 21;
            checkBoxManagementUseSSL.Text = "Use SSL?";
            checkBoxManagementUseSSL.UseVisualStyleBackColor = true;
            // 
            // labelInitialReceiveBufferSize
            // 
            labelInitialReceiveBufferSize.AutoSize = true;
            labelInitialReceiveBufferSize.Location = new Point(46, 50);
            labelInitialReceiveBufferSize.Name = "labelInitialReceiveBufferSize";
            labelInitialReceiveBufferSize.Size = new Size(132, 15);
            labelInitialReceiveBufferSize.TabIndex = 22;
            labelInitialReceiveBufferSize.Text = "Initial buffer size (bytes)";
            // 
            // labelMaxReceiveBufferSize
            // 
            labelMaxReceiveBufferSize.AutoSize = true;
            labelMaxReceiveBufferSize.Location = new Point(52, 79);
            labelMaxReceiveBufferSize.Name = "labelMaxReceiveBufferSize";
            labelMaxReceiveBufferSize.Size = new Size(126, 15);
            labelMaxReceiveBufferSize.TabIndex = 23;
            labelMaxReceiveBufferSize.Text = "Max buffer size (bytes)";
            // 
            // labelReceiveBufferGrowthRate
            // 
            labelReceiveBufferGrowthRate.AutoSize = true;
            labelReceiveBufferGrowthRate.Location = new Point(54, 108);
            labelReceiveBufferGrowthRate.Name = "labelReceiveBufferGrowthRate";
            labelReceiveBufferGrowthRate.Size = new Size(124, 15);
            labelReceiveBufferGrowthRate.TabIndex = 24;
            labelReceiveBufferGrowthRate.Text = "Buffer growth rate (%)";
            // 
            // textBoxInitialReceiveBufferSize
            // 
            textBoxInitialReceiveBufferSize.Location = new Point(184, 47);
            textBoxInitialReceiveBufferSize.Name = "textBoxInitialReceiveBufferSize";
            textBoxInitialReceiveBufferSize.Size = new Size(100, 23);
            textBoxInitialReceiveBufferSize.TabIndex = 25;
            // 
            // textBoxMaxReceiveBufferSize
            // 
            textBoxMaxReceiveBufferSize.Location = new Point(184, 76);
            textBoxMaxReceiveBufferSize.Name = "textBoxMaxReceiveBufferSize";
            textBoxMaxReceiveBufferSize.Size = new Size(100, 23);
            textBoxMaxReceiveBufferSize.TabIndex = 26;
            // 
            // textBoxReceiveBufferGrowthRate
            // 
            textBoxReceiveBufferGrowthRate.Location = new Point(184, 105);
            textBoxReceiveBufferGrowthRate.Name = "textBoxReceiveBufferGrowthRate";
            textBoxReceiveBufferGrowthRate.Size = new Size(100, 23);
            textBoxReceiveBufferGrowthRate.TabIndex = 27;
            // 
            // tabControlBody
            // 
            tabControlBody.Controls.Add(tabPageManagement);
            tabControlBody.Controls.Add(tabPageTunnels);
            tabControlBody.Controls.Add(tabPageEndpoints);
            tabControlBody.Controls.Add(tabPageMisc);
            tabControlBody.Location = new Point(12, 12);
            tabControlBody.Name = "tabControlBody";
            tabControlBody.SelectedIndex = 0;
            tabControlBody.Size = new Size(500, 410);
            tabControlBody.TabIndex = 28;
            // 
            // tabPageManagement
            // 
            tabPageManagement.Controls.Add(textBoxManagementPortRSASize);
            tabPageManagement.Controls.Add(labelManagementPort);
            tabPageManagement.Controls.Add(labelManagementPortRSASize);
            tabPageManagement.Controls.Add(textBoxManagementPort);
            tabPageManagement.Controls.Add(checkBoxManagementUseSSL);
            tabPageManagement.Location = new Point(4, 24);
            tabPageManagement.Name = "tabPageManagement";
            tabPageManagement.Padding = new Padding(3);
            tabPageManagement.Size = new Size(492, 382);
            tabPageManagement.TabIndex = 0;
            tabPageManagement.Text = "Management";
            tabPageManagement.UseVisualStyleBackColor = true;
            // 
            // tabPageTunnels
            // 
            tabPageTunnels.Controls.Add(textBoxTunnelEncryptionKeySize);
            tabPageTunnels.Controls.Add(textBoxReceiveBufferGrowthRate);
            tabPageTunnels.Controls.Add(textBoxMessageQueryTimeoutMs);
            tabPageTunnels.Controls.Add(textBoxMaxReceiveBufferSize);
            tabPageTunnels.Controls.Add(labelMessageQueryTimeoutMs);
            tabPageTunnels.Controls.Add(labelTunnelEncryptionKeySize);
            tabPageTunnels.Controls.Add(textBoxInitialReceiveBufferSize);
            tabPageTunnels.Controls.Add(labelInitialReceiveBufferSize);
            tabPageTunnels.Controls.Add(labelReceiveBufferGrowthRate);
            tabPageTunnels.Controls.Add(labelMaxReceiveBufferSize);
            tabPageTunnels.Location = new Point(4, 24);
            tabPageTunnels.Name = "tabPageTunnels";
            tabPageTunnels.Padding = new Padding(3);
            tabPageTunnels.Size = new Size(492, 382);
            tabPageTunnels.TabIndex = 1;
            tabPageTunnels.Text = "Tunnels";
            tabPageTunnels.UseVisualStyleBackColor = true;
            // 
            // tabPageEndpoints
            // 
            tabPageEndpoints.Controls.Add(labelTunnelAndEndpointHeartbeatDelayMs);
            tabPageEndpoints.Controls.Add(labelStaleEndpointExpirationMs);
            tabPageEndpoints.Controls.Add(textBoxEndpointBufferSize);
            tabPageEndpoints.Controls.Add(labelEndpointBufferSize);
            tabPageEndpoints.Controls.Add(textBoxStaleEndpointExpirationMs);
            tabPageEndpoints.Controls.Add(textBoxTunnelAndEndpointHeartbeatDelayMs);
            tabPageEndpoints.Location = new Point(4, 24);
            tabPageEndpoints.Name = "tabPageEndpoints";
            tabPageEndpoints.Size = new Size(492, 382);
            tabPageEndpoints.TabIndex = 2;
            tabPageEndpoints.Text = "Endpoints";
            tabPageEndpoints.UseVisualStyleBackColor = true;
            // 
            // tabPageMisc
            // 
            tabPageMisc.Controls.Add(checkBoxDebugLogging);
            tabPageMisc.Location = new Point(4, 24);
            tabPageMisc.Name = "tabPageMisc";
            tabPageMisc.Size = new Size(492, 382);
            tabPageMisc.TabIndex = 3;
            tabPageMisc.Text = "Misc.";
            tabPageMisc.UseVisualStyleBackColor = true;
            // 
            // FormServiceConfiguration
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(518, 466);
            Controls.Add(tabControlBody);
            Controls.Add(buttonSave);
            Controls.Add(buttonCancel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormServiceConfiguration";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel";
            tabControlBody.ResumeLayout(false);
            tabPageManagement.ResumeLayout(false);
            tabPageManagement.PerformLayout();
            tabPageTunnels.ResumeLayout(false);
            tabPageTunnels.PerformLayout();
            tabPageEndpoints.ResumeLayout(false);
            tabPageEndpoints.PerformLayout();
            tabPageMisc.ResumeLayout(false);
            tabPageMisc.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button buttonCancel;
        private Button buttonSave;
        private Label labelManagementPort;
        private Label labelManagementPortRSASize;
        private Label labelEndpointBufferSize;
        private Label labelMessageQueryTimeoutMs;
        private Label labelTunnelAndEndpointHeartbeatDelayMs;
        private Label labelTunnelEncryptionKeySize;
        private Label labelStaleEndpointExpirationMs;
        private TextBox textBoxManagementPort;
        private TextBox textBoxManagementPortRSASize;
        private TextBox textBoxEndpointBufferSize;
        private TextBox textBoxMessageQueryTimeoutMs;
        private TextBox textBoxTunnelAndEndpointHeartbeatDelayMs;
        private TextBox textBoxTunnelEncryptionKeySize;
        private TextBox textBoxStaleEndpointExpirationMs;
        private CheckBox checkBoxDebugLogging;
        private CheckBox checkBoxManagementUseSSL;
        private Label labelInitialReceiveBufferSize;
        private Label labelMaxReceiveBufferSize;
        private Label labelReceiveBufferGrowthRate;
        private TextBox textBoxInitialReceiveBufferSize;
        private TextBox textBoxMaxReceiveBufferSize;
        private TextBox textBoxReceiveBufferGrowthRate;
        private TabControl tabControlBody;
        private TabPage tabPageManagement;
        private TabPage tabPageTunnels;
        private TabPage tabPageEndpoints;
        private TabPage tabPageMisc;
    }
}