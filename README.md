# NetTunnel

## NetTunnel is a TCP/IP tool that is designed to allow ingress traversal through firewalls where the rules can not be altered. 

![Screenshot 2024-06-11 165343](https://github.com/NTDLS/NetTunnel/assets/11428567/40a59de6-7626-4cfc-9081-2427a938b1a8)

This is accomplished by installing the NetTunnel software at each location where the connection needs to be made FROM and TO. You will then create an OUTBOUND tunnel at the location that is behind a firewall, by doing so the NetTunnel software will reach out to the other remote installation and create an associated INBOUND tunnel. The INBOUND installation would be at a location where you can receive an inbound connection (such as home).

Once configured, the NetTunnel service installation with the OUTBOUND tunnel will make a outgoing connection to the other service.

Once the tunnel is connected you can add/manage endpoints, which are nothing more than a listening port that will pump data through the tunnel, exit at the other NetTunnel service installation and make a connection to the configured endpoint address and port.

## Operational concept
![image](https://github.com/NTDLS/NetTunnel/assets/11428567/ee826f0f-fced-4d0e-a577-a8a32e709571)

Note that in the diagram above, that we are routing to the remote server, but we do do not have its IP address nor do we have any inbound firewall rules defined. All data exchanged between the INBOUND and OUTBOUND endpoints is routed through the encrypted and compressed tunnel to the corresponding endpoint. That tunnel was established as an outbound connection FROM the remote server.

## 3rd and subsequent endpoint hops
If you are in a situation where both endpoints are behind firewalls, you can get creative and have both endpoint endpoint tunnels reach out to a 3rd (or 4th, or 5th....) location which can accept incoming connections from both sites.

## Notes
* All configuration and handshakes are done though plain ol' HTTPS, we here at NetworkDLS like our proprietary (and better) frameworks, but we use HTTPS here as to not freakout any outbound firewall rules.
* We use [diffie hellman](https://github.com/NTDLS/NTDLS.SecureKeyExchange) to create and exchange a 960bit key for each tunnel at each startup.
* All data exchanged though the tunnel encrypted using the [NASCCL](https://github.com/NTDLS/NTDLS.NASCCL) symmetric cipher.
* All data exchanged though the tunnel is compressed using the deflate algorithm.
* The UI connects to standard HTTPS endpoints using 2048bit asymmetric encryption.
