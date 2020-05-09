using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer_Mod
{
    public class GUIManager : MonoBehaviour
    {
        public string ip = "127.0.0.1";
        public string maxPlayers = "25";
        public string port = "26950";
        public int menu = 0;

        /// <summary>
        /// Will display the multiplayer gui
        /// </summary>
        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 150, 130), "Johnny's Multiplayer");
            if (!Manager.serverRunning)
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
            else
            {
                GUI.Label(new Rect(15, 35, 100, 20), "Server is running");
                GUI.Label(new Rect(15, 60, 100, 20), "Port: " + port);
            }
        }
    }
}
