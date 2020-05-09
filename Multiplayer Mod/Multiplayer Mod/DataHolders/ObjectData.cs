using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer_Mod
{
    public class ObjectData
    {
        public Transform transform;
        public Rigidbody rb;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        // For only server
        public Vector3 prevPosition;
        public Quaternion prevRotation;
        public Vector3 prevVelocity;
        public Vector3 prevAngularVelocity;
    }
}
