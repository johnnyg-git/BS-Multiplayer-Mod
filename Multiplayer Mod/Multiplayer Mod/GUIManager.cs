using Multiplayer_Mod.Client;
using Multiplayer_Mod.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Threading;
using Ping = System.Net.NetworkInformation.Ping;

namespace Multiplayer_Mod
{
    public class GUIManager : MonoBehaviour
    {
        public string ip = "127.0.0.1";
        public string maxPlayers = "25";
        public string port = "26950";
        public int menu = 0;

        public long ping;

        void pingCallback(object sender, PingCompletedEventArgs e)
        {
            if (e==null || e.Cancelled || e.Error!=null || e.Reply==null) return;
            ping = e.Reply.RoundtripTime;
        }

        /// <summary>
        /// Will display the multiplayer gui
        /// </summary>
        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 150, 130), "Johnny's Multiplayer");
            if (!Manager.serverRunning && !Client.Client.connected)
            {
                if (menu == 0)
                {
                    GUI.Label(new Rect(15, 35, 30, 20), "Ip:");
                    GUI.Label(new Rect(15, 60, 30, 20), "Port:");

                    ip = GUI.TextField(new Rect(45, 35, 100, 20), ip);
                    port = GUI.TextField(new Rect(45, 60, 100, 20), port);

                    if (GUI.Button(new Rect(15, 85, 140, 20), "Connect"))
                    {
                        try
                        {
                            Manager.connectClient(ip, Int32.Parse(port));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Ip or port was not entered correctly.\nException: " + e);
                        }
                    }

                    if (GUI.Button(new Rect(15, 110, 140, 20), "Start Server Menu"))
                    {
                        menu = 1;
                    }
                }
                else
                {
                    GUI.Label(new Rect(15, 35, 30, 20), "Max:");
                    GUI.Label(new Rect(15, 60, 30, 20), "Port:");

                    maxPlayers = GUI.TextField(new Rect(45, 35, 100, 20), maxPlayers);
                    port = GUI.TextField(new Rect(45, 60, 100, 20), port);

                    if (GUI.Button(new Rect(15, 85, 140, 20), "Start"))
                    {
                        try
                        {
                            Manager.startServer(Int32.Parse(maxPlayers), Int32.Parse(port));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Port or maxPlayers was not entered correctly.\nException: " + e);
                        }
                    }

                    if (GUI.Button(new Rect(15, 110, 140, 20), "Connect Menu"))
                    {
                        menu = 0;
                    }
                }
            }
            else if (Manager.serverRunning)
            {
                GUI.Label(new Rect(15, 35, 100, 20), "Server is running");
                GUI.Label(new Rect(15, 60, 100, 20), "Port: " + port);

                /*AutoResetEvent waiter = new AutoResetEvent(false);
                Ping pingSender = new Ping();
                pingSender.PingCompleted += new PingCompletedEventHandler(pingCallback);
                string data = "ping";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 12000;
                PingOptions options = new PingOptions(64, true);
                pingSender.SendAsync(ip, timeout, buffer, options, waiter);*/

                GUI.Label(new Rect(15, 85, 100, 20), "Ping: " + ping);
            }
            else if(Client.Client.connected)
            {
                GUI.Label(new Rect(15, 35, 100, 20), "Connected");

                /*AutoResetEvent waiter = new AutoResetEvent(false);
                Ping pingSender = new Ping();
                pingSender.PingCompleted += new PingCompletedEventHandler(pingCallback);
                string data = "ping";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 12000;
                PingOptions options = new PingOptions(64, true);
                pingSender.SendAsync(ip, timeout, buffer, options, waiter);*/

                GUI.Label(new Rect(15, 60, 100, 20), "Ping: " + ping + "ms");

                if (GUI.Button(new Rect(15, 85, 140, 20), "Disconnect"))
                {
                    Manager.disconnectClient();
                }
            }
        }
    }
}
