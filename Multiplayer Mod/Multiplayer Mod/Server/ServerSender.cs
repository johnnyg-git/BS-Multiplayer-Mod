using BS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

namespace Multiplayer_Mod.Server
{
    class ServerSender
    {
        #region setup
        /// <summary>
        /// Sends a packet over tcp to a specific client
        /// </summary>
        /// <param name="_toClient">The client to receive the data</param>
        /// <param name="_packet">The packet to send</param>
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        /// <summary>
        /// Sends a packet over udp to a specific client
        /// </summary>
        /// <param name="_toClient">The client to receive the data</param>
        /// <param name="_packet">The packet to send</param>
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>
        /// Sends data over tcp to all clients
        /// </summary>
        /// <param name="_packet">The packet to send</param>
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            foreach (Client client in Server.clients.Values)
            {
                client.tcp.SendData(_packet);
            }
        }

        /// <summary>
        /// Sends a packet over tcp to all clients except certain ones
        /// </summary>
        /// <param name="_exceptClient">The clients to not send to</param>
        /// <param name="_packet">The packet to send</param>
        private static void SendTCPDataToAllE(int[] _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            foreach(Client client in Server.clients.Values)
            {
                if (!_exceptClient.Contains<int>(client.id))
                {
                    client.tcp.SendData(_packet);
                }
            }
        }

        /// <summary>
        /// Sends a packet over udp to all clients
        /// </summary>
        /// <param name="_packet">The packet to send</param>
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            foreach (Client client in Server.clients.Values)
            {
                client.udp.SendData(_packet);
            }
        }

        /// <summary>
        /// Sends a packet over udp to all clients except certain ones
        /// </summary>
        /// <param name="_exceptClient">The clients to not send to</param>
        /// <param name="_packet">The packet to send</param>
        private static void SendUDPDataToAllE(int[] _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            foreach (Client client in Server.clients.Values)
            {
                if (!_exceptClient.Contains<int>(client.id))
                {
                    client.udp.SendData(_packet);
                }
            }
        }
        #endregion

        /// <summary>
        /// Sends the welcome message to a client over tcp
        /// Message contains the clients id
        /// </summary>
        /// <param name="_client">The id of the client to send to</param>
        public static void SendWelcome(int _client)
        {
            Debug.Log("Sending welcome message to: " + _client);
            using (Packet _packet = new Packet((int)packetTypes.welcome))
            {
                _packet.Write(_client);
                SendTCPData(_client, _packet);
            }
        }

        /// <summary>
        /// Sends all the stored player data to every player
        /// </summary>
        public static void SendPlayerData()
        {
            foreach (PlayerData player in Server.players.Values)
            {
                using (Packet _packet = new Packet((int)packetTypes.playerInfo))
                {
                    _packet.Write(player);
                    SendUDPDataToAllE(new int[] {}, _packet);
                }
            }
        }

        /// <summary>
        /// Will send disconnect information to every client to handle
        /// </summary>
        /// <param name="_disconectee">The players id who is disconnecting</param>
        public static void SendDisconnect(int _disconectee)
        {
            Debug.Log("Sending disconnect message from " + _disconectee);
            using (Packet _packet = new Packet((int)packetTypes.disconnect))
            {
                _packet.Write(_disconectee);
                SendTCPDataToAll(_packet);
            }
        }

        /// <summary>
        /// Will send an error message to a client
        /// </summary>
        /// <param name="_client">The id of the client to send to</param>
        /// <param name="errorMessage">The message to send</param>
        public static void SendError(int _client, string errorMessage)
        {
            Debug.Log("Sending error message to: " + _client);
            using (Packet _packet = new Packet((int)packetTypes.error))
            {
                _packet.Write(errorMessage);
                SendTCPData(_client, _packet);
            }
        }
    }
}
