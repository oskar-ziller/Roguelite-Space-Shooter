using UnityEngine;


namespace MeteorGame.Flight
{
    public struct FrameInput
    {
        public float z;
        public float x;
        public float jump;
        //public bool jumpDown;
        public bool boostDown;
        //public bool descentDown;

        public Vector2 inputVector => Vector2.ClampMagnitude(new Vector2(x, z), 1f);
    }

}


