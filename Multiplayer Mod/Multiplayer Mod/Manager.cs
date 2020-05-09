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
        /// Will connect the client to a server
        /// </summary>
        /// <param name="ip">The ip address to connect to</param>
        /// <param name="port">The port to connect to</param>
        public static void connectClient(string ip, int port)
        {

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
