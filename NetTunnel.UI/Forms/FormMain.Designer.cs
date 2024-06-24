﻿using NetTunnel.UI.Controls;

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
            listViewTunnels = new DoubleBufferedListView();
            menuStripBody = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            connectToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            securityToolStripMenuItem = new ToolStripMenuItem();
            usersToolStripMenuItem = new ToolStripMenuItem();
            configurationToolStripMenuItem1 = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            statusStripBody = new StatusStrip();
            splitContainer1 = new SplitContainer();
            labelTunnels = new Label();
            listViewEndpoints = new ListView();
            labelEndpoints = new Label();
            menuStripBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // listViewTunnels
            // 
            listViewTunnels.Dock = DockStyle.Fill;
            listViewTunnels.FullRowSelect = true;
            listViewTunnels.GridLines = true;
            listViewTunnels.Location = new Point(0, 15);
            listViewTunnels.MultiSelect = false;
            listViewTunnels.Name = "listViewTunnels";
            listViewTunnels.Size = new Size(912, 243);
            listViewTunnels.TabIndex = 0;
            listViewTunnels.UseCompatibleStateImageBehavior = false;
            listViewTunnels.View = View.Details;
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
            securityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { usersToolStripMenuItem, configurationToolStripMenuItem1 });
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
            // configurationToolStripMenuItem1
            // 
            configurationToolStripMenuItem1.Name = "configurationToolStripMenuItem1";
            configurationToolStripMenuItem1.Size = new Size(148, 22);
            configurationToolStripMenuItem1.Text = "Configuration";
            configurationToolStripMenuItem1.Click += ConfigurationToolStripMenuItem_Click;
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
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listViewTunnels);
            splitContainer1.Panel1.Controls.Add(labelTunnels);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listViewEndpoints);
            splitContainer1.Panel2.Controls.Add(labelEndpoints);
            splitContainer1.Size = new Size(912, 523);
            splitContainer1.SplitterDistance = 258;
            splitContainer1.TabIndex = 3;
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
            listViewEndpoints.Size = new Size(912, 246);
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
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(912, 569);
            Controls.Add(splitContainer1);
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
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DoubleBufferedListView listViewTunnels;
        private MenuStrip menuStripBody;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem securityToolStripMenuItem;
        private ToolStripMenuItem usersToolStripMenuItem;
        private StatusStrip statusStripBody;
        private SplitContainer splitContainer1;
        private Label labelTunnels;
        private ListView listViewEndpoints;
        private Label labelEndpoints;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem configurationToolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem1;
    }
}