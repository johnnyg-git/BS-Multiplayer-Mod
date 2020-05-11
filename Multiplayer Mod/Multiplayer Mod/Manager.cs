using BS;
using Multiplayer_Mod.Client;
using Multiplayer_Mod.DataHolders;
using Multiplayer_Mod.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ItemData = Multiplayer_Mod.DataHolders.ItemData;

namespace Multiplayer_Mod
{
    /// <summary>
    /// Runs and handles pretty much everything
    /// </summary>
    class Manager : MonoBehaviour
    {
        // Current manager
        public static Manager instance;
        // Own player data
        public static PlayerData player;
        // Is a server running on the local host
        public static bool serverRunning;

        /// <summary>
        /// Ran when created
        /// Will ensure there is only one manager instance at once
        /// </summary>
        void Awake()
        {
            if (instance != null) Destroy(this);
            else
            {
                instance = this;
                gameObject.AddComponent<GUIManager>();
            }
        }

        /// <summary>
        /// Ran when the game is closed
        /// Will begin disconnecting
        /// </summary>
        void OnApplicationQuit()
        {
            disconnectClient();
        }

        /// <summary>
        /// Will handle received data and send out data on clientside and serverside if server is running
        /// </summary>
        void Update()
        {
            ThreadManager.UpdateMain();
            if(Client.Client.connected && Client.Client.myId!=0)
            {
                // Client is connected to a server
                if (player == null) player = new PlayerData(Client.Client.myId);
                if (Player.local != null && Player.local.body != null) {
                    player.leftHand.position = Player.local.body.handLeft.transform.position;
                    player.rightHand.position = Player.local.body.handRight.transform.position;
                    player.head.position = Player.local.head.transform.position;
                    player.head.rotation = Player.local.head.transform.rotation;
                    ClientSender.sendPlayerData(player);
                }

                foreach(ItemData item in Client.Client.items.Values)
                {
                    if(DateTime.Now > item.toDelete)
                    {
                        ThreadManager.ExecuteOnMainThread(() =>
                        {
                            Client.Client.items.Remove(item.networkId);
                            Client.Client.networkedItems.Remove(item.networkId);
                        });
                        item.clientsideItem.Despawn();
                    }
                }

                if(serverRunning)
                {
                    int i = 0;
                    foreach(Item item in Item.list)
                    {
                        if (item.data.category != BS.ItemData.Category.Prop)
                        {
                            i++;
                            if (!Client.Client.networkedItems.ContainsValue(item) && item.data.category != BS.ItemData.Category.Body && !item.data.id.Contains("Multiplayer") && item.data.prefab != null)
                            {
                                if (!Server.Server.items.ContainsKey(Client.Client.myId))
                                {
                                    Server.Server.items[Client.Client.myId] = new Dictionary<int, ItemData>();
                                }

                                if (Client.Client.sendingItems.ContainsKey(item))
                                {
                                    Client.Client.sendingItems[item].objectData.position = item.transform.position;
                                    Client.Client.sendingItems[item].objectData.rotation = item.transform.rotation;
                                    Client.Client.sendingItems[item].objectData.velocity = item.rb.velocity;
                                    Client.Client.sendingItems[item].objectData.angularVelocity = item.rb.angularVelocity;
                                    Server.Server.items[Client.Client.myId][i] = Client.Client.sendingItems[item];
                                }
                                else
                                {
                                    Server.Server.networkIds++;
                                    Client.Client.sendingItems[item] = new ItemData(Server.Server.networkIds, item.data.id) { playerControl = Client.Client.myId };
                                    Server.Server.items[Client.Client.myId][i] = Client.Client.sendingItems[item];
                                }
                            }
                        }
                        else
                            item.Despawn();
                    }
                }
                else
                {
                    int i = 0;
                    foreach (Item item in Item.list)
                    {
                        if (item.data.category != BS.ItemData.Category.Prop)
                        {
                            i++;
                            if (!Client.Client.networkedItems.ContainsValue(item) && item.data.category != BS.ItemData.Category.Body && !item.data.id.Contains("Multiplayer") && item.data.prefab != null)
                            {
                                if (Client.Client.sendingItems.ContainsKey(item))
                                {
                                    Client.Client.sendingItems[item].objectData.position = item.transform.position;
                                    Client.Client.sendingItems[item].objectData.rotation = item.transform.rotation;
                                    Client.Client.sendingItems[item].objectData.velocity = item.rb.velocity;
                                    Client.Client.sendingItems[item].objectData.angularVelocity = item.rb.angularVelocity;

                                    ClientSender.sendItemData(Client.Client.sendingItems[item]);
                                }
                                else
                                {
                                    Client.Client.sendingItems[item] = new ItemData(0, item.data.id) { playerControl = Client.Client.myId, clientsideId = i };
                                    ClientSender.sendItemData(Client.Client.sendingItems[item]);
                                }
                            }
                        }
                        else
                            item.Despawn();
                    }
                }
            }
            if(serverRunning)
            {
                ServerSender.SendPlayerData();
                ServerSender.SendItemData();
            }
        }

        /// <summary>
        /// Will connect the client to a server
        /// </summary>
        /// <param name="ip">The ip address to connect to</param>
        /// <param name="port">The port to connect to</param>
        public static void connectClient(string ip, int port)
        {
            Debug.Log("Connecting client to " + ip + ":" + port);
            new Client.Client(ip, port);
        }

        /// <summary>
        /// Disconnects the client
        /// </summary>
        public static void disconnectClient()
        {
            if (Client.Client.connected)
            {
                Client.Client.instance.Disconnect();
            }
        }

        /// <summary>
        /// Will start a server on the local host
        /// </summary>
        /// <param name="maxPlayers">Maximum amount of players that can join at once</param>
        /// <param name="port">The port to open the server on</param>
        public static void startServer(int maxPlayers, int port)
        {
            Server.Server.Start(maxPlayers, port);
        }
    }
}
