using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using NetTunnel.Library;
using NetTunnel.Library.Tunneling;
using NetTunnel.Library.Win32;

namespace NetTunnel.Service.Tunneling
{
    public class Tunneler
    {
        #region Backend Variables.

        private readonly Tunnel _tunnel;
        private Socket _listenSocket = null;
        private readonly List<SocketState> _connections = new List<SocketState>();
        private AsyncCallback _onDataReceivedCallback;

        public int CurrentConnectionCount
        {
            get
            {
                lock (_connections)
                {
                    return _connections.Count;
                }
            }
        }

        public Tunnel Tunnel
        {
            get
            {
                return _tunnel;
            }
        }

        #endregion

        public Tunneler(Tunnel tunnel)
        {
            this._tunnel = tunnel;
        }

        public bool IsRunning
        {
            get
            {
                return _listenSocket != null;
            }
        }

        #region Start/Stop.

        public bool Start()
        {
            if (IsRunning)
            {
                return true;
            }

            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

                if (_tunnel.BindingProtocal == BindingProtocal.Pv6)
                {
                    _listenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                else if (_tunnel.BindingProtocal == BindingProtocal.Pv4)
                {
                    _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (_tunnel.ListenOnAllAddresses)
                {
                    if (_tunnel.BindingProtocal == BindingProtocal.Pv6)
                    {
                        IPEndPoint ipLocal = new IPEndPoint(IPAddress.IPv6Any, _tunnel.ListenPort);
                        _listenSocket.Bind(ipLocal);
                    }
                    else if (_tunnel.BindingProtocal == BindingProtocal.Pv4)
                    {
                        IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, _tunnel.ListenPort);
                        _listenSocket.Bind(ipLocal);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    foreach (var binding in _tunnel.Bindings)
                    {
                        if (binding.Enabled)
                        {
                            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(binding.Address), _tunnel.ListenPort);
                            _listenSocket.Bind(ipLocal);
                        }
                    }
                }

                _listenSocket.Listen(_tunnel.AcceptBacklogSize);
                _listenSocket.BeginAccept(new AsyncCallback(OnConnectionAccepted), null);

                return true;
            }
            catch (Exception ex)
            {
                if (_listenSocket != null)
                {
                    try
                    {
                        _listenSocket.Close();
                    }
                    catch
                    {
                    }
                }
                _listenSocket = null;

                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to start tunnel.",
                    Exception = ex
                });

                throw;
            }

            //return false;
        }

        public void Stop()
        {
            if (IsRunning == false)
            {
                return;
            }

            try
            {
                if (_listenSocket != null)
                {
                    _listenSocket.Close();
                    _listenSocket = null;
                }

                lock (_connections)
                {
                    while (_connections.Count > 0)
                    {
                        try
                        {
                            CleanupConnection(_connections[0]);
                        }
                        catch
                        {
                        }
                    }

                    _connections.Clear();
                }
            }
            catch (Exception ex)
            {
                _listenSocket = null;

                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to stop tunnel.",
                    Exception = ex
                });
                throw;
            }
        }

        #endregion

        #region Connect / Accept.

        private void OnConnectionAccepted(IAsyncResult asyn)
        {
            try
            {
                if (_listenSocket != null)
                {
                    Socket socket = _listenSocket.EndAccept(asyn);
                    SocketState connection = new SocketState(socket, _tunnel.InitialBufferSize);

                    connection.Tunnel = _tunnel;

                    (new Thread(EstablishPeerConnection)).Start(connection);

                    _listenSocket.BeginAccept(new AsyncCallback(OnConnectionAccepted), null);
                }
            }
            catch
            {
                //Discard.
            }
        }

        public SocketState Connect(string hostName, int port)
        {
            IPAddress ipAddress = GetIpAddress(hostName);
            if (ipAddress != null)
            {
                return Connect(ipAddress, port);
            }
            return null;
        }

        public SocketState Connect(IPAddress ipAddress, int port)
        {
            try
            {
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd = new IPEndPoint(ipAddress, port);

                socket.Connect(ipEnd);
                if (socket.Connected)
                {
                    var connection = new SocketState(socket, _tunnel.InitialBufferSize);

                    connection.Tunnel = _tunnel;

                    return connection;
                }
            }
            catch
            {
            }

            return null;
        }

        private void ProcessPeerCommand(SocketState connection, PacketEnvelope envelope)
        {
            /*
            if (envelope.Label == IntraServiceLables.ApplyResponseNegotiationToken)
            {
                connection.KeyNegotiator.ApplyNegotiationResponseToken(envelope.Payload);

                SendPacketEnvelope(connection, new PacketEnvelope
                {
                    Label = IntraServiceLables.EncryptionNegotationComplete
                });
            }
            */
        }

        void EstablishPeerConnection(object connectionObject)
        {
            SocketState accpetedConnection = (SocketState)connectionObject;

            Endpoint foreignConnectionEndpoint = null;
            SocketState foreignConnection = null;

            foreach (var remotePeer in _tunnel.Endpoints.List)
            {
                if (remotePeer.Enabled)
                {
                    if ((foreignConnection = Connect(remotePeer.Address, remotePeer.Port)) != null)
                    {
                        foreignConnectionEndpoint = remotePeer;
                        break;
                    }
                }
            }

            if (foreignConnection == null)
            {
                CleanupConnection(accpetedConnection);
                return;
            }

            accpetedConnection.Peer = foreignConnection;
            foreignConnection.Peer = accpetedConnection;

            lock (_connections)
            {
                _connections.Add(accpetedConnection);
                _connections.Add(foreignConnection);
            }

            WaitForData(accpetedConnection);
            WaitForData(foreignConnection);
        }

        #endregion

        private void WaitForData(SocketState connection)
        {
            try
            {
                if (_onDataReceivedCallback == null)
                {
                    _onDataReceivedCallback = new AsyncCallback(OnDataReceived);
                }

                if (connection.BytesReceived == connection.Buffer.Length && connection.BytesReceived < _tunnel.MaxBufferSize)
                {
                    int largerBufferSize = connection.Buffer.Length + (connection.Buffer.Length / 4);
                    connection.Buffer = new byte[largerBufferSize];
                }

                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, _onDataReceivedCallback, connection);
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "An error occured while waiting on data.",
                    Exception = ex
                });
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            SocketState connection = null;

            try
            {
                connection = (SocketState)asyn.AsyncState;
                connection.BytesReceived = connection.Socket.EndReceive(asyn);

                if (connection.BytesReceived == 0)
                {
                    CleanupConnection(connection);
                    return;
                }

                //Console.WriteLine("--Recv:{0}, Packet: {1}", tunnel.Name, Encoding.UTF8.GetString(connection.Buffer.Take(connection.BytesReceived).ToArray()));

                List<PacketEnvelope> envelopes = Packetizer.DissasemblePacketData(connection, false, null, null);
                foreach (var envelope in envelopes)
                {
                    if (envelope.Label == null)
                    {

                        ProcessReceivedData(connection, envelope.Payload, envelope.Payload.Length);
                    }
                    else
                    {
                        ProcessPeerCommand(connection, envelope);
                    }
                }

                WaitForData(connection);
            }
            catch (ObjectDisposedException)
            {
                if (connection != null)
                {
                    CleanupConnection(connection);
                }
                return;
            }
            catch (SocketException)
            {
                if (connection != null)
                {
                    CleanupConnection(connection);
                }
                return;
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to process received data.",
                    Exception = ex
                });
            }
        }

        void ProcessReceivedData(SocketState connection, byte[] buffer, int bufferSize)
        {
            //Console.WriteLine("--Send:{0}, Packet: {1}", tunnel.Name, Encoding.UTF8.GetString(buffer.Take(bufferSize).ToArray()));

            byte[] sendBuffer = Packetizer.AssembleMessagePacket(buffer, bufferSize, false, null, null);
            connection.Peer.Socket.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
        }

        private void SendPacketEnvelope(SocketState connection, PacketEnvelope envelope, string encryptionKey, string salt)
        {
            byte[] sendBuffer = Packetizer.AssembleMessagePacket(envelope, true, encryptionKey, salt);
            connection.Socket.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
        }

        private void SendPacketEnvelope(SocketState connection, PacketEnvelope envelope)
        {
            byte[] sendBuffer = Packetizer.AssembleMessagePacket(envelope, false, null, null);
            connection.Socket.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
        }

        private void CleanupConnection(SocketState connection)
        {
            try
            {
                CleanupSocket(connection.Socket);

                lock (_connections)
                {
                    _connections.Remove(connection);
                }

                if (connection.Peer != null)
                {
                    CleanupSocket(connection.Peer.Socket);

                    lock (_connections)
                    {
                        _connections.Remove(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to clean up connection.",
                    Exception = ex
                });
            }
        }

        private void CleanupSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            try
            {
                socket.Disconnect(false);
            }
            catch
            {
            }
            try
            {
                socket.Close();
            }
            catch
            {
            }
        }

        #region Utility.
        
        public static IPAddress GetIpAddress(string hostName)
        {
            try
            {
                string ip4Address = String.Empty;

                foreach (IPAddress ipAddress in Dns.GetHostAddresses(hostName))
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork
                        || ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        return ipAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to obtain IP address.",
                    Exception = ex
                });
            }

            return null;
        }

        #endregion
    }
}
