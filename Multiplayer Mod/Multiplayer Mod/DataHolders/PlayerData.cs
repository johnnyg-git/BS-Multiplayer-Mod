using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer_Mod
{
    /// <summary>
    /// Used for transfering player data across the network
    /// </summary>
    public class PlayerData
    {
        public int id;
        public ObjectData leftHand;
        public ObjectData rightHand;
        public ObjectData head;

        public PlayerData(int id)
        {
            this.id = id;
            leftHand = new ObjectData();
            rightHand = new ObjectData();
            head = new ObjectData();
        }
    }
}
