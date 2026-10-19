# NetTunnel service on Linux (x64 and arm64)

The Linux build is **self contained**: it carries its own .NET runtime, so nothing has to be installed first. The management UI
is a Windows application: run it on a Windows machine and connect it to the service (default port **52845**).

## Download and unpack

Use the `x64` or the `Arm64` zip to match the machine (`uname -m` says `x86_64` or `aarch64`).

```bash
sudo apt update
sudo apt install unzip wget

wget https://NTDLS.com/NetTunnel.Service.Linux.x64.zip      # or NetTunnel.Service.Linux.Arm64.zip

mkdir -p ~/apps/NetTunnel
unzip NetTunnel.Service.Linux.x64.zip -d ~/apps/NetTunnel
cd ~/apps/NetTunnel
chmod +x NetTunnel.Service
```

## Where things are kept

The configuration (users, tunnels and endpoints, and the key that protects the passwords in them) is kept in `/usr/share/NetTunnel`.
The service needs to be able to write there. Use the account that runs the service:

```bash
sudo mkdir -p /usr/share/NetTunnel
sudo chown -R $USER:$USER /usr/share/NetTunnel
```

Back up that whole folder, including `secret.key`: the user passwords and certificate passwords in the configuration cannot be read
without it.

The service writes its log to the console only. To also write a rolling log file, set the log path in the UI
(*Service Configuration*): a relative path is relative to the folder the service runs from.

## Run it by hand

```bash
./NetTunnel.Service
```

## Run it as a service (systemd)

Create `/etc/systemd/system/nettunnel.service`:

```ini
[Unit]
Description=NetTunnel
After=network.target

[Service]
WorkingDirectory=/home/<user>/apps/NetTunnel
ExecStart=/home/<user>/apps/NetTunnel/NetTunnel.Service
User=<user>
Restart=always
RestartSec=2
AmbientCapabilities=CAP_NET_BIND_SERVICE

[Install]
WantedBy=multi-user.target
```

Then:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now nettunnel
sudo systemctl status nettunnel
journalctl -u nettunnel -f
```

`AmbientCapabilities` lets a service that does not run as root listen on ports below 1024 (an inbound endpoint on port 80 or 443,
for example). When run by hand, use `sudo setcap 'cap_net_bind_service=+ep' ./NetTunnel.Service` instead.

## Notes

* A new installation has one user, `root`, whose password is the **lower case name of the machine** (`hostname`). Change it first
  thing in the UI.
* Both ends of a tunnel, and the UI, must run the same version: the connections between them are encrypted, and an older version
  does not do that in a way a current one can talk to.
* Open the service port (52845) only to the machines that need it (the UI, and the other NetTunnel services that tunnel to this one),
  and each inbound endpoint's port to whoever should use it.
* The service listens on IPv6 as well as IPv4 by default (*Service Configuration* can turn that off). Check the firewall for both.
* The Windows event log is not available on Linux: use the console output (`journalctl`) or a log file.
