namespace NetTunnel.UI.Forms
{
    partial class FormAddTunnel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddTunnel));
            textBoxName = new TextBox();
            labelName = new Label();
            radioButtonRemoteEndpoint = new RadioButton();
            radioButtonLocalEndpoint = new RadioButton();
            groupBoxListenEndpoint = new GroupBox();
            textBoxListenPort = new TextBox();
            labelListenPort = new Label();
            textBoxTerminationAddress = new TextBox();
            labelTerminationAddress = new Label();
            textBoxTerminationPort = new TextBox();
            labelTerminationPort = new Label();
            buttonAdd = new Button();
            buttonCancel = new Button();
            groupBoxListenEndpoint.SuspendLayout();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(16, 31);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(395, 23);
            textBoxName.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(16, 13);
            labelName.Name = "labelName";
            labelName.Size = new Size(115, 15);
            labelName.TabIndex = 8;
            labelName.Text = "Name or description";
            // 
            // radioButtonRemoteEndpoint
            // 
            radioButtonRemoteEndpoint.AutoSize = true;
            radioButtonRemoteEndpoint.Location = new Point(22, 22);
            radioButtonRemoteEndpoint.Name = "radioButtonRemoteEndpoint";
            radioButtonRemoteEndpoint.Size = new Size(117, 19);
            radioButtonRemoteEndpoint.TabIndex = 2;
            radioButtonRemoteEndpoint.Text = "Remote endpoint";
            radioButtonRemoteEndpoint.UseVisualStyleBackColor = true;
            // 
            // radioButtonLocalEndpoint
            // 
            radioButtonLocalEndpoint.AutoSize = true;
            radioButtonLocalEndpoint.Checked = true;
            radioButtonLocalEndpoint.Location = new Point(22, 53);
            radioButtonLocalEndpoint.Name = "radioButtonLocalEndpoint";
            radioButtonLocalEndpoint.Size = new Size(104, 19);
            radioButtonLocalEndpoint.TabIndex = 3;
            radioButtonLocalEndpoint.TabStop = true;
            radioButtonLocalEndpoint.Text = "Local endpoint";
            radioButtonLocalEndpoint.UseVisualStyleBackColor = true;
            // 
            // groupBoxListenEndpoint
            // 
            groupBoxListenEndpoint.Controls.Add(radioButtonRemoteEndpoint);
            groupBoxListenEndpoint.Controls.Add(radioButtonLocalEndpoint);
            groupBoxListenEndpoint.Location = new Point(16, 74);
            groupBoxListenEndpoint.Name = "groupBoxListenEndpoint";
            groupBoxListenEndpoint.Size = new Size(163, 89);
            groupBoxListenEndpoint.TabIndex = 1;
            groupBoxListenEndpoint.TabStop = false;
            groupBoxListenEndpoint.Text = "Listen Endpoint";
            // 
            // textBoxListenPort
            // 
            textBoxListenPort.Location = new Point(192, 92);
            textBoxListenPort.Name = "textBoxListenPort";
            textBoxListenPort.Size = new Size(63, 23);
            textBoxListenPort.TabIndex = 4;
            // 
            // labelListenPort
            // 
            labelListenPort.AutoSize = true;
            labelListenPort.Location = new Point(192, 74);
            labelListenPort.Name = "labelListenPort";
            labelListenPort.Size = new Size(63, 15);
            labelListenPort.TabIndex = 13;
            labelListenPort.Text = "Listen port";
            // 
            // textBoxTerminationAddress
            // 
            textBoxTerminationAddress.Location = new Point(192, 140);
            textBoxTerminationAddress.Name = "textBoxTerminationAddress";
            textBoxTerminationAddress.Size = new Size(150, 23);
            textBoxTerminationAddress.TabIndex = 5;
            // 
            // labelTerminationAddress
            // 
            labelTerminationAddress.AutoSize = true;
            labelTerminationAddress.Location = new Point(192, 122);
            labelTerminationAddress.Name = "labelTerminationAddress";
            labelTerminationAddress.Size = new Size(147, 15);
            labelTerminationAddress.TabIndex = 15;
            labelTerminationAddress.Text = "Termination address / host";
            // 
            // textBoxTerminationPort
            // 
            textBoxTerminationPort.Location = new Point(348, 140);
            textBoxTerminationPort.Name = "textBoxTerminationPort";
            textBoxTerminationPort.Size = new Size(63, 23);
            textBoxTerminationPort.TabIndex = 6;
            // 
            // labelTerminationPort
            // 
            labelTerminationPort.AutoSize = true;
            labelTerminationPort.Location = new Point(348, 122);
            labelTerminationPort.Name = "labelTerminationPort";
            labelTerminationPort.Size = new Size(29, 15);
            labelTerminationPort.TabIndex = 17;
            labelTerminationPort.Text = "Port";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(255, 195);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 7;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(336, 195);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FormAddTunnel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(434, 231);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAdd);
            Controls.Add(textBoxTerminationPort);
            Controls.Add(labelTerminationPort);
            Controls.Add(textBoxTerminationAddress);
            Controls.Add(labelTerminationAddress);
            Controls.Add(textBoxListenPort);
            Controls.Add(labelListenPort);
            Controls.Add(groupBoxListenEndpoint);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAddTunnel";
            ShowInTaskbar = false;
            Text = "NetTunnel : Tunnel";
            groupBoxListenEndpoint.ResumeLayout(false);
            groupBoxListenEndpoint.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxName;
        private Label labelName;
        private RadioButton radioButtonRemoteEndpoint;
        private RadioButton radioButtonLocalEndpoint;
        private GroupBox groupBoxListenEndpoint;
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