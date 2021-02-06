using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Debug = UnityEngine.Debug;

namespace BSMP
{
    /// <summary>
    /// Used to manage receiving and sending data for a specific client
    /// </summary>
    internal class ServerClient
    {
        /// <summary>
        /// Buffer size for packets
        /// </summary>
        public static int dataBufferSize = 4096;

        /// <summary>
        /// Id of the client
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// TCP handler for the client
        /// </summary>
        public TCP tcp { get; private set; }
        /// <summary>
        /// UDP handler for the client
        /// </summary>
        public UDP udp { get; private set; }

        /// <summary>
        /// Initialize client
        /// </summary>
        /// <param name="id">Id to give the client</param>
        public ServerClient(int id)
        {
            this.id = id;
            this.tcp = new TCP(this);
            this.udp = new UDP(this);
        }

        /// <summary>
        /// Disconnects handlers from client
        /// 
        /// </summary>
        public void Disconnect()
        {
            tcp.Disconnect();
            udp.Disconnect();
        }

        /// <summary>
        /// Handles TCP side
        /// </summary>
        public class TCP
        {
            /// <summary>
            /// Corresponding client
            /// </summary>
            public ServerClient client { get; private set; }
            /// <summary>
            /// Used to send and receive data between client
            /// </summary>
            public NetworkStream stream { get; private set; }
            /// <summary>
            /// Wrapper for sockets and stream, used to close on disconnect
            /// </summary>
            public TcpClient socket { get; private set; }

            /// <summary>
            /// Used for storing received TCP data
            /// </summary>
            private Packet receivedData;
            /// <summary>
            /// Used for receiving data over network stream, temp storage before data is casted to packet
            /// </summary>
            private byte[] receiveBuffer;

            public TCP(ServerClient client)
            {
                this.client = client;
            }

            /// <summary>
            /// Setup variables for TCP use and begin listening for data from the client
            /// </summary>
            /// <param name="socket">TcpClient to setup</param>
            public void Connect(TcpClient tcpClient)
            {
                stream = tcpClient.GetStream();
                socket = tcpClient;

                tcpClient.Client.ReceiveBufferSize = dataBufferSize;
                tcpClient.Client.SendBufferSize = dataBufferSize;

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, Receive, null);
            }

            /// <summary>
            /// Disconnects handler from client
            /// </summary>
            public void Disconnect()
            {
                socket.Close();
                socket.Dispose();
                stream = null;
                socket = null;
                receiveBuffer = null;
                receivedData = null;
            }

            /// <summary>
            /// Callback for receiving TCP data from the client
            /// </summary>
            /// <param name="result"></param>
            public void Receive(IAsyncResult result)
            {

            }

            /// <summary>
            /// Sends TCP data to the client
            /// </summary>
            /// <param name="packet">The packet to be sent</param>
            public void SendData(Packet packet)
            {
                try
                {
                    if (stream != null)
                    {
                        // Writes the packet
                        Console.WriteLine($"Sent tcp data to player {client.id}");
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    // Only ran if there was an exception
                    Console.WriteLine($"Error sending data to player {client.id} via TCP: {e}");
                }
            }
        }

        public class UDP
        {
            /// <summary>
            /// Corresponding client
            /// </summary>
            public ServerClient client { get; private set; }
            /// <summary>
            /// Endpoint to send data to
            /// </summary>
            public IPEndPoint ip { get; private set; }

            public UDP(ServerClient client)
            {
                this.client = client;
            }

            /// <summary>
            /// Setup endpoint for sending and receiving data
            /// </summary>
            /// <param name="ip"></param>
            public void Connect(IPEndPoint ip)
            {
                this.ip = ip;
            }

            /// <summary>
            /// Disconnects the handler from the client
            /// </summary>
            public void Disconnect()
            {
                ip = null;
            }

            /// <summary>
            /// Sends UDP data to the client
            /// </summary>
            /// <param name="packet">Packet to be sent</param>
            public void SendPacket(Packet packet)
            {
                try
                {
                    // If the client exists
                    if (ip != null)
                    {
                        // Send data to the client
                        packet.InsertInt(client.id, 3);
                        Server.udpClient.BeginSend(packet.ToArray(), packet.Length(), ip, null, null);
                    }
                }
                catch (Exception e)
                {
                    // Will only run if above failed
                    Server.Message($"Error sending data to {ip} via UDP: {e}");
                }
            }
        }
    }
}
