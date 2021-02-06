using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace BSMP
{
    public class Manager : MonoBehaviour
    {
        public static Manager instance;

        public static bool connected()
        {
            return false;
        }

        void Start()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
        }

        void Update()
        {
            UI.Update();
        }

        void OnGUI()
        {
            UI.OnGUI();
        }
    }
}
