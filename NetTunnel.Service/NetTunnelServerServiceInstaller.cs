﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace NetTunnel.Service
{
    [RunInstaller(true)]
    public partial class NetTunnelServerServiceInstaller : System.Configuration.Install.Installer
    {
        public NetTunnelServerServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
