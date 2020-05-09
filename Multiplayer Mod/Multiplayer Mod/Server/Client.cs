using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Debug = UnityEngine.Debug;

namespace Multiplayer_Mod.Server
{
    /// <summary>
    /// Manages the client serverside
    /// </summary>
    public class Client
    {
        // The size of every packet
        public static int dataBufferSize = 4096;

        // The clients id
        public int id;
        // The players tcp manager
        public TCP tcp;
        // The players udp manager
        public UDP udp;

        /// <summary>
        /// Constructs the client
        /// </summary>
        /// <param name="_clientId">The clients id</param>
        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            // Clients tcp id
            private readonly int id;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="_id">The clients tcp id</param>
            public TCP(int _id)
            {
                id = _id;
            }

            /// <summary>
            /// Connects the tcp 
            /// </summary>
            /// <param name="_socket"></param>
            public void Connect(TcpClient _socket)
            {
                // Sets variables of the socket
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                // Begins listening
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            /// <summary>
            /// Will send a packet over tcp to the client
            /// </summary>
            /// <param name="_packet">The packet to send</param>
            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        // Writes the packet
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    // Only ran if there was an exception
                    Debug.Log($"Error sending data to player {id} via TCP: {e}");
                }
            }

            /// <summary>
            /// Ran when tcp data is received from the client
            /// </summary>
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    // Length of the data
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        // Handle disconnect
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    // Disconnect
                    Debug.Log($"Error receiving TCP data: {_ex}");
                }
            }

            /// <summary>
            /// Will handle received data
            /// </summary>
            /// <param name="_data">Data to handle</param>
            /// <returns></returns>
            private bool HandleData(byte[] _data)
            {
                // Initialises the packet length variable
                int _packetLength = 0;

                receivedData.SetBytes(_data);
                // If length - read position is above or equal to 4
                if (receivedData.UnreadLength() >= 4)
                {
                    // Get packet length
                    _packetLength = receivedData.ReadInt();
                    // If packet is empty
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                // Begin reading the packet
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public class UDP
        {
            // Udp end point
            public IPEndPoint endPoint;

            // Clients udp id
            private int id;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="_id">The clients udp id</param>
            public UDP(int _id)
            {
                id = _id;
            }

            /// <summary>
            /// Sets the end point of all the udp data
            /// </summary>
            /// <param name="_endPoint">Where to send data to</param>
            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            /// <summary>
            /// Sends a packet to the client
            /// </summary>
            /// <param name="_packet">Packet to send</param>
            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            /// <summary>
            /// Handles a received packet
            /// </summary>
            /// <param name="_packetData">Packet received</param>
            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }
        }
    }
}
