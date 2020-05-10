using BS;
using Multiplayer_Mod.Client;
using Multiplayer_Mod.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                    ClientSender.sendPlayerData(player);
                }
            }
            if(serverRunning)
            {
                ServerSender.SendPlayerData();
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
