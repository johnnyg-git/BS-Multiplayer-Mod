using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Multiplayer_Mod
{
    class Program
    {
        static bool isRunning = true;
        public static float tickRate = 1 / 90;

        static void Main(string[] args)
        {
            /*if(args.Length==0 || args.Length==1 || args.Length == 2)
            {
                Console.WriteLine("Arguements when launching: \nMaxPlayers\nPort\nTickrate");
                return;
            }*/

            tickRate = 1 / 40;
            Server.Server.Start(25, 26950);
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                if(_nextLoop < DateTime.Now)
                {
                    if (Server.Server.players.Count > 0)
                    {
                        Server.ServerSender.SendItemData();
                        Server.ServerSender.SendPlayerData();
                    }
                    ThreadManager.UpdateMain();

                    _nextLoop = _nextLoop.AddMilliseconds(tickRate);
                }
            }
        }
    }
}
