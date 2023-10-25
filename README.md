# NetTunnel

![image](https://github.com/NTDLS/NetTunnel/assets/11428567/7be6ad6b-e1e6-4b48-b764-e07e2fa219e0)

NetTunnel is a TCP/IP that is designed to work from behind firewalls where the rules can not be altered. 

This is accomplished by installing the software at each location where the connection needs to be made FROM and TO. You will then create an OUTBOUND tunnel at the location that is behind a firewall and create an INBOUND tunnel at the location where you can receive an inbound connection (such as ay home). The service installation with the OUTBIUND tunnel configuration will make a outgoing connection to the other service.
Once the tunnel is connected you can manage endpoints which as nothing more than a listening port that will pump the data through the tunnel, exit at the other service installation and make a connection to the configured endpoint address and port.

All data exchanged between the INBOUND and OUTBOUND endpoints is routed through the encrypted and compressed tunnel to the corresponding endpoint.

