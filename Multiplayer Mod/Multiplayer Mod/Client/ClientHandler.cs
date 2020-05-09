using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using BS;

namespace Multiplayer_Mod.Client
{
    class ClientHandler
    {
        /// <summary>
        /// Will read the packet and get the clients id
        /// Will connect the clients udp to the server
        /// </summary>
        /// <param name="_packet"></param>
        public static void handleWelcome(Packet _packet)
        {
            Client.myId = _packet.ReadInt();
            Client.connected = true;
            Debug.Log("Successfully connected to server");
            Debug.Log("Client has been given id: " + Client.myId);
            Client.udp.Connect(((IPEndPoint)Client.tcp.socket.Client.LocalEndPoint).Port);
            using (Packet packet = new Packet((int)packetTypes.welcome))
            {
                packet.Write(Client.myId);
                packet.Write(Client.myId.ToString());
                ClientSender.SendTCPData(packet);
            }
        }

        public static void handlePlayer(Packet _packet)
        {
            // Handle player info
        }

        public static void handleDisconnect(Packet _packet)
        {
            int disconectee = _packet.ReadInt();
            if(Client.players.ContainsKey(disconectee))
            {
                PlayerData player = Client.players[disconectee];
                if(player.leftHand.transform!=null)
                {
                    GameObject.Destroy(player.leftHand.transform);
                }
                if (player.rightHand.transform != null)
                {
                    GameObject.Destroy(player.rightHand.transform);
                }
                if (player.head.transform != null)
                {
                    GameObject.Destroy(player.head.transform);
                }
                Client.players[disconectee] = null;
            }
        }

        public static void handleError(Packet _packet)
        {
            string error = _packet.ReadString();
            Debug.LogError($"Error received from server: {error}");
        }
    }
}
