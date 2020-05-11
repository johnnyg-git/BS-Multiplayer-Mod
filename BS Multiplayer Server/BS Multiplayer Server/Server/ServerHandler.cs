using Multiplayer_Mod.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Numerics;
using ItemData = Multiplayer_Mod.DataHolders.ItemData;

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
        }

        /// <summary>
        /// Will handle item information from a client
        /// </summary>
        /// <param name="_fromClient">Client id the packet came from</param>
        /// <param name="_packet">The packet received</param>
        public static void handleItemInfo(int _fromClient, Packet _packet)
        {
            ItemData data = _packet.ReadItemData();
            if (!Server.items.ContainsKey(_fromClient))
                Server.items[_fromClient] = new Dictionary<int, ItemData>();

            if (Server.items[_fromClient].ContainsKey(data.clientsideId))
                data.networkId = Server.items[_fromClient][data.clientsideId].networkId;
            else
            {
                data.networkId = Server.networkIds;
                Server.networkIds++;
            }

            Server.items[_fromClient][data.clientsideId] = data;
        }

        /// <summary>
        /// Confirms the client has connected
        /// </summary>
        /// <param name="_fromClient">Client id the packet came from</param>
        /// <param name="_packet">The packet received</param>
        public static void welcome(int _fromClient, Packet _packet)
        {
            Console.WriteLine(_fromClient + " sent welcome back");
        }
    }
}
