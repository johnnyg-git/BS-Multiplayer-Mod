using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BSMP
{
    public static class UI
    {
        static string ip = "127.0.0.1";
        static string maxPlayers = "25";
        static string port = "26950";
        static int menu = 1;

        static List<string> messages = new List<string>();

        public static void Message(string message)
        {
            messages.Add(message);
        }

        internal static void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10)) menu = menu == 1 || menu == 2 ? 0 : 1;
        }

        internal static void OnGUI()
        {
            try
            {
                if (menu != 0)
                {
                    GUI.skin.label.wordWrap = true;
                    GUI.Box(new Rect(10, 10, 150, 135), "B&S Multiplayer\nPress F10 to close");
                    if (!Manager.connected() && !Server.ServerRunning())
                    {
                        if (menu != 2)
                        {
                            GUI.Label(new Rect(15, 45, 30, 20), "Ip:");
                            GUI.Label(new Rect(15, 70, 30, 20), "Port:");

                            ip = GUI.TextField(new Rect(45, 45, 100, 20), ip);
                            port = GUI.TextField(new Rect(45, 70, 100, 20), port);

                            if (GUI.Button(new Rect(15, 95, 140, 20), "Connect"))
                            {
                                try
                                {
                                    Message("Connect");
                                    // Connect to server
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError("Ip or port was not entered correctly.\nException: " + e);
                                }
                            }

                            if (GUI.Button(new Rect(15, 120, 140, 20), "Start Server Menu"))
                                menu = 2;
                        }
                        else
                        {
                            GUI.Label(new Rect(15, 45, 30, 20), "Max:");
                            GUI.Label(new Rect(15, 70, 30, 20), "Port:");

                            maxPlayers = GUI.TextField(new Rect(45, 45, 100, 20), maxPlayers);
                            port = GUI.TextField(new Rect(45, 70, 100, 20), port);

                            if (GUI.Button(new Rect(15, 95, 140, 20), "Start"))
                            {
                                try
                                {
                                    Server.Start(Int32.Parse(maxPlayers), Int32.Parse(port));
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError("Port or maxPlayers was not entered correctly.\nException: " + e);
                                }
                            }

                            if (GUI.Button(new Rect(15, 120, 140, 20), "Connect Menu"))
                                menu = 1;
                        }
                    }
                    else if (Server.ServerRunning())
                    {
                        GUI.Label(new Rect(15, 45, 100, 20), "Server is running");
                        GUI.Label(new Rect(15, 60, 100, 20), "Port: " + port);

                        if (GUI.Button(new Rect(15, 120, 140, 20), "Shutdown"))
                        {
                            Server.Shutdown();
                        }

                    }
                    else if (Manager.connected())
                    {
                        GUI.Label(new Rect(15, 45, 100, 20), "Connected");

                        if (GUI.Button(new Rect(15, 120, 140, 20), "Disconnect"))
                        {
                            // Disconnect
                        }
                    }
                }

                int i = 0;
                List<int> toDel = new List<int>();
                foreach (string message in messages)
                {
                    GUI.Box(new Rect(170 + i * 160, 10, 150, 135), "Message");
                    GUI.Label(new Rect(175 + i * 160, 25, 100, 100), message);

                    if (GUI.Button(new Rect(175 + i * 160, 120, 140, 20), "Close"))
                        toDel.Add(i);

                    i++;
                }

                foreach (int point in toDel)
                    messages.RemoveAt(point);
            }
            catch (Exception e)
            {
                Debug.LogError("Error whilst running gui:\n " + e);
                Message("Error whilst running gui, please check console for further details.");
            }
        }
    }
}
