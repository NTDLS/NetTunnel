namespace NetTunnel.UI.Forms
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
            textBoxName = new TextBox();
            labelName = new Label();
            textBoxListenPort = new TextBox();
            labelListenPort = new Label();
            textBoxTerminationAddress = new TextBox();
            labelTerminationAddress = new Label();
            textBoxTerminationPort = new TextBox();
            labelTerminationPort = new Label();
            buttonAdd = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(16, 33);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(369, 23);
            textBoxName.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(16, 15);
            labelName.Name = "labelName";
            labelName.Size = new Size(115, 15);
            labelName.TabIndex = 8;
            labelName.Text = "Name or description";
            // 
            // textBoxListenPort
            // 
            textBoxListenPort.Location = new Point(16, 83);
            textBoxListenPort.Name = "textBoxListenPort";
            textBoxListenPort.Size = new Size(74, 23);
            textBoxListenPort.TabIndex = 1;
            // 
            // labelListenPort
            // 
            labelListenPort.AutoSize = true;
            labelListenPort.Location = new Point(16, 65);
            labelListenPort.Name = "labelListenPort";
            labelListenPort.Size = new Size(63, 15);
            labelListenPort.TabIndex = 13;
            labelListenPort.Text = "Listen port";
            // 
            // textBoxTerminationAddress
            // 
            textBoxTerminationAddress.Location = new Point(96, 83);
            textBoxTerminationAddress.Name = "textBoxTerminationAddress";
            textBoxTerminationAddress.Size = new Size(191, 23);
            textBoxTerminationAddress.TabIndex = 2;
            // 
            // labelTerminationAddress
            // 
            labelTerminationAddress.AutoSize = true;
            labelTerminationAddress.Location = new Point(96, 65);
            labelTerminationAddress.Name = "labelTerminationAddress";
            labelTerminationAddress.Size = new Size(147, 15);
            labelTerminationAddress.TabIndex = 15;
            labelTerminationAddress.Text = "Termination address / host";
            // 
            // textBoxTerminationPort
            // 
            textBoxTerminationPort.Location = new Point(293, 83);
            textBoxTerminationPort.Name = "textBoxTerminationPort";
            textBoxTerminationPort.Size = new Size(92, 23);
            textBoxTerminationPort.TabIndex = 3;
            // 
            // labelTerminationPort
            // 
            labelTerminationPort.AutoSize = true;
            labelTerminationPort.Location = new Point(293, 65);
            labelTerminationPort.Name = "labelTerminationPort";
            labelTerminationPort.Size = new Size(92, 15);
            labelTerminationPort.TabIndex = 17;
            labelTerminationPort.Text = "Destination Port";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(310, 124);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 4;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(229, 124);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FormAddEndpoint
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 159);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAdd);
            Controls.Add(textBoxTerminationPort);
            Controls.Add(labelTerminationPort);
            Controls.Add(textBoxTerminationAddress);
            Controls.Add(labelTerminationAddress);
            Controls.Add(textBoxListenPort);
            Controls.Add(labelListenPort);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAddEndpoint";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel : Endpoint";
            Load += FormAddEndpoint_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxName;
        private Label labelName;
        private TextBox textBoxListenPort;
        private Label labelListenPort;
        private TextBox textBoxTerminationAddress;
        private Label labelTerminationAddress;
        private TextBox textBoxTerminationPort;
        private Label labelTerminationPort;
        private Button buttonAdd;
        private Button buttonCancel;
    }
}