using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer_Mod.Server
{
    class ServerHandler
    {
        /// <summary>
        /// Will handle player information from a client
        /// </summary>
        /// <param name="_fromClient">Client id the packet came from</param>
        /// <param name="_packet">The packet received</param>
        public static void handlePlayerInfo(int _fromClient, Packet _packet)
        {
            // Implement later
        }

        /// <summary>
        /// Confirms the client has connected
        /// </summary>
        /// <param name="_fromClient">Client id the packet came from</param>
        /// <param name="_packet">The packet received</param>
        public static void welcome(int _fromClient, Packet _packet)
        {
            Debug.Log(_fromClient + " sent welcome back");
        }
    }
}
