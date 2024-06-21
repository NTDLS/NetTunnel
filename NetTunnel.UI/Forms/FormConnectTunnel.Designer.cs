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
            labelAddress = new Label();
            labelUsername = new Label();
            labelPassword = new Label();
            textBoxAddress = new TextBox();
            textBoxUsername = new TextBox();
            textBoxPassword = new TextBox();
            textBoxPort = new TextBox();
            labelPort = new Label();
            buttonCancel = new Button();
            textBoxName = new TextBox();
            labelName = new Label();
            buttonConnect = new Button();
            groupBoxRemoteService = new GroupBox();
            groupBoxRemoteService.SuspendLayout();
            SuspendLayout();
            // 
            // labelAddress
            // 
            labelAddress.AutoSize = true;
            labelAddress.Location = new Point(19, 36);
            labelAddress.Name = "labelAddress";
            labelAddress.Size = new Size(86, 15);
            labelAddress.TabIndex = 0;
            labelAddress.Text = "Address / host:";
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Location = new Point(20, 90);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(63, 15);
            labelUsername.TabIndex = 1;
            labelUsername.Text = "Username:";
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(19, 139);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(57, 15);
            labelPassword.TabIndex = 2;
            labelPassword.Text = "Password";
            // 
            // textBoxAddress
            // 
            textBoxAddress.Location = new Point(20, 54);
            textBoxAddress.Name = "textBoxAddress";
            textBoxAddress.Size = new Size(156, 23);
            textBoxAddress.TabIndex = 1;
            // 
            // textBoxUsername
            // 
            textBoxUsername.Location = new Point(20, 108);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(239, 23);
            textBoxUsername.TabIndex = 3;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(20, 157);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new Size(239, 23);
            textBoxPassword.TabIndex = 4;
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(182, 54);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(77, 23);
            textBoxPort.TabIndex = 2;
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
            buttonCancel.TabIndex = 6;
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
            buttonConnect.TabIndex = 5;
            buttonConnect.Text = "Connect";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += ButtonConnect_Click;
            // 
            // groupBoxRemoteService
            // 
            groupBoxRemoteService.Controls.Add(labelAddress);
            groupBoxRemoteService.Controls.Add(textBoxUsername);
            groupBoxRemoteService.Controls.Add(textBoxPassword);
            groupBoxRemoteService.Controls.Add(textBoxPort);
            groupBoxRemoteService.Controls.Add(labelUsername);
            groupBoxRemoteService.Controls.Add(textBoxAddress);
            groupBoxRemoteService.Controls.Add(labelPort);
            groupBoxRemoteService.Controls.Add(labelPassword);
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
            groupBoxRemoteService.ResumeLayout(false);
            groupBoxRemoteService.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelAddress;
        private Label labelUsername;
        private Label labelPassword;
        private TextBox textBoxAddress;
        private TextBox textBoxUsername;
        private TextBox textBoxPassword;
        private TextBox textBoxPort;
        private Label labelPort;
        private Button buttonCancel;
        private TextBox textBoxName;
        private Label labelName;
        private Button buttonConnect;
        private GroupBox groupBoxRemoteService;
    }
}