namespace NetTunnel.UI.Forms
{
    partial class FormLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            pictureBoxLogin = new PictureBox();
            labelUsername = new Label();
            labelPassword = new Label();
            textBoxUsername = new TextBox();
            textBoxPassword = new TextBox();
            buttonCancel = new Button();
            buttonLogin = new Button();
            labelRemotePort = new Label();
            textBoxPort = new TextBox();
            textBoxAddress = new TextBox();
            labelRemoteAddress = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogin).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxLogin
            // 
            pictureBoxLogin.Image = Properties.Resources.Login;
            pictureBoxLogin.Location = new Point(19, 33);
            pictureBoxLogin.Name = "pictureBoxLogin";
            pictureBoxLogin.Size = new Size(52, 54);
            pictureBoxLogin.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBoxLogin.TabIndex = 0;
            pictureBoxLogin.TabStop = false;
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Location = new Point(90, 72);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(136, 15);
            labelUsername.TabIndex = 1;
            labelUsername.Text = "Username (default: root)";
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(90, 123);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(245, 15);
            labelPassword.TabIndex = 2;
            labelPassword.Text = "Password (default: lowercase machine name)";
            // 
            // textBoxUsername
            // 
            textBoxUsername.Location = new Point(90, 90);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(262, 23);
            textBoxUsername.TabIndex = 2;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(90, 141);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new Size(262, 23);
            textBoxPassword.TabIndex = 3;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(277, 189);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonLogin
            // 
            buttonLogin.Location = new Point(196, 189);
            buttonLogin.Name = "buttonLogin";
            buttonLogin.Size = new Size(75, 23);
            buttonLogin.TabIndex = 4;
            buttonLogin.Text = "Login";
            buttonLogin.UseVisualStyleBackColor = true;
            buttonLogin.Click += buttonLogin_Click;
            // 
            // labelRemotePort
            // 
            labelRemotePort.AutoSize = true;
            labelRemotePort.Location = new Point(294, 16);
            labelRemotePort.Name = "labelRemotePort";
            labelRemotePort.Size = new Size(29, 15);
            labelRemotePort.TabIndex = 8;
            labelRemotePort.Text = "Port";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(294, 34);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(58, 23);
            textBoxPort.TabIndex = 1;
            // 
            // textBoxAddress
            // 
            textBoxAddress.Location = new Point(90, 34);
            textBoxAddress.Name = "textBoxAddress";
            textBoxAddress.Size = new Size(198, 23);
            textBoxAddress.TabIndex = 0;
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(90, 16);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(86, 15);
            labelRemoteAddress.TabIndex = 5;
            labelRemoteAddress.Text = "Address / host:";
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(369, 227);
            Controls.Add(labelRemotePort);
            Controls.Add(textBoxPort);
            Controls.Add(textBoxAddress);
            Controls.Add(labelRemoteAddress);
            Controls.Add(buttonLogin);
            Controls.Add(buttonCancel);
            Controls.Add(textBoxPassword);
            Controls.Add(textBoxUsername);
            Controls.Add(labelPassword);
            Controls.Add(labelUsername);
            Controls.Add(pictureBoxLogin);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormLogin";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel";
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogin).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoxLogin;
        private Label labelUsername;
        private Label labelPassword;
        private TextBox textBoxUsername;
        private TextBox textBoxPassword;
        private Button buttonCancel;
        private Button buttonLogin;
        private Label labelRemotePort;
        private TextBox textBoxPort;
        private TextBox textBoxAddress;
        private Label labelRemoteAddress;
    }
}