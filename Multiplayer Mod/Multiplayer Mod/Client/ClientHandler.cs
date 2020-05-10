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

        /// <summary>
        /// Will handle player information sent by the server
        /// </summary>
        /// <param name="_packet"></param>
        public static void handlePlayer(Packet _packet)
        {
            PlayerData player = _packet.ReadPlayerData();
            if(Client.players.ContainsKey(player.id) && Client.players[player.id].leftHand.transform !=null)
            {
                Client.players[player.id].leftHand.position = player.leftHand.position;
                player.leftHand = Client.players[player.id].leftHand;
            }
            else
            {
                player.leftHand.transform = Catalog.current.GetData<ItemData>("MultiplayerHand").Spawn().transform;
                if(player.id==Client.myId)
                    GameObject.Destroy(player.leftHand.transform.GetComponent<Collider>());
            }
            if (Client.players.ContainsKey(player.id) && Client.players[player.id].rightHand.transform != null)
            {
                Client.players[player.id].rightHand.transform.position = player.rightHand.position;
                player.rightHand = Client.players[player.id].rightHand;
                if (player.id == Client.myId)
                    GameObject.Destroy(player.rightHand.transform.GetComponent<Collider>());
            }
            else
            {
                player.rightHand.transform = Catalog.current.GetData<ItemData>("MultiplayerHand").Spawn().transform;
            }
            if (player.id != Client.myId)
            {
                Debug.Log($"Their id {player.id} our id {Client.myId}");
                if (Client.players.ContainsKey(player.id) && Client.players[player.id].head.transform != null)
                {
                    Client.players[player.id].head.transform.position = player.head.position;
                    player.head = Client.players[player.id].head;
                }
                else
                {
                    player.head.transform = Catalog.current.GetData<ItemData>("MultiplayerHand").Spawn().transform;
                    player.head.transform.localScale *= 2;
                }
            }
            Debug.Log("Handled player data\nLeftHandPos: " + player.leftHand.position);
            Client.players[player.id] = player;
        }

        /// <summary>
        /// Will handle everything when a player disconnects from the server
        /// </summary>
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
