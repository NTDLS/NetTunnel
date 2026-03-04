-----Ensure you have unzip installed.
sudo apt update
sudo apt install unzip

-----Install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

-----Then add it to your path:
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

-----Verify:
dotnet --version

-----Download NetTunnel
wget https://NTDLS.com/NetTunnel.Service.Linux.Arm64.zip

-----Create app directory and unzip:
mkdir -p ~/apps/NetTunnel
unzip NetTunnel.Service.Linux.Arm64.zip -d ~/apps/NetTunnel

-----Change to extraction directory.
cd ~/apps/NetTunnel

-----Make the application executable
chmod +x NetTunnel.Service

-----Run the executable
dotnet NetTunnel.Service.dll



-----Permissions:

sudo mkdir -p /usr/share/NetTunnel
sudo chown -R jpatterson:jpatterson /usr/share/NetTunnel

