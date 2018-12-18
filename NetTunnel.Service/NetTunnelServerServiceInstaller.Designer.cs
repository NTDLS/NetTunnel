namespace NetTunnel.Service
{
    partial class NetTunnelServerServiceInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serverServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serverServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serverServiceProcessInstaller
            // 
            this.serverServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serverServiceProcessInstaller.Password = null;
            this.serverServiceProcessInstaller.Username = null;
            // 
            // serverServiceInstaller
            // 
            this.serverServiceInstaller.Description = "Provides TCP/IP v4/v6/tunneling and routing services on NT based operating system" +
    "s.";
            this.serverServiceInstaller.DisplayName = "NetworkDLS NetTunnel";
            this.serverServiceInstaller.ServiceName = "NetTunnelServerService";
            this.serverServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // NetTunnelServerServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serverServiceProcessInstaller,
            this.serverServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serverServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serverServiceInstaller;
    }
}