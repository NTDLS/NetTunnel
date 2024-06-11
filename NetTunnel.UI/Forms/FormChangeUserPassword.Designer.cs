namespace NetTunnel.UI.Forms
{
    partial class FormChangeUserPassword
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormChangeUserPassword));
            buttonSave = new Button();
            buttonCancel = new Button();
            labelUsername = new Label();
            labelPassword = new Label();
            labelConfirmPassword = new Label();
            textBoxUsername = new TextBox();
            textBoxPassword = new TextBox();
            textBoxConfirmPassword = new TextBox();
            SuspendLayout();
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(173, 191);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 2;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(92, 191);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Location = new Point(16, 13);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(60, 15);
            labelUsername.TabIndex = 2;
            labelUsername.Text = "Username";
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(16, 80);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(57, 15);
            labelPassword.TabIndex = 3;
            labelPassword.Text = "Password";
            // 
            // labelConfirmPassword
            // 
            labelConfirmPassword.AutoSize = true;
            labelConfirmPassword.Location = new Point(16, 128);
            labelConfirmPassword.Name = "labelConfirmPassword";
            labelConfirmPassword.Size = new Size(104, 15);
            labelConfirmPassword.TabIndex = 4;
            labelConfirmPassword.Text = "Confirm password";
            // 
            // textBoxUsername
            // 
            textBoxUsername.Enabled = false;
            textBoxUsername.Location = new Point(16, 31);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.ReadOnly = true;
            textBoxUsername.Size = new Size(232, 23);
            textBoxUsername.TabIndex = 10;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(16, 98);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new Size(232, 23);
            textBoxPassword.TabIndex = 0;
            // 
            // textBoxConfirmPassword
            // 
            textBoxConfirmPassword.Location = new Point(16, 146);
            textBoxConfirmPassword.Name = "textBoxConfirmPassword";
            textBoxConfirmPassword.PasswordChar = '*';
            textBoxConfirmPassword.Size = new Size(232, 23);
            textBoxConfirmPassword.TabIndex = 1;
            // 
            // FormChangeUserPassword
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(268, 231);
            Controls.Add(textBoxConfirmPassword);
            Controls.Add(textBoxPassword);
            Controls.Add(textBoxUsername);
            Controls.Add(labelConfirmPassword);
            Controls.Add(labelPassword);
            Controls.Add(labelUsername);
            Controls.Add(buttonCancel);
            Controls.Add(buttonSave);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormChangeUserPassword";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Change Password";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonSave;
        private Button buttonCancel;
        private Label labelUsername;
        private Label labelPassword;
        private Label labelConfirmPassword;
        private TextBox textBoxUsername;
        private TextBox textBoxPassword;
        private TextBox textBoxConfirmPassword;
    }
}