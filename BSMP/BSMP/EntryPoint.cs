using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace BSMP
{
    class EntryPoint : LevelModule
    {
        public override IEnumerator OnLoadCoroutine(Level level)
        {
            GameObject manager = new GameObject();
            manager.AddComponent<Manager>();
            GameObject.DontDestroyOnLoad(manager);

            return base.OnLoadCoroutine(level);
        }
    }
}
