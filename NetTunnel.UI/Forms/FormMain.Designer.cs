using NetTunnel.UI.Controls;

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
            menuStripBody = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            connectToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            securityToolStripMenuItem = new ToolStripMenuItem();
            usersToolStripMenuItem = new ToolStripMenuItem();
            configurationToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            statusStripBody = new StatusStrip();
            splitContainerTunnelsAndEndpoints = new SplitContainer();
            listViewTunnels = new DoubleBufferedListView();
            labelTunnels = new Label();
            listViewEndpoints = new DoubleBufferedListView();
            labelEndpoints = new Label();
            splitContainerBody = new SplitContainer();
            listViewLogs = new DoubleBufferedListView();
            columnHeaderDateTime = new ColumnHeader();
            columnHeaderSeverity = new ColumnHeader();
            columnHeaderText = new ColumnHeader();
            menuStripBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerTunnelsAndEndpoints).BeginInit();
            splitContainerTunnelsAndEndpoints.Panel1.SuspendLayout();
            splitContainerTunnelsAndEndpoints.Panel2.SuspendLayout();
            splitContainerTunnelsAndEndpoints.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerBody).BeginInit();
            splitContainerBody.Panel1.SuspendLayout();
            splitContainerBody.Panel2.SuspendLayout();
            splitContainerBody.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripBody
            // 
            menuStripBody.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, securityToolStripMenuItem, helpToolStripMenuItem });
            menuStripBody.Location = new Point(0, 0);
            menuStripBody.Name = "menuStripBody";
            menuStripBody.Size = new Size(912, 24);
            menuStripBody.TabIndex = 1;
            menuStripBody.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { connectToolStripMenuItem, toolStripMenuItem1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(56, 20);
            fileToolStripMenuItem.Text = "Service";
            // 
            // connectToolStripMenuItem
            // 
            connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            connectToolStripMenuItem.Size = new Size(119, 22);
            connectToolStripMenuItem.Text = "Connect";
            connectToolStripMenuItem.Click += ConnectToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(116, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(119, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // securityToolStripMenuItem
            // 
            securityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { usersToolStripMenuItem, configurationToolStripMenuItem });
            securityToolStripMenuItem.Name = "securityToolStripMenuItem";
            securityToolStripMenuItem.Size = new Size(61, 20);
            securityToolStripMenuItem.Text = "Settings";
            // 
            // usersToolStripMenuItem
            // 
            usersToolStripMenuItem.Name = "usersToolStripMenuItem";
            usersToolStripMenuItem.Size = new Size(148, 22);
            usersToolStripMenuItem.Text = "Users";
            usersToolStripMenuItem.Click += UsersToolStripMenuItem_Click;
            // 
            // configurationToolStripMenuItem
            // 
            configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            configurationToolStripMenuItem.Size = new Size(148, 22);
            configurationToolStripMenuItem.Text = "Configuration";
            configurationToolStripMenuItem.Click += ConfigurationToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(107, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
            // 
            // statusStripBody
            // 
            statusStripBody.Location = new Point(0, 547);
            statusStripBody.Name = "statusStripBody";
            statusStripBody.Size = new Size(912, 22);
            statusStripBody.TabIndex = 2;
            statusStripBody.Text = "statusStrip1";
            // 
            // splitContainerTunnelsAndEndpoints
            // 
            splitContainerTunnelsAndEndpoints.Dock = DockStyle.Fill;
            splitContainerTunnelsAndEndpoints.Location = new Point(0, 0);
            splitContainerTunnelsAndEndpoints.Name = "splitContainerTunnelsAndEndpoints";
            splitContainerTunnelsAndEndpoints.Orientation = Orientation.Horizontal;
            // 
            // splitContainerTunnelsAndEndpoints.Panel1
            // 
            splitContainerTunnelsAndEndpoints.Panel1.Controls.Add(listViewTunnels);
            splitContainerTunnelsAndEndpoints.Panel1.Controls.Add(labelTunnels);
            // 
            // splitContainerTunnelsAndEndpoints.Panel2
            // 
            splitContainerTunnelsAndEndpoints.Panel2.Controls.Add(listViewEndpoints);
            splitContainerTunnelsAndEndpoints.Panel2.Controls.Add(labelEndpoints);
            splitContainerTunnelsAndEndpoints.Size = new Size(912, 275);
            splitContainerTunnelsAndEndpoints.SplitterDistance = 135;
            splitContainerTunnelsAndEndpoints.TabIndex = 3;
            // 
            // listViewTunnels
            // 
            listViewTunnels.Dock = DockStyle.Fill;
            listViewTunnels.FullRowSelect = true;
            listViewTunnels.GridLines = true;
            listViewTunnels.Location = new Point(0, 15);
            listViewTunnels.MultiSelect = false;
            listViewTunnels.Name = "listViewTunnels";
            listViewTunnels.Size = new Size(912, 120);
            listViewTunnels.TabIndex = 2;
            listViewTunnels.UseCompatibleStateImageBehavior = false;
            listViewTunnels.View = View.Details;
            // 
            // labelTunnels
            // 
            labelTunnels.AutoSize = true;
            labelTunnels.Dock = DockStyle.Top;
            labelTunnels.Location = new Point(0, 0);
            labelTunnels.Name = "labelTunnels";
            labelTunnels.Size = new Size(48, 15);
            labelTunnels.TabIndex = 1;
            labelTunnels.Text = "Tunnels";
            // 
            // listViewEndpoints
            // 
            listViewEndpoints.Dock = DockStyle.Fill;
            listViewEndpoints.FullRowSelect = true;
            listViewEndpoints.GridLines = true;
            listViewEndpoints.Location = new Point(0, 15);
            listViewEndpoints.MultiSelect = false;
            listViewEndpoints.Name = "listViewEndpoints";
            listViewEndpoints.Size = new Size(912, 121);
            listViewEndpoints.TabIndex = 1;
            listViewEndpoints.UseCompatibleStateImageBehavior = false;
            listViewEndpoints.View = View.Details;
            // 
            // labelEndpoints
            // 
            labelEndpoints.AutoSize = true;
            labelEndpoints.Dock = DockStyle.Top;
            labelEndpoints.Location = new Point(0, 0);
            labelEndpoints.Name = "labelEndpoints";
            labelEndpoints.Size = new Size(196, 15);
            labelEndpoints.TabIndex = 2;
            labelEndpoints.Text = "Endpoints of (select a tunnel above)";
            // 
            // splitContainerBody
            // 
            splitContainerBody.Dock = DockStyle.Fill;
            splitContainerBody.Location = new Point(0, 24);
            splitContainerBody.Name = "splitContainerBody";
            splitContainerBody.Orientation = Orientation.Horizontal;
            // 
            // splitContainerBody.Panel1
            // 
            splitContainerBody.Panel1.Controls.Add(splitContainerTunnelsAndEndpoints);
            // 
            // splitContainerBody.Panel2
            // 
            splitContainerBody.Panel2.Controls.Add(listViewLogs);
            splitContainerBody.Size = new Size(912, 523);
            splitContainerBody.SplitterDistance = 275;
            splitContainerBody.TabIndex = 4;
            // 
            // listViewLogs
            // 
            listViewLogs.Columns.AddRange(new ColumnHeader[] { columnHeaderDateTime, columnHeaderSeverity, columnHeaderText });
            listViewLogs.Dock = DockStyle.Fill;
            listViewLogs.Location = new Point(0, 0);
            listViewLogs.Name = "listViewLogs";
            listViewLogs.Size = new Size(912, 244);
            listViewLogs.TabIndex = 0;
            listViewLogs.UseCompatibleStateImageBehavior = false;
            listViewLogs.View = View.Details;
            // 
            // columnHeaderDateTime
            // 
            columnHeaderDateTime.Text = "Date/Time";
            columnHeaderDateTime.Width = 150;
            // 
            // columnHeaderSeverity
            // 
            columnHeaderSeverity.Text = "Severity";
            columnHeaderSeverity.Width = 100;
            // 
            // columnHeaderText
            // 
            columnHeaderText.Text = "Text";
            columnHeaderText.Width = 600;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(912, 569);
            Controls.Add(splitContainerBody);
            Controls.Add(menuStripBody);
            Controls.Add(statusStripBody);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripBody;
            MinimumSize = new Size(600, 300);
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NetTunnel";
            Load += FormMain_Load;
            menuStripBody.ResumeLayout(false);
            menuStripBody.PerformLayout();
            splitContainerTunnelsAndEndpoints.Panel1.ResumeLayout(false);
            splitContainerTunnelsAndEndpoints.Panel1.PerformLayout();
            splitContainerTunnelsAndEndpoints.Panel2.ResumeLayout(false);
            splitContainerTunnelsAndEndpoints.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerTunnelsAndEndpoints).EndInit();
            splitContainerTunnelsAndEndpoints.ResumeLayout(false);
            splitContainerBody.Panel1.ResumeLayout(false);
            splitContainerBody.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerBody).EndInit();
            splitContainerBody.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStripBody;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem securityToolStripMenuItem;
        private ToolStripMenuItem usersToolStripMenuItem;
        private StatusStrip statusStripBody;
        private SplitContainer splitContainerTunnelsAndEndpoints;
        private Label labelTunnels;
        private DoubleBufferedListView listViewEndpoints;
        private Label labelEndpoints;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem configurationToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private SplitContainer splitContainerBody;
        private DoubleBufferedListView listViewLogs;
        private ColumnHeader columnHeaderDateTime;
        private ColumnHeader columnHeaderSeverity;
        private ColumnHeader columnHeaderText;
        private DoubleBufferedListView listViewTunnels;
    }
}