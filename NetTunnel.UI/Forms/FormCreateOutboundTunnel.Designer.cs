namespace NetTunnel.UI.Forms
{
    partial class FormCreateOutboundTunnel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCreateOutboundTunnel));
            labelRemoteAddress = new Label();
            labelRemoteUsername = new Label();
            labelRemotePassword = new Label();
            textBoxRemoteAddress = new TextBox();
            textBoxRemoteUsername = new TextBox();
            textBoxRemotePassword = new TextBox();
            textBoxManagementPort = new TextBox();
            labelPort = new Label();
            buttonCancel = new Button();
            textBoxName = new TextBox();
            labelName = new Label();
            tabControlBody = new TabControl();
            tabPageBasics = new TabPage();
            tabPageRemoteService = new TabPage();
            tabPageTunnel = new TabPage();
            labelSelectATunnel = new Label();
            listViewTunnels = new ListView();
            columnHeaderTunnelName = new ColumnHeader();
            buttonNext = new Button();
            buttonPrevious = new Button();
            tabControlBody.SuspendLayout();
            tabPageBasics.SuspendLayout();
            tabPageRemoteService.SuspendLayout();
            tabPageTunnel.SuspendLayout();
            SuspendLayout();
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(13, 21);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(86, 15);
            labelRemoteAddress.TabIndex = 0;
            labelRemoteAddress.Text = "Address / host:";
            // 
            // labelRemoteUsername
            // 
            labelRemoteUsername.AutoSize = true;
            labelRemoteUsername.Location = new Point(14, 75);
            labelRemoteUsername.Name = "labelRemoteUsername";
            labelRemoteUsername.Size = new Size(63, 15);
            labelRemoteUsername.TabIndex = 1;
            labelRemoteUsername.Text = "Username:";
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(13, 124);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(57, 15);
            labelRemotePassword.TabIndex = 2;
            labelRemotePassword.Text = "Password";
            // 
            // textBoxRemoteAddress
            // 
            textBoxRemoteAddress.Location = new Point(14, 39);
            textBoxRemoteAddress.Name = "textBoxRemoteAddress";
            textBoxRemoteAddress.Size = new Size(156, 23);
            textBoxRemoteAddress.TabIndex = 2;
            // 
            // textBoxRemoteUsername
            // 
            textBoxRemoteUsername.Location = new Point(14, 93);
            textBoxRemoteUsername.Name = "textBoxRemoteUsername";
            textBoxRemoteUsername.Size = new Size(239, 23);
            textBoxRemoteUsername.TabIndex = 4;
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(14, 142);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.PasswordChar = '*';
            textBoxRemotePassword.Size = new Size(239, 23);
            textBoxRemotePassword.TabIndex = 5;
            // 
            // textBoxManagementPort
            // 
            textBoxManagementPort.Location = new Point(176, 39);
            textBoxManagementPort.Name = "textBoxManagementPort";
            textBoxManagementPort.Size = new Size(77, 23);
            textBoxManagementPort.TabIndex = 3;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(176, 21);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(29, 15);
            labelPort.TabIndex = 4;
            labelPort.Text = "Port";
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(12, 295);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(14, 39);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(307, 23);
            textBoxName.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(14, 21);
            labelName.Name = "labelName";
            labelName.Size = new Size(115, 15);
            labelName.TabIndex = 6;
            labelName.Text = "Name or description";
            // 
            // tabControlBody
            // 
            tabControlBody.Controls.Add(tabPageBasics);
            tabControlBody.Controls.Add(tabPageRemoteService);
            tabControlBody.Controls.Add(tabPageTunnel);
            tabControlBody.Location = new Point(12, 12);
            tabControlBody.Name = "tabControlBody";
            tabControlBody.SelectedIndex = 0;
            tabControlBody.Size = new Size(345, 281);
            tabControlBody.TabIndex = 29;
            // 
            // tabPageBasics
            // 
            tabPageBasics.Controls.Add(labelName);
            tabPageBasics.Controls.Add(textBoxName);
            tabPageBasics.Location = new Point(4, 24);
            tabPageBasics.Name = "tabPageBasics";
            tabPageBasics.Size = new Size(337, 253);
            tabPageBasics.TabIndex = 2;
            tabPageBasics.Text = "Basics";
            tabPageBasics.UseVisualStyleBackColor = true;
            // 
            // tabPageRemoteService
            // 
            tabPageRemoteService.Controls.Add(labelRemoteAddress);
            tabPageRemoteService.Controls.Add(labelRemoteUsername);
            tabPageRemoteService.Controls.Add(labelRemotePassword);
            tabPageRemoteService.Controls.Add(labelPort);
            tabPageRemoteService.Controls.Add(textBoxRemoteAddress);
            tabPageRemoteService.Controls.Add(textBoxManagementPort);
            tabPageRemoteService.Controls.Add(textBoxRemoteUsername);
            tabPageRemoteService.Controls.Add(textBoxRemotePassword);
            tabPageRemoteService.Location = new Point(4, 24);
            tabPageRemoteService.Name = "tabPageRemoteService";
            tabPageRemoteService.Padding = new Padding(3);
            tabPageRemoteService.Size = new Size(337, 253);
            tabPageRemoteService.TabIndex = 0;
            tabPageRemoteService.Text = "Remote Service";
            tabPageRemoteService.UseVisualStyleBackColor = true;
            // 
            // tabPageTunnel
            // 
            tabPageTunnel.Controls.Add(labelSelectATunnel);
            tabPageTunnel.Controls.Add(listViewTunnels);
            tabPageTunnel.Location = new Point(4, 24);
            tabPageTunnel.Name = "tabPageTunnel";
            tabPageTunnel.Padding = new Padding(3);
            tabPageTunnel.Size = new Size(337, 253);
            tabPageTunnel.TabIndex = 1;
            tabPageTunnel.Text = "Tunnel";
            tabPageTunnel.UseVisualStyleBackColor = true;
            // 
            // labelSelectATunnel
            // 
            labelSelectATunnel.AutoSize = true;
            labelSelectATunnel.Location = new Point(3, 3);
            labelSelectATunnel.Name = "labelSelectATunnel";
            labelSelectATunnel.Size = new Size(202, 15);
            labelSelectATunnel.TabIndex = 1;
            labelSelectATunnel.Text = "Select a remote tunnel to connect to:";
            // 
            // listViewTunnels
            // 
            listViewTunnels.Columns.AddRange(new ColumnHeader[] { columnHeaderTunnelName });
            listViewTunnels.FullRowSelect = true;
            listViewTunnels.GridLines = true;
            listViewTunnels.HeaderStyle = ColumnHeaderStyle.None;
            listViewTunnels.Location = new Point(3, 21);
            listViewTunnels.Name = "listViewTunnels";
            listViewTunnels.Size = new Size(328, 226);
            listViewTunnels.TabIndex = 0;
            listViewTunnels.UseCompatibleStateImageBehavior = false;
            listViewTunnels.View = View.Details;
            // 
            // columnHeaderTunnelName
            // 
            columnHeaderTunnelName.Text = "Name";
            columnHeaderTunnelName.Width = 300;
            // 
            // buttonNext
            // 
            buttonNext.Location = new Point(270, 295);
            buttonNext.Name = "buttonNext";
            buttonNext.Size = new Size(83, 23);
            buttonNext.TabIndex = 30;
            buttonNext.Text = "Next >";
            buttonNext.UseVisualStyleBackColor = true;
            buttonNext.Click += buttonNext_Click;
            // 
            // buttonPrevious
            // 
            buttonPrevious.Location = new Point(181, 295);
            buttonPrevious.Name = "buttonPrevious";
            buttonPrevious.Size = new Size(83, 23);
            buttonPrevious.TabIndex = 31;
            buttonPrevious.Text = "< Previous";
            buttonPrevious.UseVisualStyleBackColor = true;
            buttonPrevious.Click += buttonPrevious_Click;
            // 
            // FormCreateOutboundTunnel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(366, 330);
            Controls.Add(buttonPrevious);
            Controls.Add(buttonNext);
            Controls.Add(tabControlBody);
            Controls.Add(buttonCancel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCreateOutboundTunnel";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Create Outbound Tunnel";
            Load += FormCreateOutboundTunnel_Load;
            tabControlBody.ResumeLayout(false);
            tabPageBasics.ResumeLayout(false);
            tabPageBasics.PerformLayout();
            tabPageRemoteService.ResumeLayout(false);
            tabPageRemoteService.PerformLayout();
            tabPageTunnel.ResumeLayout(false);
            tabPageTunnel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label labelRemoteAddress;
        private Label labelRemoteUsername;
        private Label labelRemotePassword;
        private TextBox textBoxRemoteAddress;
        private TextBox textBoxRemoteUsername;
        private TextBox textBoxRemotePassword;
        private TextBox textBoxManagementPort;
        private Label labelPort;
        private Button buttonAdd;
        private Button buttonCancel;
        private TextBox textBoxName;
        private Label labelName;
        private Label labelTunnelDataPort;
        private TabControl tabControlBody;
        private TabPage tabPageRemoteService;
        private TabPage tabPageTunnel;
        private TabPage tabPageBasics;
        private Button buttonNext;
        private Button buttonPrevious;
        private Label labelSelectATunnel;
        private ListView listViewTunnels;
        private ColumnHeader columnHeaderTunnelName;
    }
}