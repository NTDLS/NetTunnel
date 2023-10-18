namespace NetTunnel.UI
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
            listView1 = new ListView();
            columnHeaderName = new ColumnHeader();
            columnEntrypoint = new ColumnHeader();
            columnHeaderPort = new ColumnHeader();
            columnHeaderTunnels = new ColumnHeader();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeaderName, columnEntrypoint, columnHeaderPort, columnHeaderTunnels });
            listView1.Dock = DockStyle.Fill;
            listView1.GridLines = true;
            listView1.Location = new Point(0, 0);
            listView1.Name = "listView1";
            listView1.Size = new Size(751, 463);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
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
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(751, 463);
            Controls.Add(listView1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NetTunnel";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListView listView1;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnEntrypoint;
        private ColumnHeader columnHeaderPort;
        private ColumnHeader columnHeaderTunnels;
    }
}