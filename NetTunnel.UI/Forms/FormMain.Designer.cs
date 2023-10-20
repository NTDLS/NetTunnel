namespace NetTunnel.UI.Forms
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            listViewTunnels = new ListView();
            columnHeaderName = new ColumnHeader();
            columnEntrypoint = new ColumnHeader();
            columnHeaderPort = new ColumnHeader();
            columnHeaderTunnels = new ColumnHeader();
            menuStripBody = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            connectToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            securityToolStripMenuItem = new ToolStripMenuItem();
            usersToolStripMenuItem = new ToolStripMenuItem();
            statusStripBody = new StatusStrip();
            menuStripBody.SuspendLayout();
            SuspendLayout();
            // 
            // listViewEndpoints
            // 
            listViewTunnels.Columns.AddRange(new ColumnHeader[] { columnHeaderName, columnEntrypoint, columnHeaderPort, columnHeaderTunnels });
            listViewTunnels.Dock = DockStyle.Fill;
            listViewTunnels.FullRowSelect = true;
            listViewTunnels.GridLines = true;
            listViewTunnels.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewTunnels.Location = new Point(0, 24);
            listViewTunnels.Name = "listViewEndpoints";
            listViewTunnels.Size = new Size(751, 417);
            listViewTunnels.TabIndex = 0;
            listViewTunnels.UseCompatibleStateImageBehavior = false;
            listViewTunnels.View = View.Details;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 250;
            // 
            // columnEntrypoint
            // 
            columnEntrypoint.Text = "Entry Point";
            columnEntrypoint.Width = 115;
            // 
            // columnHeaderPort
            // 
            columnHeaderPort.Text = "Port";
            columnHeaderPort.Width = 100;
            // 
            // columnHeaderTunnels
            // 
            columnHeaderTunnels.Text = "Tunnels";
            columnHeaderTunnels.Width = 80;
            // 
            // menuStripBody
            // 
            menuStripBody.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, securityToolStripMenuItem });
            menuStripBody.Location = new Point(0, 0);
            menuStripBody.Name = "menuStripBody";
            menuStripBody.Size = new Size(751, 24);
            menuStripBody.TabIndex = 1;
            menuStripBody.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { connectToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // connectToolStripMenuItem
            // 
            connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            connectToolStripMenuItem.Size = new Size(180, 22);
            connectToolStripMenuItem.Text = "Connect";
            connectToolStripMenuItem.Click += connectToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(180, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // securityToolStripMenuItem
            // 
            securityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { usersToolStripMenuItem });
            securityToolStripMenuItem.Name = "securityToolStripMenuItem";
            securityToolStripMenuItem.Size = new Size(61, 20);
            securityToolStripMenuItem.Text = "Security";
            // 
            // usersToolStripMenuItem
            // 
            usersToolStripMenuItem.Name = "usersToolStripMenuItem";
            usersToolStripMenuItem.Size = new Size(180, 22);
            usersToolStripMenuItem.Text = "Users";
            usersToolStripMenuItem.Click += usersToolStripMenuItem_Click;
            // 
            // statusStripBody
            // 
            statusStripBody.Location = new Point(0, 441);
            statusStripBody.Name = "statusStripBody";
            statusStripBody.Size = new Size(751, 22);
            statusStripBody.TabIndex = 2;
            statusStripBody.Text = "statusStrip1";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(751, 463);
            Controls.Add(listViewTunnels);
            Controls.Add(menuStripBody);
            Controls.Add(statusStripBody);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripBody;
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NetTunnel";
            Load += FormMain_Load;
            menuStripBody.ResumeLayout(false);
            menuStripBody.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listViewTunnels;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnEntrypoint;
        private ColumnHeader columnHeaderPort;
        private ColumnHeader columnHeaderTunnels;
        private MenuStrip menuStripBody;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem securityToolStripMenuItem;
        private ToolStripMenuItem usersToolStripMenuItem;
        private StatusStrip statusStripBody;
    }
}