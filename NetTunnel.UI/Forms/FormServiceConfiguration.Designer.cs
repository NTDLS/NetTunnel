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
            labelMaxFrameSize = new Label();
            labelFrameQueryTimeoutMs = new Label();
            labelHeartbeatDelayMs = new Label();
            labelTunnelEncryptionKeySize = new Label();
            labelMaxStaleConnectionAgeMs = new Label();
            textBoxManagementPort = new TextBox();
            textBoxManagementPortRSASize = new TextBox();
            textBoxEndpointBufferSize = new TextBox();
            textBoxMaxFrameSize = new TextBox();
            textBoxFrameQueryTimeoutMs = new TextBox();
            textBoxHeartbeatDelayMs = new TextBox();
            textBoxTunnelEncryptionKeySize = new TextBox();
            textBoxMaxStaleConnectionAgeMs = new TextBox();
            checkBoxDebugLogging = new CheckBox();
            checkBox1 = new CheckBox();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(440, 401);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(357, 401);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 1;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            // 
            // labelManagementPort
            // 
            labelManagementPort.AutoSize = true;
            labelManagementPort.Location = new Point(85, 59);
            labelManagementPort.Name = "labelManagementPort";
            labelManagementPort.Size = new Size(103, 15);
            labelManagementPort.TabIndex = 3;
            labelManagementPort.Text = "Management Port";
            // 
            // labelManagementPortRSASize
            // 
            labelManagementPortRSASize.AutoSize = true;
            labelManagementPortRSASize.Location = new Point(36, 86);
            labelManagementPortRSASize.Name = "labelManagementPortRSASize";
            labelManagementPortRSASize.Size = new Size(152, 15);
            labelManagementPortRSASize.TabIndex = 4;
            labelManagementPortRSASize.Text = "Management-Port RSA Size";
            // 
            // labelEndpointBufferSize
            // 
            labelEndpointBufferSize.AutoSize = true;
            labelEndpointBufferSize.Location = new Point(36, 135);
            labelEndpointBufferSize.Name = "labelEndpointBufferSize";
            labelEndpointBufferSize.Size = new Size(113, 15);
            labelEndpointBufferSize.TabIndex = 5;
            labelEndpointBufferSize.Text = "Endpoint Buffer Size";
            // 
            // labelMaxFrameSize
            // 
            labelMaxFrameSize.AutoSize = true;
            labelMaxFrameSize.Location = new Point(55, 164);
            labelMaxFrameSize.Name = "labelMaxFrameSize";
            labelMaxFrameSize.Size = new Size(92, 15);
            labelMaxFrameSize.TabIndex = 6;
            labelMaxFrameSize.Text = "Max. Frame Size";
            // 
            // labelFrameQueryTimeoutMs
            // 
            labelFrameQueryTimeoutMs.AutoSize = true;
            labelFrameQueryTimeoutMs.Location = new Point(52, 257);
            labelFrameQueryTimeoutMs.Name = "labelFrameQueryTimeoutMs";
            labelFrameQueryTimeoutMs.Size = new Size(151, 15);
            labelFrameQueryTimeoutMs.TabIndex = 8;
            labelFrameQueryTimeoutMs.Text = "Frame-Query Timeout (ms)";
            // 
            // labelHeartbeatDelayMs
            // 
            labelHeartbeatDelayMs.AutoSize = true;
            labelHeartbeatDelayMs.Location = new Point(52, 300);
            labelHeartbeatDelayMs.Name = "labelHeartbeatDelayMs";
            labelHeartbeatDelayMs.Size = new Size(86, 15);
            labelHeartbeatDelayMs.TabIndex = 9;
            labelHeartbeatDelayMs.Text = "Heartbeat (ms)";
            // 
            // labelTunnelEncryptionKeySize
            // 
            labelTunnelEncryptionKeySize.AutoSize = true;
            labelTunnelEncryptionKeySize.Location = new Point(52, 330);
            labelTunnelEncryptionKeySize.Name = "labelTunnelEncryptionKeySize";
            labelTunnelEncryptionKeySize.Size = new Size(149, 15);
            labelTunnelEncryptionKeySize.TabIndex = 10;
            labelTunnelEncryptionKeySize.Text = "Tunnel Encryption Key-size";
            // 
            // labelMaxStaleConnectionAgeMs
            // 
            labelMaxStaleConnectionAgeMs.AutoSize = true;
            labelMaxStaleConnectionAgeMs.Location = new Point(52, 367);
            labelMaxStaleConnectionAgeMs.Name = "labelMaxStaleConnectionAgeMs";
            labelMaxStaleConnectionAgeMs.Size = new Size(177, 15);
            labelMaxStaleConnectionAgeMs.TabIndex = 11;
            labelMaxStaleConnectionAgeMs.Text = "Max. Stale Connection Age (ms)";
            // 
            // textBoxManagementPort
            // 
            textBoxManagementPort.Location = new Point(194, 54);
            textBoxManagementPort.Name = "textBoxManagementPort";
            textBoxManagementPort.Size = new Size(100, 23);
            textBoxManagementPort.TabIndex = 12;
            // 
            // textBoxManagementPortRSASize
            // 
            textBoxManagementPortRSASize.Location = new Point(194, 83);
            textBoxManagementPortRSASize.Name = "textBoxManagementPortRSASize";
            textBoxManagementPortRSASize.Size = new Size(100, 23);
            textBoxManagementPortRSASize.TabIndex = 13;
            // 
            // textBoxEndpointBufferSize
            // 
            textBoxEndpointBufferSize.Location = new Point(153, 132);
            textBoxEndpointBufferSize.Name = "textBoxEndpointBufferSize";
            textBoxEndpointBufferSize.Size = new Size(100, 23);
            textBoxEndpointBufferSize.TabIndex = 14;
            // 
            // textBoxMaxFrameSize
            // 
            textBoxMaxFrameSize.Location = new Point(153, 161);
            textBoxMaxFrameSize.Name = "textBoxMaxFrameSize";
            textBoxMaxFrameSize.Size = new Size(100, 23);
            textBoxMaxFrameSize.TabIndex = 15;
            // 
            // textBoxFrameQueryTimeoutMs
            // 
            textBoxFrameQueryTimeoutMs.Location = new Point(225, 252);
            textBoxFrameQueryTimeoutMs.Name = "textBoxFrameQueryTimeoutMs";
            textBoxFrameQueryTimeoutMs.Size = new Size(100, 23);
            textBoxFrameQueryTimeoutMs.TabIndex = 16;
            // 
            // textBoxHeartbeatDelayMs
            // 
            textBoxHeartbeatDelayMs.Location = new Point(219, 296);
            textBoxHeartbeatDelayMs.Name = "textBoxHeartbeatDelayMs";
            textBoxHeartbeatDelayMs.Size = new Size(100, 23);
            textBoxHeartbeatDelayMs.TabIndex = 17;
            // 
            // textBoxTunnelEncryptionKeySize
            // 
            textBoxTunnelEncryptionKeySize.Location = new Point(224, 328);
            textBoxTunnelEncryptionKeySize.Name = "textBoxTunnelEncryptionKeySize";
            textBoxTunnelEncryptionKeySize.Size = new Size(100, 23);
            textBoxTunnelEncryptionKeySize.TabIndex = 18;
            // 
            // textBoxMaxStaleConnectionAgeMs
            // 
            textBoxMaxStaleConnectionAgeMs.Location = new Point(235, 363);
            textBoxMaxStaleConnectionAgeMs.Name = "textBoxMaxStaleConnectionAgeMs";
            textBoxMaxStaleConnectionAgeMs.Size = new Size(100, 23);
            textBoxMaxStaleConnectionAgeMs.TabIndex = 19;
            // 
            // checkBoxDebugLogging
            // 
            checkBoxDebugLogging.AutoSize = true;
            checkBoxDebugLogging.Location = new Point(211, 209);
            checkBoxDebugLogging.Name = "checkBoxDebugLogging";
            checkBoxDebugLogging.Size = new Size(113, 19);
            checkBoxDebugLogging.TabIndex = 20;
            checkBoxDebugLogging.Text = "Debug Logging?";
            checkBoxDebugLogging.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(300, 58);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(71, 19);
            checkBox1.TabIndex = 21;
            checkBox1.Text = "Use SSL?";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // FormServiceConfiguration
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(868, 504);
            Controls.Add(checkBox1);
            Controls.Add(checkBoxDebugLogging);
            Controls.Add(textBoxMaxStaleConnectionAgeMs);
            Controls.Add(textBoxTunnelEncryptionKeySize);
            Controls.Add(textBoxHeartbeatDelayMs);
            Controls.Add(textBoxFrameQueryTimeoutMs);
            Controls.Add(textBoxMaxFrameSize);
            Controls.Add(textBoxEndpointBufferSize);
            Controls.Add(textBoxManagementPortRSASize);
            Controls.Add(textBoxManagementPort);
            Controls.Add(labelMaxStaleConnectionAgeMs);
            Controls.Add(labelTunnelEncryptionKeySize);
            Controls.Add(labelHeartbeatDelayMs);
            Controls.Add(labelFrameQueryTimeoutMs);
            Controls.Add(labelMaxFrameSize);
            Controls.Add(labelEndpointBufferSize);
            Controls.Add(labelManagementPortRSASize);
            Controls.Add(labelManagementPort);
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonCancel;
        private Button buttonSave;
        private Label labelManagementPort;
        private Label labelManagementPortRSASize;
        private Label labelEndpointBufferSize;
        private Label labelMaxFrameSize;
        private Label labelFrameQueryTimeoutMs;
        private Label labelHeartbeatDelayMs;
        private Label labelTunnelEncryptionKeySize;
        private Label labelMaxStaleConnectionAgeMs;
        private TextBox textBoxManagementPort;
        private TextBox textBoxManagementPortRSASize;
        private TextBox textBoxEndpointBufferSize;
        private TextBox textBoxMaxFrameSize;
        private TextBox textBoxFrameQueryTimeoutMs;
        private TextBox textBoxHeartbeatDelayMs;
        private TextBox textBoxTunnelEncryptionKeySize;
        private TextBox textBoxMaxStaleConnectionAgeMs;
        private CheckBox checkBoxDebugLogging;
        private CheckBox checkBox1;
    }
}