﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Multiplayer_Mod.Client
{
	class ClientSender
	{
		/// <summary>
		/// Send a packet via tcp to the server
		/// </summary>
		/// <param name="_packet">Packet to send</param>
		public static void SendTCPData(Packet _packet)
		{
			_packet.WriteLength();
			Client.tcp.SendData(_packet);
		}

		/// <summary>
		/// Send a packet via udp to the server
		/// </summary>
		/// <param name="_packet">Packet to send</param>
		public static void SendUDPData(Packet _packet)
		{
			_packet.WriteLength();
			Client.udp.SendData(_packet);
		}

		public static void sendPlayerData(PlayerData data)
		{
			Debug.Log("Sending player data");	
			using (Packet packet = new Packet((int)packetTypes.playerInfo))
			{
				packet.Write(data);
				SendUDPData(packet);
			}
		}
	}
}