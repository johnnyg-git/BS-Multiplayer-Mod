using BS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer_Mod.DataHolders
{
    /// <summary>
    /// Used for transfering item information across the network
    /// </summary>
    public class ItemData
    {
        public int networkId;
        public string itemId;

        public int playerControl;

        public ObjectData objectData;
        public Item clientsideItem;
        public int clientsideId;

        public DateTime toDelete;

        public ItemData(int networkId, string itemId)
        {
            this.itemId = itemId;
            this.networkId = networkId;
            objectData = new ObjectData();
            toDelete = DateTime.Now.AddSeconds(1);
        }

        public ItemData(int networkId, string itemId, ObjectData objectData)
        {
            this.itemId = itemId;
            this.networkId = networkId;
            this.objectData = objectData;
            toDelete = DateTime.Now.AddSeconds(1);
        }
    }
}
