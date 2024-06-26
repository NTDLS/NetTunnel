namespace NetTunnel.UI.Forms
{
    partial class FormAddEditUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddEditUser));
            buttonSave = new Button();
            buttonCancel = new Button();
            labelUsername = new Label();
            labelPassword = new Label();
            labelConfirmPassword = new Label();
            textBoxUsername = new TextBox();
            textBoxPassword = new TextBox();
            textBoxConfirmPassword = new TextBox();
            checkBoxAdministrator = new CheckBox();
            tabControl1 = new TabControl();
            tabPageAccount = new TabPage();
            tabPageEndpoints = new TabPage();
            buttonEdit = new Button();
            buttonAddOutbound = new Button();
            buttonDelete = new Button();
            listViewEndpoints = new ListView();
            columnHeaderName = new ColumnHeader();
            columnHeaderDirection = new ColumnHeader();
            columnHeaderPort = new ColumnHeader();
            buttonAddInbound = new Button();
            tabControl1.SuspendLayout();
            tabPageAccount.SuspendLayout();
            tabPageEndpoints.SuspendLayout();
            SuspendLayout();
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(483, 326);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 100;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += ButtonSave_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(402, 326);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 101;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Location = new Point(10, 7);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(60, 15);
            labelUsername.TabIndex = 2;
            labelUsername.Text = "Username";
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(10, 74);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(57, 15);
            labelPassword.TabIndex = 3;
            labelPassword.Text = "Password";
            // 
            // labelConfirmPassword
            // 
            labelConfirmPassword.AutoSize = true;
            labelConfirmPassword.Location = new Point(10, 122);
            labelConfirmPassword.Name = "labelConfirmPassword";
            labelConfirmPassword.Size = new Size(104, 15);
            labelConfirmPassword.TabIndex = 4;
            labelConfirmPassword.Text = "Confirm password";
            // 
            // textBoxUsername
            // 
            textBoxUsername.Enabled = false;
            textBoxUsername.Location = new Point(10, 25);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.ReadOnly = true;
            textBoxUsername.Size = new Size(232, 23);
            textBoxUsername.TabIndex = 10;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(10, 92);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new Size(232, 23);
            textBoxPassword.TabIndex = 0;
            // 
            // textBoxConfirmPassword
            // 
            textBoxConfirmPassword.Location = new Point(10, 140);
            textBoxConfirmPassword.Name = "textBoxConfirmPassword";
            textBoxConfirmPassword.PasswordChar = '*';
            textBoxConfirmPassword.Size = new Size(232, 23);
            textBoxConfirmPassword.TabIndex = 1;
            // 
            // checkBoxAdministrator
            // 
            checkBoxAdministrator.AutoSize = true;
            checkBoxAdministrator.Location = new Point(10, 179);
            checkBoxAdministrator.Name = "checkBoxAdministrator";
            checkBoxAdministrator.Size = new Size(113, 19);
            checkBoxAdministrator.TabIndex = 11;
            checkBoxAdministrator.Text = "Is administrator?";
            checkBoxAdministrator.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageAccount);
            tabControl1.Controls.Add(tabPageEndpoints);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(550, 308);
            tabControl1.TabIndex = 12;
            // 
            // tabPageAccount
            // 
            tabPageAccount.Controls.Add(labelUsername);
            tabPageAccount.Controls.Add(checkBoxAdministrator);
            tabPageAccount.Controls.Add(textBoxConfirmPassword);
            tabPageAccount.Controls.Add(textBoxPassword);
            tabPageAccount.Controls.Add(labelPassword);
            tabPageAccount.Controls.Add(textBoxUsername);
            tabPageAccount.Controls.Add(labelConfirmPassword);
            tabPageAccount.Location = new Point(4, 24);
            tabPageAccount.Name = "tabPageAccount";
            tabPageAccount.Padding = new Padding(3);
            tabPageAccount.Size = new Size(542, 280);
            tabPageAccount.TabIndex = 0;
            tabPageAccount.Text = "Account";
            tabPageAccount.UseVisualStyleBackColor = true;
            // 
            // tabPageEndpoints
            // 
            tabPageEndpoints.Controls.Add(buttonEdit);
            tabPageEndpoints.Controls.Add(buttonAddOutbound);
            tabPageEndpoints.Controls.Add(buttonDelete);
            tabPageEndpoints.Controls.Add(listViewEndpoints);
            tabPageEndpoints.Controls.Add(buttonAddInbound);
            tabPageEndpoints.Location = new Point(4, 24);
            tabPageEndpoints.Name = "tabPageEndpoints";
            tabPageEndpoints.Padding = new Padding(3);
            tabPageEndpoints.Size = new Size(542, 280);
            tabPageEndpoints.TabIndex = 1;
            tabPageEndpoints.Text = "Endpoints";
            tabPageEndpoints.UseVisualStyleBackColor = true;
            // 
            // buttonEdit
            // 
            buttonEdit.Location = new Point(418, 131);
            buttonEdit.Name = "buttonEdit";
            buttonEdit.Size = new Size(114, 23);
            buttonEdit.TabIndex = 3;
            buttonEdit.Text = "Edit";
            buttonEdit.UseVisualStyleBackColor = true;
            buttonEdit.Click += ButtonEdit_Click;
            // 
            // buttonAddOutbound
            // 
            buttonAddOutbound.Location = new Point(418, 35);
            buttonAddOutbound.Name = "buttonAddOutbound";
            buttonAddOutbound.Size = new Size(114, 23);
            buttonAddOutbound.TabIndex = 2;
            buttonAddOutbound.Text = "Add Outbound";
            buttonAddOutbound.UseVisualStyleBackColor = true;
            buttonAddOutbound.Click += ButtonAddOutbound_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(418, 251);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(114, 23);
            buttonDelete.TabIndex = 4;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // listViewEndpoints
            // 
            listViewEndpoints.Columns.AddRange(new ColumnHeader[] { columnHeaderName, columnHeaderDirection, columnHeaderPort });
            listViewEndpoints.FullRowSelect = true;
            listViewEndpoints.GridLines = true;
            listViewEndpoints.Location = new Point(6, 6);
            listViewEndpoints.Name = "listViewEndpoints";
            listViewEndpoints.Size = new Size(406, 268);
            listViewEndpoints.TabIndex = 0;
            listViewEndpoints.UseCompatibleStateImageBehavior = false;
            listViewEndpoints.View = View.Details;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 200;
            // 
            // columnHeaderDirection
            // 
            columnHeaderDirection.Text = "Direction";
            columnHeaderDirection.Width = 100;
            // 
            // columnHeaderPort
            // 
            columnHeaderPort.Text = "Port";
            // 
            // buttonAddInbound
            // 
            buttonAddInbound.Location = new Point(418, 6);
            buttonAddInbound.Name = "buttonAddInbound";
            buttonAddInbound.Size = new Size(114, 23);
            buttonAddInbound.TabIndex = 1;
            buttonAddInbound.Text = "Add Inbound";
            buttonAddInbound.UseVisualStyleBackColor = true;
            buttonAddInbound.Click += ButtonAddInbound_Click;
            // 
            // FormEditUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(570, 358);
            Controls.Add(tabControl1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonSave);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormEditUser";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Add/Edit User";
            tabControl1.ResumeLayout(false);
            tabPageAccount.ResumeLayout(false);
            tabPageAccount.PerformLayout();
            tabPageEndpoints.ResumeLayout(false);
            ResumeLayout(false);
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
        private CheckBox checkBoxAdministrator;
        private TabControl tabControl1;
        private TabPage tabPageAccount;
        private TabPage tabPageEndpoints;
        private Button buttonDelete;
        private ListView listViewEndpoints;
        private Button buttonAddInbound;
        private Button buttonAddOutbound;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnHeaderDirection;
        private ColumnHeader columnHeaderPort;
        private Button buttonEdit;
    }
}