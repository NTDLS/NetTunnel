using NetTunnel.UI.Controls;

namespace NetTunnel.UI.Forms
{
    partial class FormEndpointEdgeConnections
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEndpointEdgeConnections));
            listViewConnections = new DoubleBufferedListView();
            columnHeaderProperty = new ColumnHeader();
            columnHeaderValue = new ColumnHeader();
            SuspendLayout();
            // 
            // listViewConnections
            // 
            listViewConnections.Columns.AddRange(new ColumnHeader[] { columnHeaderProperty, columnHeaderValue });
            listViewConnections.Dock = DockStyle.Fill;
            listViewConnections.FullRowSelect = true;
            listViewConnections.Location = new Point(0, 0);
            listViewConnections.Name = "listViewConnections";
            listViewConnections.Size = new Size(704, 381);
            listViewConnections.TabIndex = 0;
            listViewConnections.UseCompatibleStateImageBehavior = false;
            listViewConnections.View = View.Details;
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
            // FormEndpointEdgeConnections
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 381);
            Controls.Add(listViewConnections);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimizeBox = false;
            Name = "FormEndpointEdgeConnections";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NetTunnel";
            ResumeLayout(false);
        }

        #endregion

        private DoubleBufferedListView listViewConnections;
        private ColumnHeader columnHeaderProperty;
        private ColumnHeader columnHeaderValue;
    }
}