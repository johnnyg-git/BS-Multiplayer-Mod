using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Multiplayer_Mod
{
    /// <summary>
    /// Base for networkable objects
    /// </summary>
    public class ObjectData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }
}
