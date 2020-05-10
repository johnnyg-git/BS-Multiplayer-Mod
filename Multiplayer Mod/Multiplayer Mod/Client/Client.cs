using BS;
using Multiplayer_Mod;
using Multiplayer_Mod.DataHolders;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Threading;
using UnityEngine;
using ItemData = Multiplayer_Mod.DataHolders.ItemData;

namespace Multiplayer_Mod.Client
{
    /// <summary>
    /// Clientside client manager
    /// </summary>
    public class Client
    {
        // Current client instance
        public static Client instance;

        // Packet size
        public static int dataBufferSize = 4096;
        // Connected ip
        public static string ip = "";
        // Connected port
        public static int port = 26950;

        // Clients id
        public static int myId;
        // Is connected
        public static bool connected;

        // Current tcp manager
        public static TCP tcp;
        // Current udp manager
        public static UDP udp;

        // Packet handlers and the id of what packet type they are handing
        public static Dictionary<int, PacketHandler> packetHandlers;
        // How packet handling methods should be setup
        public delegate void PacketHandler(Packet _packet);

        public static Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
        public static Dictionary<int, ItemData> items = new Dictionary<int, ItemData>();
        public static Dictionary<Item, ItemData> sendingItems = new Dictionary<Item, ItemData>();
        public static Dictionary<int, Item> networkedItems = new Dictionary<int, Item>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Client(string _ip, int _port)
        {
            ip = _ip;
            port = _port;

            if(instance!=null)
            {
                // Disconnect old instance
            }
            instance = this;
            // Create managers
            tcp = new TCP();
            udp = new UDP();

            // Will setup packet handlers
            InitializeClientData();
            // Connect the tcp manager
            tcp.Connect();
        }

        /// <summary>
        /// Will setup packet handlers
        /// </summary>
        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>
            {
            {(int)packetTypes.welcome, ClientHandler.handleWelcome },
            {(int)packetTypes.playerInfo, ClientHandler.handlePlayer },
            {(int)packetTypes.itemInfo, ClientHandler.handleItem },
            {(int)packetTypes.error, ClientHandler.handleError },
            {(int)packetTypes.disconnect, ClientHandler.handleDisconnect }
            };
            Debug.Log("Initialized packets.");
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {
            connected = false;
            tcp.socket.Close();
            udp.socket.Close();
            Debug.Log("Disconnected");
        }

        /// <summary>
        /// Tcp manager
        /// </summary>
        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            /// <summary>
            /// Connect to the server
            /// </summary>
            public void Connect()
            {
                // Setup socket
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };
                receiveBuffer = new byte[dataBufferSize];
                // Connect to the server
                IAsyncResult result = socket.BeginConnect(ip, port, new AsyncCallback(this.ConnectCallback), this.socket);
                // Begin timeout wait
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                // If timed out
                if(!success)
                {
                    // End connection
                    socket.EndConnect(result);
                    Debug.LogError("Could not connect to server, timed out");
                    return;
                }
            }

            /// <summary>
            /// Will disconnect the tcp from a server
            /// </summary>
            public void Disconnect()
            {
                Client.instance.Disconnect();

                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }

            /// <summary>
            /// Called when the server confirms connection
            /// </summary>
            private void ConnectCallback(IAsyncResult _result)
            {
                // Stop connecting
                socket.EndConnect(_result);
                // If the socket is already connected then stop
                if (!socket.Connected)
                {
                    return;
                }
                stream = socket.GetStream();
                receivedData = new Packet();
                // Begin listening
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, new AsyncCallback(ReceiveCallback), null);
            }

            /// <summary>
            /// If data is received through tdp
            /// </summary>
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int num = stream.EndRead(_result);
                    if (num <= 0)
                    {
                        Client.instance.Disconnect();
                        return;
                    }
                    byte[] array = new byte[num];
                    Array.Copy(receiveBuffer, array, num);
                    receivedData.Reset(HandleData(array));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, new AsyncCallback(ReceiveCallback), null);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to receive callback: " + e);
                    Client.instance.Disconnect();
                }
            }

            /// <summary>
            /// Send tcp data to server
            /// </summary>
            /// <param name="_packet"></param>
            public void SendData(Packet _packet)
            {
                try
                {
                    // If socket is connected
                    if (socket != null)
                    {
                        // Begin writing data and send
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    // If failed to send data
                    Debug.LogError($"Error sending data to server via TCP: {e}");
                }
            }

            /// <summary>
            /// Will handle received data
            /// Will send packet with data to correct packet handler
            /// </summary>
            /// <param name="_data">Data to handle</param>
            /// <returns></returns>
            private bool HandleData(byte[] _data)
            {
                int num = 0;
                receivedData.SetBytes(_data);
                if (receivedData.UnreadLength() >= 4)
                {
                    // Read the length
                    num = receivedData.ReadInt(true);
                    if (num <= 0)
                    {
                        // If length is 0
                        return true;
                    }
                }
                while (num > 0 && num <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(num, true);
                    ThreadManager.ExecuteOnMainThread(delegate
                    {
                        using (Packet packet = new Packet(_packetBytes))
                        {
                            int key = packet.ReadInt(true);
                            packetHandlers[key](packet);
                        }
                    });
                    num = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        num = receivedData.ReadInt(true);
                        if (num <= 0)
                        {
                            return true;
                        }
                    }
                }
                return num <= 1;
            }
        }

        /// <summary>
        /// Udp manager
        /// </summary>
        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                // Set endpoint to server
                endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            /// <summary>
            /// Connect to the server
            /// </summary>
            /// <param name="_localPort">Clients port to receive data</param>
            public void Connect(int _localPort)
            {
                socket = new UdpClient(_localPort);
                // Connect to the server
                socket.Connect(endPoint);
                // Route udp data receive
                socket.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
            
            /// <summary>
            /// If data is received via udp from the server
            /// </summary>
            /// <param name="_result"></param>
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    // Read data
                    byte[] array = this.socket.EndReceive(_result, ref this.endPoint);
                    this.socket.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
                    if (array.Length < 4)
                    {
                        // If the received data length is below a nibble
                        instance.Disconnect();
                        return;
                    }
                    // Begin handling data
                    HandleData(array);
                }
                catch (Exception e)
                {
                    // If data is failed to receive
                    Debug.LogError("Failed to receive data with udp, " + e);
                    Disconnect();
                }
            }

            /// <summary>
            /// Read data from server
            /// </summary>
            /// <param name="_data">Data received</param>
            private void HandleData(byte[] _data)
            {
                using (Packet packet = new Packet(_data))
                {
                    // Read length of data
                    int length = packet.ReadInt(true);
                    // Read rest of data
                    _data = packet.ReadBytes(length, true);
                }
                // Run packet handler on main thread
                ThreadManager.ExecuteOnMainThread(delegate
                {
                    using (Packet packet2 = new Packet(_data))
                    {
                        int key = packet2.ReadInt(true);
                        if (packetHandlers.ContainsKey(key))
                        {
                            packetHandlers[key](packet2);
                        }
                        else
                        {
                            Debug.LogError("Could not find packet handler for key " + key);
                        }
                    }
                });
            }

            /// <summary>
            /// Send data with udp to the server
            /// </summary>
            /// <param name="_packet">Packet to send</param>
            public void SendData(Packet _packet)
            {
                try
                {
                    // Send the clients id
                    _packet.InsertInt(myId);
                    if (socket != null)
                    {
                        // Send the data
                        this.socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    // If failed
                    Debug.LogError($"Error sending data to server via UDP: {e}");
                }
            }

            /// <summary>
            /// Disconnects udp and disconnects the client
            /// </summary>
            private void Disconnect()
            {
                Client.instance.Disconnect();

                instance.Disconnect();
                endPoint = null;
                socket = null;
            }
        }
    }
}
