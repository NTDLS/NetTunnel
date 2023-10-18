namespace NetTunnel.UI
{
    partial class FormAddEndpoint
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddEndpoint));
            labelRemoteAddress = new Label();
            labelRemoteUsername = new Label();
            labelRemotePassword = new Label();
            textBoxRemoteAddress = new TextBox();
            textBoxRemoteUsername = new TextBox();
            textBoxRemotePassword = new TextBox();
            textBox1 = new TextBox();
            labelRemotePort = new Label();
            buttonAdd = new Button();
            buttonCancel = new Button();
            textBoxName = new TextBox();
            labelName = new Label();
            SuspendLayout();
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(17, 82);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(128, 15);
            labelRemoteAddress.TabIndex = 0;
            labelRemoteAddress.Text = "Remote address / host:";
            // 
            // labelRemoteUsername
            // 
            labelRemoteUsername.AutoSize = true;
            labelRemoteUsername.Location = new Point(17, 145);
            labelRemoteUsername.Name = "labelRemoteUsername";
            labelRemoteUsername.Size = new Size(106, 15);
            labelRemoteUsername.TabIndex = 1;
            labelRemoteUsername.Text = "Remote username:";
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(16, 198);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(101, 15);
            labelRemotePassword.TabIndex = 2;
            labelRemotePassword.Text = "Remote password";
            // 
            // textBoxRemoteAddress
            // 
            textBoxRemoteAddress.Location = new Point(17, 100);
            textBoxRemoteAddress.Name = "textBoxRemoteAddress";
            textBoxRemoteAddress.Size = new Size(156, 23);
            textBoxRemoteAddress.TabIndex = 1;
            // 
            // textBoxRemoteUsername
            // 
            textBoxRemoteUsername.Location = new Point(17, 163);
            textBoxRemoteUsername.Name = "textBoxRemoteUsername";
            textBoxRemoteUsername.Size = new Size(239, 23);
            textBoxRemoteUsername.TabIndex = 3;
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(17, 216);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.Size = new Size(239, 23);
            textBoxRemotePassword.TabIndex = 4;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(179, 100);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(77, 23);
            textBox1.TabIndex = 2;
            // 
            // labelRemotePort
            // 
            labelRemotePort.AutoSize = true;
            labelRemotePort.Location = new Point(179, 82);
            labelRemotePort.Name = "labelRemotePort";
            labelRemotePort.Size = new Size(29, 15);
            labelRemotePort.TabIndex = 4;
            labelRemotePort.Text = "Port";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(155, 263);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 5;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(236, 263);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 6;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(17, 32);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(294, 23);
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
            // FormAddEndpoint
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(328, 298);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAdd);
            Controls.Add(labelRemotePort);
            Controls.Add(textBox1);
            Controls.Add(textBoxRemotePassword);
            Controls.Add(textBoxRemoteUsername);
            Controls.Add(textBoxRemoteAddress);
            Controls.Add(labelRemotePassword);
            Controls.Add(labelRemoteUsername);
            Controls.Add(labelRemoteAddress);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAddEndpoint";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Add Endpoint";
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
        private TextBox textBox1;
        private Label labelRemotePort;
        private Button buttonAdd;
        private Button buttonCancel;
        private TextBox textBoxName;
        private Label labelName;
    }
}