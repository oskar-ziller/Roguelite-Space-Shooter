using UnityEngine;


namespace MeteorGame.Flight
{
    public struct FrameInput
    {
        public float z;
        public float x;
        public float jump;
        public bool isBoosting;

        public Vector2 inputVector => Vector2.ClampMagnitude(new Vector2(x, z), 1f);
    }

}


