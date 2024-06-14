namespace NetTunnel.UI.Forms
{
    partial class FormConnectTunnel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConnectTunnel));
            labelRemoteAddress = new Label();
            labelRemoteUsername = new Label();
            labelRemotePassword = new Label();
            textBoxRemoteAddress = new TextBox();
            textBoxRemoteUsername = new TextBox();
            textBoxRemotePassword = new TextBox();
            textBoxManagementPort = new TextBox();
            labelPort = new Label();
            buttonCancel = new Button();
            textBoxName = new TextBox();
            labelName = new Label();
            buttonConnect = new Button();
            groupBoxRemoteService = new GroupBox();
            groupBoxRemoteService.SuspendLayout();
            SuspendLayout();
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(19, 36);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(86, 15);
            labelRemoteAddress.TabIndex = 0;
            labelRemoteAddress.Text = "Address / host:";
            // 
            // labelRemoteUsername
            // 
            labelRemoteUsername.AutoSize = true;
            labelRemoteUsername.Location = new Point(20, 90);
            labelRemoteUsername.Name = "labelRemoteUsername";
            labelRemoteUsername.Size = new Size(63, 15);
            labelRemoteUsername.TabIndex = 1;
            labelRemoteUsername.Text = "Username:";
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(19, 139);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(57, 15);
            labelRemotePassword.TabIndex = 2;
            labelRemotePassword.Text = "Password";
            // 
            // textBoxRemoteAddress
            // 
            textBoxRemoteAddress.Location = new Point(20, 54);
            textBoxRemoteAddress.Name = "textBoxRemoteAddress";
            textBoxRemoteAddress.Size = new Size(156, 23);
            textBoxRemoteAddress.TabIndex = 2;
            // 
            // textBoxRemoteUsername
            // 
            textBoxRemoteUsername.Location = new Point(20, 108);
            textBoxRemoteUsername.Name = "textBoxRemoteUsername";
            textBoxRemoteUsername.Size = new Size(239, 23);
            textBoxRemoteUsername.TabIndex = 4;
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(20, 157);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.PasswordChar = '*';
            textBoxRemotePassword.Size = new Size(239, 23);
            textBoxRemotePassword.TabIndex = 5;
            // 
            // textBoxManagementPort
            // 
            textBoxManagementPort.Location = new Point(182, 54);
            textBoxManagementPort.Name = "textBoxManagementPort";
            textBoxManagementPort.Size = new Size(77, 23);
            textBoxManagementPort.TabIndex = 3;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(182, 36);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(29, 15);
            labelPort.TabIndex = 4;
            labelPort.Text = "Port";
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(141, 272);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(67, 23);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(12, 27);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(277, 23);
            textBoxName.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 9);
            labelName.Name = "labelName";
            labelName.Size = new Size(115, 15);
            labelName.TabIndex = 6;
            labelName.Text = "Name or description";
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(214, 272);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(75, 23);
            buttonConnect.TabIndex = 30;
            buttonConnect.Text = "Connect";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;
            // 
            // groupBoxRemoteService
            // 
            groupBoxRemoteService.Controls.Add(labelRemoteAddress);
            groupBoxRemoteService.Controls.Add(textBoxRemoteUsername);
            groupBoxRemoteService.Controls.Add(textBoxRemotePassword);
            groupBoxRemoteService.Controls.Add(textBoxManagementPort);
            groupBoxRemoteService.Controls.Add(labelRemoteUsername);
            groupBoxRemoteService.Controls.Add(textBoxRemoteAddress);
            groupBoxRemoteService.Controls.Add(labelPort);
            groupBoxRemoteService.Controls.Add(labelRemotePassword);
            groupBoxRemoteService.Location = new Point(12, 58);
            groupBoxRemoteService.Name = "groupBoxRemoteService";
            groupBoxRemoteService.Size = new Size(277, 208);
            groupBoxRemoteService.TabIndex = 31;
            groupBoxRemoteService.TabStop = false;
            groupBoxRemoteService.Text = "Remote Service";
            // 
            // FormConnectTunnel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(306, 311);
            Controls.Add(groupBoxRemoteService);
            Controls.Add(labelName);
            Controls.Add(textBoxName);
            Controls.Add(buttonConnect);
            Controls.Add(buttonCancel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormConnectTunnel";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Connect Tunnel";
            Load += FormConnectTunnel_Load;
            groupBoxRemoteService.ResumeLayout(false);
            groupBoxRemoteService.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelRemoteAddress;
        private Label labelRemoteUsername;
        private Label labelRemotePassword;
        private TextBox textBoxRemoteAddress;
        private TextBox textBoxRemoteUsername;
        private TextBox textBoxRemotePassword;
        private TextBox textBoxManagementPort;
        private Label labelPort;
        private Button buttonCancel;
        private TextBox textBoxName;
        private Label labelName;
        private Button buttonConnect;
        private GroupBox groupBoxRemoteService;
    }
}