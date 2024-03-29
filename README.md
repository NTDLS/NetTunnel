# NetTunnel

## NetTunnel is a TCP/IP proxy that is designed to work from behind firewalls where the rules can not be altered. 

![image](https://github.com/NTDLS/NetTunnel/assets/11428567/6177b0bb-a726-441f-a069-bd6fe10b9c33)

This is accomplished by installing the software at each location where the connection needs to be made FROM and TO. You will then create an OUTBOUND tunnel at the location that is behind a firewall and create an INBOUND tunnel at the location where you can receive an inbound connection (such as ay home). The service installation with the OUTBIUND tunnel configuration will make a outgoing connection to the other service.
Once the tunnel is connected you can manage endpoints which as nothing more than a listening port that will pump the data through the tunnel, exit at the other service installation and make a connection to the configured endpoint address and port.

## Operational concept
![image](https://github.com/NTDLS/NetTunnel/assets/11428567/ee826f0f-fced-4d0e-a577-a8a32e709571)

Note that in the diagram above, that we are routing to the remote server, but we do do not have its IP address nor do we have any inbound firewall rules defined. All data exchanged between the INBOUND and OUTBOUND endpoints is routed through the encrypted and compressed tunnel to the corresponding endpoint. That tunnel was established as an outbound connection FROM the remote server.
