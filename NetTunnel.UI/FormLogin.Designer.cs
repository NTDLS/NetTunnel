namespace NetTunnel.UI
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
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogin).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxLogin
            // 
            pictureBoxLogin.Image = Properties.Resources.Login;
            pictureBoxLogin.Location = new Point(24, 43);
            pictureBoxLogin.Name = "pictureBoxLogin";
            pictureBoxLogin.Size = new Size(52, 54);
            pictureBoxLogin.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBoxLogin.TabIndex = 0;
            pictureBoxLogin.TabStop = false;
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Location = new Point(112, 25);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(60, 15);
            labelUsername.TabIndex = 1;
            labelUsername.Text = "Username";
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(112, 74);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(57, 15);
            labelPassword.TabIndex = 2;
            labelPassword.Text = "Password";
            // 
            // textBoxUsername
            // 
            textBoxUsername.Location = new Point(112, 43);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(167, 23);
            textBoxUsername.TabIndex = 0;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(112, 92);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(167, 23);
            textBoxPassword.TabIndex = 1;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(204, 140);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonLogin
            // 
            buttonLogin.Location = new Point(123, 140);
            buttonLogin.Name = "buttonLogin";
            buttonLogin.Size = new Size(75, 23);
            buttonLogin.TabIndex = 2;
            buttonLogin.Text = "Login";
            buttonLogin.UseVisualStyleBackColor = true;
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 185);
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
    }
}