using NetTunnel.UI.Controls;

namespace NetTunnel.UI.Forms
{
    partial class FormEndpointProperties
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEndpointProperties));
            listViewProperties = new DoubleBufferedListView();
            columnHeaderProperty = new ColumnHeader();
            columnHeaderValue = new ColumnHeader();
            SuspendLayout();
            // 
            // listViewProperties
            // 
            listViewProperties.Columns.AddRange(new ColumnHeader[] { columnHeaderProperty, columnHeaderValue });
            listViewProperties.FullRowSelect = true;
            listViewProperties.Location = new Point(6, 6);
            listViewProperties.Name = "listViewProperties";
            listViewProperties.Size = new Size(652, 454);
            listViewProperties.TabIndex = 0;
            listViewProperties.UseCompatibleStateImageBehavior = false;
            listViewProperties.View = View.Details;
            // 
            // columnHeaderProperty
            // 
            columnHeaderProperty.Text = "Property";
            columnHeaderProperty.Width = 150;
            // 
            // columnHeaderValue
            // 
            columnHeaderValue.Text = "Value";
            columnHeaderValue.Width = 475;
            // 
            // FormEndpointProperties
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(664, 466);
            Controls.Add(listViewProperties);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormEndpointProperties";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel";
            ResumeLayout(false);
        }

        #endregion

        private DoubleBufferedListView listViewProperties;
        private ColumnHeader columnHeaderProperty;
        private ColumnHeader columnHeaderValue;
    }
}