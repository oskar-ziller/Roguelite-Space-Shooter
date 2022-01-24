using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MeteorGame
{
    public class SpawnInfo
    {
        public ColliderShape shape;
        public Vector3 extends;
        public float r;

        public SpawnInfo(float r)
        {
            shape = ColliderShape.Sphere;
            this.r = r;
        }

        public SpawnInfo(Vector3 extends)
        {
            shape = ColliderShape.Cube;
            this.extends = extends;
        }
    }
}
