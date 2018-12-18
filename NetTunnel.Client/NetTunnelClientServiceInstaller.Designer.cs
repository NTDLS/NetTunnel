namespace NetTunnel.Client
{
    partial class NetTunnelClientServiceInstaller
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
            this.clientServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // clientServiceProcessInstaller
            // 
            this.clientServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.clientServiceProcessInstaller.Password = null;
            this.clientServiceProcessInstaller.Username = null;
            // 
            // serviceInstaller
            // 
            this.serviceInstaller.Description = "Provides TCP/IP v4/v6/tunneling and routing services on NT based operating system" +
    "s.";
            this.serviceInstaller.DisplayName = "NetworkDLS NetTunnel Client";
            this.serviceInstaller.ServiceName = "NetTunnelClientService";
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // NetTunnelClientServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.clientServiceProcessInstaller,
            this.serviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller clientServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;
    }
}