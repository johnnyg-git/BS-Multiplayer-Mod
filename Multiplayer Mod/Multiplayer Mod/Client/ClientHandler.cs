using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using BS;
using Multiplayer_Mod.DataHolders;
using ItemData = Multiplayer_Mod.DataHolders.ItemData;

namespace Multiplayer_Mod.Client
{
    class ClientHandler
    {
        public static List<string> ignoredItems = new List<string>();

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

        public static void handleItem(Packet _packet)
        {
            ItemData data = _packet.ReadItemData();
            if (data.playerControl != Client.myId)
            {
                if (ignoredItems.Contains(data.itemId)) return;
                if (Client.items.ContainsKey(data.networkId) && Client.items[data.networkId].clientsideItem != null)
                {
                    if(Client.items[data.networkId].clientsideItem.data.id!=data.itemId)
                    {
                        Client.items[data.networkId].clientsideItem.Despawn();
                        return;
                    }
                    Client.items[data.networkId].clientsideItem.transform.position = data.objectData.position;
                    Client.items[data.networkId].clientsideItem.transform.rotation = data.objectData.rotation;
                    Client.items[data.networkId].clientsideItem.rb.velocity = data.objectData.velocity;
                    Client.items[data.networkId].clientsideItem.rb.angularVelocity = data.objectData.angularVelocity;
                    Client.items[data.networkId].clientsideItem.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    Client.items[data.networkId].toDelete = DateTime.Now.AddMilliseconds(100);
                }
                else
                {
                    if(Catalog.current.GetData<BS.ItemData>(data.itemId)!=null)
                    {
                        data.clientsideItem = Catalog.current.GetData<BS.ItemData>(data.itemId).Spawn();
                        data.clientsideItem.data.forceLayer = LayerName.MovingObject;
                        data.clientsideItem.SetColliderAndMeshLayer(VRManager.GetLayer(LayerName.MovingObject));
                        data.clientsideItem.data.moduleAI = null;
                        data.clientsideItem.transform.position = data.objectData.position;
                        data.clientsideItem.transform.rotation = data.objectData.rotation;
                        data.clientsideItem.rb.velocity = data.objectData.velocity;
                        data.clientsideItem.rb.angularVelocity = data.objectData.angularVelocity;
                        data.toDelete = DateTime.Now.AddMilliseconds(100);
                        Client.items[data.networkId] = data;
                        Client.networkedItems[data.networkId] = data.clientsideItem;
                    }
                    else
                    {
                        Debug.Log($"{data.itemId} failed to spawn, adding to ignored items list");
                        ignoredItems.Add(data.itemId);
                    }
                }
            }
        }

        /// <summary>
        /// Will handle player information sent by the server
        /// </summary>
        /// <param name="_packet"></param>
        public static void handlePlayer(Packet _packet)
        {
            PlayerData player = _packet.ReadPlayerData();
            if (Client.players.ContainsKey(player.id) && Client.players[player.id].leftHand.transform != null)
            {
                Client.players[player.id].leftHand.transform.position = player.leftHand.position;
                player.leftHand = Client.players[player.id].leftHand;
            }
            else
            {
                player.leftHand.transform = Catalog.current.GetData<BS.ItemData>("MultiplayerHand").Spawn().transform;
                if (player.id == Client.myId)
                    GameObject.Destroy(player.leftHand.transform.GetComponent<Collider>());
            }
            if (Client.players.ContainsKey(player.id) && Client.players[player.id].rightHand.transform != null)
            {
                Client.players[player.id].rightHand.transform.position = player.rightHand.position;
                player.rightHand = Client.players[player.id].rightHand;
            }
            else
            {
                player.rightHand.transform = Catalog.current.GetData<BS.ItemData>("MultiplayerHand").Spawn().transform;
                if (player.id == Client.myId)
                    GameObject.Destroy(player.rightHand.transform.GetComponent<Collider>());
            }
            if (player.id != Client.myId)
            {
                if (Client.players.ContainsKey(player.id) && Client.players[player.id].head.transform != null)
                {
                    Client.players[player.id].head.transform.position = player.head.position;
                    Client.players[player.id].head.transform.rotation = player.head.rotation;
                    player.head = Client.players[player.id].head;
                }
                else
                {
                    player.head.transform = Catalog.current.GetData<BS.ItemData>("MultiplayerHead").Spawn().transform;
                }
            }
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

        /// <summary>
        /// Ran when an error is received from the server
        /// </summary>
        /// <param name="_packet"></param>
        public static void handleError(Packet _packet)
        {
            string error = _packet.ReadString();
            Debug.LogError($"Error received from server: {error}");
        }
    }
}
