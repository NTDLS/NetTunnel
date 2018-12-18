using System;
using System.Collections.Generic;
using System.Linq;
using NetTunnel.Library.Tunneling;
using NetTunnel.Library.Win32;

namespace NetTunnel.Service.Tunneling
{
    public class Tunnelers
    {
        public List<Tunneler> List = new List<Tunneler>();

        public List<Tunnel> Tunnels()
        {
            var tunnels = new List<Tunnel>();

            foreach (var tunnel in List)
            {
                tunnels.Add(tunnel.Tunnel);
            }

            return tunnels;
        }

        public Tunneler this[System.Guid tunnelId]
        {
            get
            {
                return (from o in List where o.Tunnel.Id == tunnelId select o).FirstOrDefault();
            }
        }

        public void Add(Tunneler tunneler)
        {
            List.Add(tunneler);
        }

        public void Start()
        {
            KeepConnectedThread();

            foreach (var tunneler in List)
            {
                try
                {
                    tunneler.Start();
                }
                catch (Exception ex)
                {
                    Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                    {
                        Severity = EventLogging.Severity.Error,
                        CustomText = "Failed to start tunnel.",
                        Exception = ex
                    });
                }
            }
        }

        public void Stop()
        {
            foreach (var tunneler in List)
            {
                try
                {
                    tunneler.Stop();
                }
                catch (Exception ex)
                {
                    Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                    {
                        Severity = EventLogging.Severity.Error,
                        CustomText = "Failed to stop tunnel.",
                        Exception = ex
                    });
                }
            }
        }
   
    }
}
