namespace NetTunnel.UI.Forms
{
    partial class FormAddTunnel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddTunnel));
            labelRemoteAddress = new Label();
            labelRemoteUsername = new Label();
            labelRemotePassword = new Label();
            textBoxRemoteAddress = new TextBox();
            textBoxRemoteUsername = new TextBox();
            textBoxRemotePassword = new TextBox();
            textBoxRemotePort = new TextBox();
            labelRemotePort = new Label();
            buttonAdd = new Button();
            buttonCancel = new Button();
            textBoxName = new TextBox();
            labelName = new Label();
            textBoxTunnelDataPort = new TextBox();
            labelEndpointDataPort = new Label();
            groupBoxLoginInfo = new GroupBox();
            groupBoxLoginInfo.SuspendLayout();
            SuspendLayout();
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(17, 30);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(86, 15);
            labelRemoteAddress.TabIndex = 0;
            labelRemoteAddress.Text = "Address / host:";
            // 
            // labelRemoteUsername
            // 
            labelRemoteUsername.AutoSize = true;
            labelRemoteUsername.Location = new Point(18, 88);
            labelRemoteUsername.Name = "labelRemoteUsername";
            labelRemoteUsername.Size = new Size(63, 15);
            labelRemoteUsername.TabIndex = 1;
            labelRemoteUsername.Text = "Username:";
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(17, 141);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(57, 15);
            labelRemotePassword.TabIndex = 2;
            labelRemotePassword.Text = "Password";
            // 
            // textBoxRemoteAddress
            // 
            textBoxRemoteAddress.Location = new Point(18, 48);
            textBoxRemoteAddress.Name = "textBoxRemoteAddress";
            textBoxRemoteAddress.Size = new Size(156, 23);
            textBoxRemoteAddress.TabIndex = 2;
            // 
            // textBoxRemoteUsername
            // 
            textBoxRemoteUsername.Location = new Point(18, 106);
            textBoxRemoteUsername.Name = "textBoxRemoteUsername";
            textBoxRemoteUsername.Size = new Size(239, 23);
            textBoxRemoteUsername.TabIndex = 4;
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(18, 159);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.PasswordChar = '*';
            textBoxRemotePassword.Size = new Size(239, 23);
            textBoxRemotePassword.TabIndex = 5;
            // 
            // textBoxRemotePort
            // 
            textBoxRemotePort.Location = new Point(180, 48);
            textBoxRemotePort.Name = "textBoxRemotePort";
            textBoxRemotePort.Size = new Size(77, 23);
            textBoxRemotePort.TabIndex = 3;
            // 
            // labelRemotePort
            // 
            labelRemotePort.AutoSize = true;
            labelRemotePort.Location = new Point(180, 30);
            labelRemotePort.Name = "labelRemotePort";
            labelRemotePort.Size = new Size(68, 15);
            labelRemotePort.TabIndex = 4;
            labelRemotePort.Text = "Mgmt. Port";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(137, 330);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 6;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(220, 330);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(17, 32);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(278, 23);
            textBoxName.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(17, 14);
            labelName.Name = "labelName";
            labelName.Size = new Size(115, 15);
            labelName.TabIndex = 6;
            labelName.Text = "Name or description";
            // 
            // textBoxTunnelDataPort
            // 
            textBoxTunnelDataPort.Location = new Point(17, 79);
            textBoxTunnelDataPort.Name = "textBoxTunnelDataPort";
            textBoxTunnelDataPort.Size = new Size(77, 23);
            textBoxTunnelDataPort.TabIndex = 1;
            // 
            // labelEndpointDataPort
            // 
            labelEndpointDataPort.AutoSize = true;
            labelEndpointDataPort.Location = new Point(17, 61);
            labelEndpointDataPort.Name = "labelEndpointDataPort";
            labelEndpointDataPort.Size = new Size(106, 15);
            labelEndpointDataPort.TabIndex = 8;
            labelEndpointDataPort.Text = "Endpoint data port";
            // 
            // groupBoxLoginInfo
            // 
            groupBoxLoginInfo.Controls.Add(labelRemoteAddress);
            groupBoxLoginInfo.Controls.Add(labelRemoteUsername);
            groupBoxLoginInfo.Controls.Add(labelRemotePassword);
            groupBoxLoginInfo.Controls.Add(textBoxRemoteAddress);
            groupBoxLoginInfo.Controls.Add(textBoxRemoteUsername);
            groupBoxLoginInfo.Controls.Add(textBoxRemotePassword);
            groupBoxLoginInfo.Controls.Add(textBoxRemotePort);
            groupBoxLoginInfo.Controls.Add(labelRemotePort);
            groupBoxLoginInfo.Location = new Point(17, 117);
            groupBoxLoginInfo.Name = "groupBoxLoginInfo";
            groupBoxLoginInfo.Size = new Size(278, 196);
            groupBoxLoginInfo.TabIndex = 9;
            groupBoxLoginInfo.TabStop = false;
            groupBoxLoginInfo.Text = "Remote Service Login";
            // 
            // FormAddTunnel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(312, 365);
            Controls.Add(groupBoxLoginInfo);
            Controls.Add(labelEndpointDataPort);
            Controls.Add(textBoxTunnelDataPort);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAdd);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAddTunnel";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Tunnel";
            Load += FormAddTunnel_Load;
            groupBoxLoginInfo.ResumeLayout(false);
            groupBoxLoginInfo.PerformLayout();
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
        private TextBox textBoxRemotePort;
        private Label labelRemotePort;
        private Button buttonAdd;
        private Button buttonCancel;
        private TextBox textBoxName;
        private Label labelName;
        private TextBox textBoxTunnelDataPort;
        private Label labelEndpointDataPort;
        private GroupBox groupBoxLoginInfo;
    }
}