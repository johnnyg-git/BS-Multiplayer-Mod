using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer_Mod
{
    public class PlayerData
    {
        public ObjectData leftHand;
        public ObjectData rightHand;
        public ObjectData head;

        public PlayerData()
        {
            leftHand = new ObjectData();
            rightHand = new ObjectData();
            head = new ObjectData();
        }
    }
}
