using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS;
using UnityEngine;
using IngameDebugConsole;

namespace Multiplayer_Mod
{
    /// <summary>
    /// The first entry of the mod
    /// </summary>
    public class EntryPoint : LevelModule
    {
        /// <summary>
        /// Called when master scene is loaded
        /// Creates the manager instance and adds commands to the debug console
        /// </summary>
        public override void OnLevelLoaded(LevelDefinition levelDefinition)
        {
            // Creates the manager GameObject and gives it the Manager MonoBehaviour
            // Makes it so that GameObject is not destroyed on loading a new scene
            Manager m = new GameObject().AddComponent<Manager>();
            GameObject.DontDestroyOnLoad(m.gameObject);

            // Adds the commands to the debug console
            DebugLogConsole.AddCommandInstance("connect", "Connect as client, Parameters: ip, port", "connectClient", m);
            DebugLogConsole.AddCommandInstance("start", "Start a server, Parameters, port, maxPlayers", "startServer", m);
            DebugLogConsole.AddCommandInstance("disconnect", "Disconnect from a server", "disconnectClient", m);
        }
    }
}
