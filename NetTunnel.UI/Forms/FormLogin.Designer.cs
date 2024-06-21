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
            labelPort = new Label();
            textBoxPort = new TextBox();
            textBoxAddress = new TextBox();
            labelAddress = new Label();
            buttonAbout = new Button();
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
            textBoxUsername.Size = new Size(275, 23);
            textBoxUsername.TabIndex = 6;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(90, 141);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new Size(275, 23);
            textBoxPassword.TabIndex = 0;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(209, 189);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonLogin
            // 
            buttonLogin.Location = new Point(290, 189);
            buttonLogin.Name = "buttonLogin";
            buttonLogin.Size = new Size(75, 23);
            buttonLogin.TabIndex = 1;
            buttonLogin.Text = "Login";
            buttonLogin.UseVisualStyleBackColor = true;
            buttonLogin.Click += buttonLogin_Click;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(294, 16);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(29, 15);
            labelPort.TabIndex = 8;
            labelPort.Text = "Port";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(294, 34);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(71, 23);
            textBoxPort.TabIndex = 4;
            // 
            // textBoxAddress
            // 
            textBoxAddress.Location = new Point(90, 34);
            textBoxAddress.Name = "textBoxAddress";
            textBoxAddress.Size = new Size(198, 23);
            textBoxAddress.TabIndex = 3;
            // 
            // labelAddress
            // 
            labelAddress.AutoSize = true;
            labelAddress.Location = new Point(90, 16);
            labelAddress.Name = "labelAddress";
            labelAddress.Size = new Size(86, 15);
            labelAddress.TabIndex = 5;
            labelAddress.Text = "Address / host:";
            // 
            // buttonAbout
            // 
            buttonAbout.Location = new Point(19, 189);
            buttonAbout.Name = "buttonAbout";
            buttonAbout.Size = new Size(75, 23);
            buttonAbout.TabIndex = 10;
            buttonAbout.Text = "About";
            buttonAbout.UseVisualStyleBackColor = true;
            buttonAbout.Click += buttonAbout_Click;
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 227);
            Controls.Add(buttonAbout);
            Controls.Add(labelPort);
            Controls.Add(textBoxPort);
            Controls.Add(textBoxAddress);
            Controls.Add(labelAddress);
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
        private Label labelPort;
        private TextBox textBoxPort;
        private TextBox textBoxAddress;
        private Label labelAddress;
        private Button buttonAbout;
    }
}