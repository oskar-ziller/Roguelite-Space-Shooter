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

        [Header("BOOST")]
        [Tooltip("Max speed multiplier wheen boost key is down, higher value = faster when boosting")]
        [SerializeField] private float boostMultipMaxVel = 2f;
        [Tooltip("Accel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipAccel = 2f;


        [Header("SPEED")]
        [SerializeField] private float airMaxSpeed = 10f;


        [Header("MISC")]
        [Tooltip("Window of time to allow jumping after leaving ground surface")]
        [SerializeField] private float coyoteTime = 0.1f;
        [Tooltip("Window of time to remember wanting to jump before hitting ground. " +
            "So we can jump even at an early key press.")]
        [SerializeField] private float jumpBuffer = 0.1f;


        [Header("GRAVITY")]
        [Tooltip("Gravity applied when jump key is down")]
        [SerializeField] private float minGravity = -1;
        [Tooltip("Gravity applied when jump key is not down")]
        [SerializeField] private float maxGravity = -2;


        [Header("JUMPING")]
        [SerializeField] private float jumpForce = 30;


        [Header("FALL")]
        [Tooltip("Downwards speed clamp when falling with jump key pressed")]
        [SerializeField] private float fallLimitMin = -30f;



        private FrameInput inputs;
        //private Rigidbody rb;
        private float boostAccel = 1f;
        private float boostSpeed = 1f;
        private Vector3 targetJumpVel = Vector3.zero;


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
            boostSpeed = 1f;

            if (inputs.boostDown)
            {
                boostAccel = boostMultipAccel;
                boostSpeed = boostMultipMaxVel;
            }

            //var groundVel = CalculateGroundVelocity();
            //var asc = CalculateAscend();
            var fwd = CalculateForward2();
            var rgh = CalculateRight2();
            var up = CalculateUp2();
            //var upSlow = CalculateUpSlow();

            //var stop = CalculateStop();
            //var grav = CalculateGravity();
            //var desc = CalculateDescent();

            velocity += fwd + rgh + up;
            transform.position += velocity;

            //velocity += fwd + jumpVel;

            //velocity += groundVel;
            //velocity += asc;
            //velocity += grav;
            //velocity += desc;
        }

        private Vector3 CalculateStop()
        {
            if (inputs.jump == 0 && inputs.inputVector == Vector2.zero)
            {
                var accel = airDecel * boostAccel;
                return Vector3.MoveTowards(velocity, Vector3.zero, accel * Time.deltaTime) - velocity;
            }

            return Vector3.zero;
        }

        private Vector3 CalculateUp()
        {
            if (inputs.jump != 0)
            {
                var targetSpeed = inputs.jump * airMaxSpeed * boostSpeed;
                var accel = airAccel * boostAccel;

                var newY = Mathf.MoveTowards(velocity.y, targetSpeed, accel * Time.deltaTime);

                return new Vector3(velocity.x, newY, velocity.z) - velocity;
            }
            else
            {
                var locVel = transform.InverseTransformDirection(velocity);

                var targetSpeed = 0;
                var accel = airAccel * boostAccel;
                locVel.y = Mathf.MoveTowards(locVel.y, targetSpeed, accel * Time.deltaTime); ;

                var globalVel = transform.TransformDirection(locVel);

                return globalVel - velocity;
            }

        }

        private Vector3 CalculateUp2()
        {
            if (inputs.jump != 0)
            {
                var targetSpeed2 = inputs.jump * airMaxSpeed * boostSpeed;
                var accel2 = airAccel * boostAccel;

                if (Mathf.Abs(velocity.y) > Mathf.Abs(targetSpeed2))
                {
                    accel2 = airDecel * boostAccel;
                }

                var newY = Mathf.MoveTowards(velocity.y, targetSpeed2, accel2 * Time.deltaTime);

                return new Vector3(velocity.x, newY, velocity.z) - velocity;
            }

            var locVel = transform.InverseTransformDirection(velocity);

            var targetSpeed = inputs.jump * airMaxSpeed * boostSpeed;
            var accel = airAccel * boostAccel;

            if (Mathf.Abs(locVel.y) > Mathf.Abs(targetSpeed))
            {
                accel = airDecel * boostAccel;
            }

            locVel.y = Mathf.MoveTowards(locVel.y, targetSpeed, accel * Time.deltaTime); ;

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - velocity;
        }


        private Vector3 CalculateUpSlow()
        {
            var locVel = transform.InverseTransformDirection(velocity);

            var targetSpeed = 0;
            var accel = airAccel * boostAccel;
            locVel.y = Mathf.MoveTowards(locVel.y, targetSpeed, accel * Time.deltaTime); ;

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - velocity;
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


        private Vector3 CalculateForward2()
        {
            if (inputs.jump != 0)
            {
                return Vector3.zero;
            }

            var locVel = transform.InverseTransformDirection(velocity);

            var targetSpeed = inputs.inputVector.y * airMaxSpeed * boostSpeed;
            var accel = airAccel * boostAccel;

            if (Mathf.Abs(locVel.z) > Mathf.Abs(targetSpeed))
            {
                accel = airDecel * boostAccel;
            }

            locVel.z = Mathf.MoveTowards(locVel.z, targetSpeed, accel * Time.deltaTime); ;

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - velocity;
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


        private Vector3 CalculateRight2()
        {
            if (inputs.jump != 0)
            {
                return Vector3.zero;
            }

            var locVel = transform.InverseTransformDirection(velocity);

            var targetSpeed = inputs.inputVector.x * airMaxSpeed * boostSpeed;
            var accel = airAccel * boostAccel;

            if (Mathf.Abs(locVel.x) > Mathf.Abs(targetSpeed))
            {
                accel = airDecel * boostAccel;
            }

            locVel.x = Mathf.MoveTowards(locVel.x, targetSpeed, accel * Time.deltaTime); ;

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - velocity;
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


