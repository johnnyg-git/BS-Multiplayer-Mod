using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
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
            PlayerData data = _packet.ReadPlayerData();
            Server.players[_fromClient] = data;
            Debug.Log($"Received PlayerData from {_fromClient}\nLeftHand pos: {data.leftHand.position}\nRightHandPos: {data.rightHand.position}\nHeadPos: {data.head.position}");
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
