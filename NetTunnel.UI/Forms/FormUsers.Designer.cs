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
            columnHeaderRole = new ColumnHeader();
            columnHeaderEndpoints = new ColumnHeader();
            buttonCreateUser = new Button();
            SuspendLayout();
            // 
            // listViewUsers
            // 
            listViewUsers.Columns.AddRange(new ColumnHeader[] { columnHeaderUsername, columnHeaderRole, columnHeaderEndpoints });
            listViewUsers.FullRowSelect = true;
            listViewUsers.GridLines = true;
            listViewUsers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewUsers.Location = new Point(12, 12);
            listViewUsers.Name = "listViewUsers";
            listViewUsers.Size = new Size(615, 233);
            listViewUsers.TabIndex = 0;
            listViewUsers.UseCompatibleStateImageBehavior = false;
            listViewUsers.View = View.Details;
            // 
            // columnHeaderUsername
            // 
            columnHeaderUsername.Text = "Username";
            columnHeaderUsername.Width = 300;
            // 
            // columnHeaderRole
            // 
            columnHeaderRole.Text = "Role";
            columnHeaderRole.Width = 200;
            // 
            // columnHeaderEndpoints
            // 
            columnHeaderEndpoints.Text = "Endpoints";
            columnHeaderEndpoints.Width = 90;
            // 
            // buttonCreateUser
            // 
            buttonCreateUser.Location = new Point(539, 251);
            buttonCreateUser.Name = "buttonCreateUser";
            buttonCreateUser.Size = new Size(87, 23);
            buttonCreateUser.TabIndex = 2;
            buttonCreateUser.Text = "Create User";
            buttonCreateUser.UseVisualStyleBackColor = true;
            buttonCreateUser.Click += ButtonCreateUser_Click;
            // 
            // FormUsers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(638, 284);
            Controls.Add(buttonCreateUser);
            Controls.Add(listViewUsers);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormUsers";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Users";
            ResumeLayout(false);
        }

        #endregion

        private ListView listViewUsers;
        private ColumnHeader columnHeaderUsername;
        private Button buttonCreateUser;
        private ColumnHeader columnHeaderRole;
        private ColumnHeader columnHeaderEndpoints;
    }
}