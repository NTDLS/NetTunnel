﻿namespace NetTunnel.UI.Forms
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
            labelMessageQueryTimeoutMs = new Label();
            labelEndpointHeartbeatDelayMs = new Label();
            labelTunnelCryptographyKeySize = new Label();
            labelStaleEndpointExpirationMs = new Label();
            textBoxManagementPort = new TextBox();
            textBoxMessageQueryTimeoutMs = new TextBox();
            textBoxEndpointHeartbeatDelayMs = new TextBox();
            textBoxTunnelCryptographyKeySize = new TextBox();
            textBoxStaleEndpointExpirationMs = new TextBox();
            labelInitialReceiveBufferSize = new Label();
            labelMaxReceiveBufferSize = new Label();
            labelReceiveBufferGrowthRate = new Label();
            textBoxInitialReceiveBufferSize = new TextBox();
            textBoxMaxReceiveBufferSize = new TextBox();
            textBoxReceiveBufferGrowthRate = new TextBox();
            tabControlBody = new TabControl();
            tabPageManagement = new TabPage();
            tabPageTunnels = new TabPage();
            textBoxPingCadence = new TextBox();
            labelPingCadence = new Label();
            groupBoxBuffering = new GroupBox();
            tabPageEndpoints = new TabPage();
            tabControlBody.SuspendLayout();
            tabPageManagement.SuspendLayout();
            tabPageTunnels.SuspendLayout();
            groupBoxBuffering.SuspendLayout();
            tabPageEndpoints.SuspendLayout();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(350, 431);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(431, 431);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 0;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += ButtonSave_Click;
            // 
            // labelManagementPort
            // 
            labelManagementPort.AutoSize = true;
            labelManagementPort.Location = new Point(29, 23);
            labelManagementPort.Name = "labelManagementPort";
            labelManagementPort.Size = new Size(69, 15);
            labelManagementPort.TabIndex = 3;
            labelManagementPort.Text = "Service port";
            // 
            // labelMessageQueryTimeoutMs
            // 
            labelMessageQueryTimeoutMs.AutoSize = true;
            labelMessageQueryTimeoutMs.Location = new Point(22, 50);
            labelMessageQueryTimeoutMs.Name = "labelMessageQueryTimeoutMs";
            labelMessageQueryTimeoutMs.Size = new Size(158, 15);
            labelMessageQueryTimeoutMs.TabIndex = 8;
            labelMessageQueryTimeoutMs.Text = "Message query timeout (ms)";
            // 
            // labelEndpointHeartbeatDelayMs
            // 
            labelEndpointHeartbeatDelayMs.AutoSize = true;
            labelEndpointHeartbeatDelayMs.Location = new Point(47, 28);
            labelEndpointHeartbeatDelayMs.Name = "labelEndpointHeartbeatDelayMs";
            labelEndpointHeartbeatDelayMs.Size = new Size(135, 15);
            labelEndpointHeartbeatDelayMs.TabIndex = 9;
            labelEndpointHeartbeatDelayMs.Text = "Endpoint heartbeat (ms)";
            // 
            // labelTunnelCryptographyKeySize
            // 
            labelTunnelCryptographyKeySize.AutoSize = true;
            labelTunnelCryptographyKeySize.Location = new Point(18, 21);
            labelTunnelCryptographyKeySize.Name = "labelTunnelCryptographyKeySize";
            labelTunnelCryptographyKeySize.Size = new Size(162, 15);
            labelTunnelCryptographyKeySize.TabIndex = 10;
            labelTunnelCryptographyKeySize.Text = "Tunnel cryptography key-size";
            // 
            // labelStaleEndpointExpirationMs
            // 
            labelStaleEndpointExpirationMs.AutoSize = true;
            labelStaleEndpointExpirationMs.Location = new Point(16, 52);
            labelStaleEndpointExpirationMs.Name = "labelStaleEndpointExpirationMs";
            labelStaleEndpointExpirationMs.Size = new Size(166, 15);
            labelStaleEndpointExpirationMs.TabIndex = 11;
            labelStaleEndpointExpirationMs.Text = "Stale endpoint expiration (ms)";
            // 
            // textBoxManagementPort
            // 
            textBoxManagementPort.Location = new Point(104, 20);
            textBoxManagementPort.Name = "textBoxManagementPort";
            textBoxManagementPort.Size = new Size(100, 23);
            textBoxManagementPort.TabIndex = 0;
            // 
            // textBoxMessageQueryTimeoutMs
            // 
            textBoxMessageQueryTimeoutMs.Location = new Point(186, 47);
            textBoxMessageQueryTimeoutMs.Name = "textBoxMessageQueryTimeoutMs";
            textBoxMessageQueryTimeoutMs.Size = new Size(100, 23);
            textBoxMessageQueryTimeoutMs.TabIndex = 1;
            // 
            // textBoxEndpointHeartbeatDelayMs
            // 
            textBoxEndpointHeartbeatDelayMs.Location = new Point(188, 20);
            textBoxEndpointHeartbeatDelayMs.Name = "textBoxEndpointHeartbeatDelayMs";
            textBoxEndpointHeartbeatDelayMs.Size = new Size(100, 23);
            textBoxEndpointHeartbeatDelayMs.TabIndex = 0;
            // 
            // textBoxTunnelCryptographyKeySize
            // 
            textBoxTunnelCryptographyKeySize.Location = new Point(186, 18);
            textBoxTunnelCryptographyKeySize.Name = "textBoxTunnelCryptographyKeySize";
            textBoxTunnelCryptographyKeySize.Size = new Size(100, 23);
            textBoxTunnelCryptographyKeySize.TabIndex = 0;
            // 
            // textBoxStaleEndpointExpirationMs
            // 
            textBoxStaleEndpointExpirationMs.Location = new Point(188, 49);
            textBoxStaleEndpointExpirationMs.Name = "textBoxStaleEndpointExpirationMs";
            textBoxStaleEndpointExpirationMs.Size = new Size(100, 23);
            textBoxStaleEndpointExpirationMs.TabIndex = 1;
            // 
            // labelInitialReceiveBufferSize
            // 
            labelInitialReceiveBufferSize.AutoSize = true;
            labelInitialReceiveBufferSize.Location = new Point(26, 26);
            labelInitialReceiveBufferSize.Name = "labelInitialReceiveBufferSize";
            labelInitialReceiveBufferSize.Size = new Size(132, 15);
            labelInitialReceiveBufferSize.TabIndex = 22;
            labelInitialReceiveBufferSize.Text = "Initial buffer size (bytes)";
            // 
            // labelMaxReceiveBufferSize
            // 
            labelMaxReceiveBufferSize.AutoSize = true;
            labelMaxReceiveBufferSize.Location = new Point(32, 55);
            labelMaxReceiveBufferSize.Name = "labelMaxReceiveBufferSize";
            labelMaxReceiveBufferSize.Size = new Size(129, 15);
            labelMaxReceiveBufferSize.TabIndex = 23;
            labelMaxReceiveBufferSize.Text = "Max. buffer size (bytes)";
            // 
            // labelReceiveBufferGrowthRate
            // 
            labelReceiveBufferGrowthRate.AutoSize = true;
            labelReceiveBufferGrowthRate.Location = new Point(34, 84);
            labelReceiveBufferGrowthRate.Name = "labelReceiveBufferGrowthRate";
            labelReceiveBufferGrowthRate.Size = new Size(124, 15);
            labelReceiveBufferGrowthRate.TabIndex = 24;
            labelReceiveBufferGrowthRate.Text = "Buffer growth rate (%)";
            // 
            // textBoxInitialReceiveBufferSize
            // 
            textBoxInitialReceiveBufferSize.Location = new Point(164, 23);
            textBoxInitialReceiveBufferSize.Name = "textBoxInitialReceiveBufferSize";
            textBoxInitialReceiveBufferSize.Size = new Size(100, 23);
            textBoxInitialReceiveBufferSize.TabIndex = 3;
            // 
            // textBoxMaxReceiveBufferSize
            // 
            textBoxMaxReceiveBufferSize.Location = new Point(164, 52);
            textBoxMaxReceiveBufferSize.Name = "textBoxMaxReceiveBufferSize";
            textBoxMaxReceiveBufferSize.Size = new Size(100, 23);
            textBoxMaxReceiveBufferSize.TabIndex = 4;
            // 
            // textBoxReceiveBufferGrowthRate
            // 
            textBoxReceiveBufferGrowthRate.Location = new Point(164, 81);
            textBoxReceiveBufferGrowthRate.Name = "textBoxReceiveBufferGrowthRate";
            textBoxReceiveBufferGrowthRate.Size = new Size(100, 23);
            textBoxReceiveBufferGrowthRate.TabIndex = 5;
            // 
            // tabControlBody
            // 
            tabControlBody.Controls.Add(tabPageManagement);
            tabControlBody.Controls.Add(tabPageTunnels);
            tabControlBody.Controls.Add(tabPageEndpoints);
            tabControlBody.Location = new Point(12, 12);
            tabControlBody.Name = "tabControlBody";
            tabControlBody.SelectedIndex = 0;
            tabControlBody.Size = new Size(500, 410);
            tabControlBody.TabIndex = 28;
            // 
            // tabPageManagement
            // 
            tabPageManagement.Controls.Add(labelManagementPort);
            tabPageManagement.Controls.Add(textBoxManagementPort);
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
            tabPageTunnels.Controls.Add(textBoxPingCadence);
            tabPageTunnels.Controls.Add(labelPingCadence);
            tabPageTunnels.Controls.Add(groupBoxBuffering);
            tabPageTunnels.Controls.Add(textBoxTunnelCryptographyKeySize);
            tabPageTunnels.Controls.Add(textBoxMessageQueryTimeoutMs);
            tabPageTunnels.Controls.Add(labelMessageQueryTimeoutMs);
            tabPageTunnels.Controls.Add(labelTunnelCryptographyKeySize);
            tabPageTunnels.Location = new Point(4, 24);
            tabPageTunnels.Name = "tabPageTunnels";
            tabPageTunnels.Padding = new Padding(3);
            tabPageTunnels.Size = new Size(492, 382);
            tabPageTunnels.TabIndex = 1;
            tabPageTunnels.Text = "Tunnels";
            tabPageTunnels.UseVisualStyleBackColor = true;
            // 
            // textBoxPingCadence
            // 
            textBoxPingCadence.Location = new Point(186, 76);
            textBoxPingCadence.Name = "textBoxPingCadence";
            textBoxPingCadence.Size = new Size(100, 23);
            textBoxPingCadence.TabIndex = 2;
            // 
            // labelPingCadence
            // 
            labelPingCadence.AutoSize = true;
            labelPingCadence.Location = new Point(72, 79);
            labelPingCadence.Name = "labelPingCadence";
            labelPingCadence.Size = new Size(108, 15);
            labelPingCadence.TabIndex = 30;
            labelPingCadence.Text = "Ping cadence  (ms)";
            // 
            // groupBoxBuffering
            // 
            groupBoxBuffering.Controls.Add(labelInitialReceiveBufferSize);
            groupBoxBuffering.Controls.Add(labelMaxReceiveBufferSize);
            groupBoxBuffering.Controls.Add(textBoxReceiveBufferGrowthRate);
            groupBoxBuffering.Controls.Add(labelReceiveBufferGrowthRate);
            groupBoxBuffering.Controls.Add(textBoxInitialReceiveBufferSize);
            groupBoxBuffering.Controls.Add(textBoxMaxReceiveBufferSize);
            groupBoxBuffering.Location = new Point(22, 118);
            groupBoxBuffering.Name = "groupBoxBuffering";
            groupBoxBuffering.Size = new Size(280, 125);
            groupBoxBuffering.TabIndex = 28;
            groupBoxBuffering.TabStop = false;
            groupBoxBuffering.Text = "Buffering";
            // 
            // tabPageEndpoints
            // 
            tabPageEndpoints.Controls.Add(labelEndpointHeartbeatDelayMs);
            tabPageEndpoints.Controls.Add(labelStaleEndpointExpirationMs);
            tabPageEndpoints.Controls.Add(textBoxStaleEndpointExpirationMs);
            tabPageEndpoints.Controls.Add(textBoxEndpointHeartbeatDelayMs);
            tabPageEndpoints.Location = new Point(4, 24);
            tabPageEndpoints.Name = "tabPageEndpoints";
            tabPageEndpoints.Size = new Size(492, 382);
            tabPageEndpoints.TabIndex = 2;
            tabPageEndpoints.Text = "Endpoints";
            tabPageEndpoints.UseVisualStyleBackColor = true;
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
            groupBoxBuffering.ResumeLayout(false);
            groupBoxBuffering.PerformLayout();
            tabPageEndpoints.ResumeLayout(false);
            tabPageEndpoints.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button buttonCancel;
        private Button buttonSave;
        private Label labelManagementPort;
        private Label labelMessageQueryTimeoutMs;
        private Label labelEndpointHeartbeatDelayMs;
        private Label labelTunnelCryptographyKeySize;
        private Label labelStaleEndpointExpirationMs;
        private TextBox textBoxManagementPort;
        private TextBox textBoxMessageQueryTimeoutMs;
        private TextBox textBoxEndpointHeartbeatDelayMs;
        private TextBox textBoxTunnelCryptographyKeySize;
        private TextBox textBoxStaleEndpointExpirationMs;
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
        private GroupBox groupBoxBuffering;
        private TextBox textBoxPingCadence;
        private Label labelPingCadence;
    }
}