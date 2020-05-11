using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

using Multiplayer_Mod.DataHolders;
using ItemData = Multiplayer_Mod.DataHolders.ItemData;
using System.Runtime.CompilerServices;

namespace Multiplayer_Mod.Server
{
    /// <summary>
    /// Manages the server
    /// </summary>
    public class Server
    {
        // Maximum amount of players
        public static int MaxPlayers { get; private set; }
        // Port the server is on
        public static int Port { get; private set; }

        // Clients and their ids
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        // Player data and their ids
        public static Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
        // Item data and their ids
        public static Dictionary<int, Dictionary<int, ItemData>> items = new Dictionary<int, Dictionary<int, ItemData>>();
        public static int networkIds;
        // Packet handlers and the id of what packet type they are handing
        public static Dictionary<int, PacketHandler> packetHandlers;
        // How packet handling methods should be setup
        public delegate void PacketHandler(int _fromClient, Packet _packet);

        // The current tcp listener
        private static TcpListener tcpListener;
        // The current udp listener
        private static UdpClient udpListener;

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="maxPlayers">Maximum amount of players at once</param>
        /// <param name="port">The port the server is started on</param>
        public static void Start(int maxPlayers, int port)
        {
            //Sets variables
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting server...");
            // Will add temporary clients to the client dictionary and setup packet handlers
            InitializeServerData();

            // Creates the tcp listener and sets it up to route connections to TCPConnectCallback
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            // Creates the udp listener and sets it up to route messages to UDPReceiveCallback
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {IPAddress.Any} and {port}");
        }

        /// <summary>
        /// Will create temporary clients to fill the client slots
        /// Will initialise the packet handlers
        /// Will connect the host to their server
        /// </summary>
        private static void InitializeServerData()
        {
            // Fills clients dictionary with temporary clients to be assigned
            for (int i = 1; i <= MaxPlayers + 10; i++)
            {
                clients.Add(i, new Client(i));
            }
            // Initialises the packet handlers
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)packetTypes.welcome, ServerHandler.welcome },
                { (int)packetTypes.playerInfo, ServerHandler.handlePlayerInfo },
                { (int)packetTypes.itemInfo, ServerHandler.handleItemInfo }
            };
            Console.WriteLine("Initialized packets.");
        }

        /// <summary>
        /// Called when a client connects to the tcp listener
        /// Will send the client their id via the welcome message if they successfully connect
        /// </summary>
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            // Begins connection with the client
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            // Will search for a slot and assign the connectee to an available slot
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    clients[i].timeout = DateTime.Now.AddSeconds(10);
                    ServerSender.SendWelcome(i);
                    return;
                }
            }
            // Only reaches this point if the server is full
            for (int i = MaxPlayers + 1; i <= MaxPlayers + 10; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    ServerSender.SendError(i, "Failed to connect, server is full");
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        /// <summary>
        /// Will intialise a udp connection if needed
        /// Will handle udp data received
        /// </summary>
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4) return;

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0) return;

                    // If client udp is not connected then connect
                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].timeout = DateTime.Now.AddSeconds(10);
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].timeout = DateTime.Now.AddSeconds(10);
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error receiving UDP data: {e}");
            }
        }

        /// <summary>
        /// Will send data over udp to a client
        /// </summary>
        /// <param name="_clientEndPoint">Client to send to</param>
        /// <param name="_packet">The packet to send</param>
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                // If the client exists
                if (_clientEndPoint != null)
                {
                    // Send data to the client
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception e)
            {
                // Will only run if above failed
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {e}");
            }
        }
    }
}
