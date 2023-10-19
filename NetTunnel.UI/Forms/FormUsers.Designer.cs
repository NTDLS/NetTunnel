namespace NetTunnel.UI.Forms
{
    partial class FormUsers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUsers));
            listViewUsers = new ListView();
            columnHeaderUsername = new ColumnHeader();
            buttonAddUser = new Button();
            SuspendLayout();
            // 
            // listViewUsers
            // 
            listViewUsers.Columns.AddRange(new ColumnHeader[] { columnHeaderUsername });
            listViewUsers.FullRowSelect = true;
            listViewUsers.GridLines = true;
            listViewUsers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewUsers.Location = new Point(12, 12);
            listViewUsers.Name = "listViewUsers";
            listViewUsers.Size = new Size(317, 312);
            listViewUsers.TabIndex = 0;
            listViewUsers.UseCompatibleStateImageBehavior = false;
            listViewUsers.View = View.Details;
            // 
            // columnHeaderUsername
            // 
            columnHeaderUsername.Text = "Username";
            columnHeaderUsername.Width = 260;
            // 
            // buttonAddUser
            // 
            buttonAddUser.Location = new Point(12, 332);
            buttonAddUser.Name = "buttonAddUser";
            buttonAddUser.Size = new Size(75, 23);
            buttonAddUser.TabIndex = 2;
            buttonAddUser.Text = "Add User";
            buttonAddUser.UseVisualStyleBackColor = true;
            buttonAddUser.Click += buttonAddUser_Click;
            // 
            // FormUsers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(341, 367);
            Controls.Add(buttonAddUser);
            Controls.Add(listViewUsers);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormUsers";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Users";
            Load += FormUsers_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListView listViewUsers;
        private ColumnHeader columnHeaderUsername;
        private Button buttonAddUser;
    }
}