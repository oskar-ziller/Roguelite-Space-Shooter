using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MeteorGame
{
    //public enum JumpState
    //{
    //    Jumping,
    //    Falling,
    //    Grounded
    //}

    //public struct FrameInput
    //{
    //    public float z;
    //    public float x;
    //    public bool jumpPressed;
    //    public bool jumpReleased;
    //    public bool jumpDown;
    //    public bool boostDown;

    //    public Vector2 inputVector => Vector2.ClampMagnitude(new Vector2(x, z), 1f);
    //}



    public class FlyController : MonoBehaviour
    {
        #region Variables

        [Header("ACCEL")]
        [SerializeField] private float airAccel = 1f;
        [SerializeField] private float airDecel = 10f;

        [Tooltip("Amount to decel with when pressing no input." +
            " Makes it so that when pressing buttons, manouverability is better.")]
        [SerializeField] private float airDecelFreefall = 10f;

        [Header("BOOST")]
        [Tooltip("Max speed multiplier wheen boost key is down, higher value = faster when boosting")]
        [SerializeField] private float boostMultipMaxVel = 2f;
        [Tooltip("Accel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipAccel = 2f;
        
        [Tooltip("Decel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipDec = 2f;


        [Header("SPEED")]
        [SerializeField] private float airMaxSpeed = 10f;






        private FrameInput inputs;
        //private Rigidbody rb;
        private float boostAccel = 1f;
        private float boostDec = 1f;
        private float boostSpeed = 1f;
        

        private Vector3 velocity = Vector3.zero;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            //rb = GetComponentInParent<Rigidbody>();
        }


        void Start()
        {
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Player.Instance.SpellSlot(1).Cast();
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                Player.Instance.SpellSlot(2).Cast();
            }

            GatherInput();

            boostAccel = 1f;
            boostDec = 1f;
            boostSpeed = 1f;

            if (inputs.boostDown)
            {
                boostAccel = boostMultipAccel;
                boostDec = boostMultipDec;
                boostSpeed = boostMultipMaxVel;
            }

            //var groundVel = CalculateGroundVelocity();
            //var asc = CalculateAscend();
            var fwd = CalculateForward();
            var rgh = CalculateRight();
            var up = CalculateUp();
            var down = CalculateDown();
            var slow = CalculateSlow();

            //var stop = CalculateStop();
            //var grav = CalculateGravity();
            //var desc = CalculateDescent();



            var frameVel = fwd + rgh + up + down + slow;

            //frameVel = frameVel.normalized * airMaxSpeed * Time.deltaTime;

            velocity += frameVel;

            //velocity = velocity.normalized * airMaxSpeed;
            transform.position += velocity;

            //velocity += fwd + jumpVel;

            //velocity += groundVel;
            //velocity += asc;
            //velocity += grav;
            //velocity += desc;
        }

        private Vector3 CalculateDown()
        {
            var pressingFall = inputs.jump < 0;

            if (pressingFall)
            {
                var locVel = transform.InverseTransformDirection(velocity);
                var targetSpeed =  airMaxSpeed * boostSpeed;

                if (locVel.magnitude < Math.Abs(targetSpeed))
                {
                    var accel = airAccel * boostAccel;

                    locVel.y = Mathf.MoveTowards(locVel.y, -targetSpeed, accel * Time.deltaTime); ;

                    var globalVel = transform.TransformDirection(locVel);

                    return globalVel - velocity;
                }
            }

            return Vector3.zero;
        }



        private Vector3 CalculateUp()
        {
            var pressingJump = inputs.jump > 0;

            if (pressingJump)
            {
                var locVel = transform.InverseTransformDirection(velocity);
                var targetSpeed = airMaxSpeed * boostSpeed;

                if (locVel.magnitude < Math.Abs(targetSpeed))
                {
                    var accel = airAccel * boostAccel;

                    locVel.y = Mathf.MoveTowards(locVel.y, targetSpeed, accel * Time.deltaTime); ;

                    var globalVel = transform.TransformDirection(locVel);

                    return globalVel - velocity;
                }
            }

            return Vector3.zero;
















        }


        private Vector3 CalculateSlow()
        {
            var pressingJump = inputs.jump != 0;
            var pressingArrows = inputs.inputVector != Vector2.zero;
            var pressingBoost = inputs.boostDown;
            var accel = airDecel * boostDec;

            if (!pressingJump && !pressingArrows && !pressingBoost) // pressing nothing, low decel
            {
                accel = airDecelFreefall * boostDec;
            }

            var newVel = Vector3.MoveTowards(velocity, Vector3.zero, accel * Time.deltaTime);

            return newVel - velocity;
        }




        private Vector3 CalculateForward()
        {
            if (inputs.inputVector.y != 0)
            {
                var locVel = transform.InverseTransformDirection(velocity);
                var targetSpeed = inputs.inputVector.y * airMaxSpeed * boostSpeed;

                var accel = airAccel * boostAccel;

                locVel.z = Mathf.MoveTowards(locVel.z, targetSpeed, accel * Time.deltaTime); ;

                var globalVel = transform.TransformDirection(locVel);

                return globalVel - velocity;
            }

            return Vector3.zero;
        }




        private Vector3 CalculateRight()
        {
            if (inputs.inputVector.x != 0)
            {
                var locVel = transform.InverseTransformDirection(velocity);
                var targetSpeed = inputs.inputVector.x * airMaxSpeed * boostSpeed;

                var accel = airAccel * boostAccel;

                locVel.x = Mathf.MoveTowards(locVel.x, targetSpeed, accel * Time.deltaTime); ;

                var globalVel = transform.TransformDirection(locVel);

                return globalVel - velocity;
            }

            return Vector3.zero;
        }

        private struct FrameInput
        {
            public float z;
            public float x;
            public float jump;
            //public bool jumpDown;
            public bool boostDown;
            //public bool descentDown;

            public Vector2 inputVector => Vector2.ClampMagnitude(new Vector2(x, z), 1f);
        }

        private void GatherInput()
        {
            inputs = new FrameInput
            {
                //jumpDown = UnityEngine.Input.GetButton("Jump") || UnityEngine.Input.GetKey(KeyCode.E),
                boostDown = Input.GetKey(KeyCode.LeftShift),
                //descentDown = Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.Q),
                x = UnityEngine.Input.GetAxisRaw("Horizontal"),
                z = UnityEngine.Input.GetAxisRaw("Vertical"),
                jump = UnityEngine.Input.GetAxisRaw("Jump")
            };
        }


        #endregion

        #region Methods


        public void TurnHorizontal(float amount)
        {
            transform.Rotate(Vector3.up, amount);
        }



        #endregion
    }

}


