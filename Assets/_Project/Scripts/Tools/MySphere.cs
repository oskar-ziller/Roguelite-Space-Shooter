using UnityEngine;

namespace MeteorGame
{
    public class MySphere
    {
        public Vector3 center;
        public float radi;

        public MySphere(float radi, Vector3 validRandomPos)
        {
            this.radi = radi;
            this.center = validRandomPos;
        }
    }
}
