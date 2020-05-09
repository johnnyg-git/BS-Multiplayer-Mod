using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer_Mod
{
    /// <summary>
    /// Defines every type of packet possible
    /// </summary>
    public enum packetTypes
    {
        playerInfo = 1,
        welcome,
        disconnect,
        error
    }
}
