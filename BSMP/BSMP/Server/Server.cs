using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;

namespace BSMP
{
    /// <summary>
    /// Main serverside manager, handles the entire backend of the server
    /// </summary>
    public static class Server
    {
        /// <summary>
        /// Maximum amount of players to allow on the server
        /// </summary>
        public static int maxPlayers { get; private set; }
        /// <summary>
        /// Port to listen to for data
        /// </summary>
        public static int port { get; private set; }
        /// <summary>
        /// How many server ticks per second
        /// </summary>
        public static int tps = 90;

        /// <summary>
        /// Used to receive TCP connections from clients
        /// </summary>
        public static TcpListener tcpListener { get; private set; }
        /// <summary>
        /// Used to receive and send UDP data between clients
        /// </summary>
        public static UdpClient udpClient { get; private set; }

        /// <summary>
        /// Server clients and their corresponding ids
        /// Setup like this to find clients by their ids without a special function aswell
        /// </summary>
        internal static Dictionary<int, ServerClient> clients;

        /// <summary>
        /// Dictionary for packet ids and their corresponding packet handlers
        /// Used to find out what method to use when handling a certain type of packet
        /// </summary>
        private static Dictionary<int, PacketHandler> packetHandlers;
        private delegate void PacketHandler(int clientID, Packet packet);

        /// <summary>
        /// Used to determine if the server is running or not
        /// </summary>
        /// <returns>[Boolean] If server is running</returns>
        public static bool ServerRunning()
        {
            if (tcpListener != null && udpClient != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// This is so when I switch over to B&S I am able to easily switch messaging systems
        /// </summary>
        public static void Message(string message)
        {
            Console.WriteLine(message);
            UI.Message(message);
        }

        /// <summary>
        /// Starts the server and begins listening to a certain port via TCP and UDP
        /// </summary>
        /// <param name="maxPlayers">Maximum amount of players to allow</param>
        /// <param name="port">Port to listen to</param>
        public static void Start(int maxPlayers, int port)
        {
            try
            {
                Server.maxPlayers = maxPlayers;
                Server.port = port;

                Message($"Starting server at {port}, allowing for {maxPlayers} players");

                Message("Initializing client list and handlers");
                clients = new Dictionary<int, ServerClient>();
                for (int i = 1; i <= maxPlayers; i++)
                {
                    clients.Add(i, new ServerClient(i));
                }

                tcpListener = new TcpListener(IPAddress.Any, port);
                udpClient = new UdpClient(port);

                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(TCPConnect, null);
                Message("Listening on TCP");

                udpClient.BeginReceive(UDPReceive, null);
                Message("Listening on UDP");

                Message("Starting main thread");
                Thread mainThread = new Thread(MainThread);
                mainThread.Start();
                Message("Main server thread started");

                Message("Server started");
            }
            catch (Exception e)
            {
                Message("Error starting server");
                Console.WriteLine(e);
                Shutdown();
            }
        }

        /// <summary>
        /// Shuts the server down if running
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                if (tcpListener != null || udpClient != null)
                {
                    Message("Shutting server down");
                    if (tcpListener != null)
                    {
                        tcpListener.Stop();
                        tcpListener = null;
                    }
                    if (udpClient != null)
                    {
                        udpClient.Close();
                        udpClient.Dispose();
                        udpClient = null;
                    }
                    clients = null;
                    Message("Server shutdown");
                }
                else
                {
                    Message("Attempted to shutdown server but no server is running");
                }
            }
            catch (Exception e)
            {
                Message("Error shutting server down");
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Manages what the server does constantly
        /// Eg. Sending data to clients every tick
        /// Runs at specified tps
        /// </summary>
        private static void MainThread()
        {
            while (ServerRunning())
            {
                try
                {

                }
                catch (Exception e)
                {
                    Message("Error during main thread");
                    Console.WriteLine(e);
                }
                Thread.Sleep(1000 / tps);
            }
        }

        /// <summary>
        /// Callback for when a TCP connection is received
        /// </summary>
        /// <param name="result">References asyncronous creation of TcpClient</param>
        private static void TCPConnect(IAsyncResult result)
        {
            try
            {
                TcpClient client = tcpListener.EndAcceptTcpClient(result);
            }
            catch (Exception e)
            {
                Message("Error during connection of TCP Client");
                Console.WriteLine(e);
            }
            tcpListener.BeginAcceptTcpClient(TCPConnect, null);
        }

        /// <summary>
        /// Callback for when UDP data is received
        /// </summary>
        /// <param name="result">References asynchronous receive</param>
        private static void UDPReceive(IAsyncResult result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpClient.EndReceive(result, ref _clientEndPoint);
            }
            catch (Exception e)
            {
                Message("Error during receiving of UDP data");
                Console.WriteLine(e);
            }
            udpClient.BeginReceive(UDPReceive, null);
        }
    }
}
