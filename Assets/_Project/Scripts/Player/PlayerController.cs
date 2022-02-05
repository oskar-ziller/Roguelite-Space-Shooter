using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MeteorGame
{
    public enum JumpState
    {
        Jumping,
        Falling,
        Grounded
    }

    public struct FrameInput
    {
        public float z;
        public float x;
        public bool jumpPressed;
        public bool jumpReleased;
        public bool jumpDown;
        public bool boostDown;
        public bool descentDown;

        public Vector2 inputVector => Vector2.ClampMagnitude(new Vector2(x, z), 1f);
    }



    public class PlayerController : MonoBehaviour
    {
        #region Variables

        [Header("ACCEL")]
        [SerializeField] private float groundAccel = 10f;
        [SerializeField] private float groundDecel = 10f;
        [SerializeField] private float airAccel = 1f;
        [SerializeField] private float airDecel = 10f;

        [Header("BOOST")]
        [Tooltip("Max speed multiplier wheen boost key is down, higher value = faster when boosting")]
        [SerializeField] private float boostMultipMaxVel = 2f;
        [Tooltip("Accel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipAccel = 2f;


        [Header("SPEED")]
        [SerializeField] private float airMaxSpeed = 10f;
        [SerializeField] private float groundMaxSpeed = 10f;


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



        private bool CanUseCoyote => coyoteUsable && !isGrounded && timeLeftGrounded + coyoteTime > Time.time;
        private bool coyoteUsable;
        private bool hasBufferedJump = false;
        private FrameInput inputs;
        private float lastJumpPressed = float.MinValue;
        private Rigidbody rb;
        private bool isGrounded;
        private float timeLeftGrounded;
        private float currentBoostMultipAccel = 1f;
        private float currentBoostMultipMaxVel = 1f;
        private Vector3 targetJumpVel = Vector3.zero;
        private bool endedJumpEarly = true;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }


        void Start()
        {
        }

        private void OnCollisionEnter(Collision collision)
        {

            if (collision.collider.tag == "Ground")
            {
                isGrounded = true;
                coyoteUsable = true;
                hasBufferedJump = lastJumpPressed + jumpBuffer > Time.time;
            }

        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.tag == "Ground")
            {
                isGrounded = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {

            if (collision.collider.tag == "Ground")
            {
                isGrounded = false;
                timeLeftGrounded = Time.time;
            }
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

            currentBoostMultipAccel = 1f;
            currentBoostMultipMaxVel = 1f;

            if (inputs.boostDown)
            {
                currentBoostMultipAccel = boostMultipAccel;
                currentBoostMultipMaxVel = boostMultipMaxVel;
            }

            var groundVel = CalculateGroundVelocity();
            var jumpVel = CalculateJump();
            var grav = CalculateGravity();

            rb.velocity += groundVel;
            rb.velocity += jumpVel;
            rb.velocity += grav;
        }



        private void GatherInput()
        {
            inputs = new FrameInput
            {
                jumpPressed = UnityEngine.Input.GetButtonDown("Jump"),
                jumpReleased = UnityEngine.Input.GetButtonUp("Jump"),
                jumpDown = UnityEngine.Input.GetButton("Jump"),
                boostDown = Input.GetKey(KeyCode.LeftShift),
                descentDown = Input.GetKey(KeyCode.LeftControl),
                x = UnityEngine.Input.GetAxisRaw("Horizontal"),
                z = UnityEngine.Input.GetAxisRaw("Vertical")
            };

            if (inputs.jumpPressed)
            {
                lastJumpPressed = Time.time;
            }
        }


        #endregion

        #region Methods


        //public void TurnHorizontal(float amount)
        //{
        //    transform.Rotate(Vector3.up, amount);
        //}

        private Vector3 CalculateJump()
        {
            Vector3 toReturn = Vector3.zero;

            // Jump if: grounded or within coyote threshold || sufficient jump buffer
            if ((inputs.jumpPressed || hasBufferedJump) && (isGrounded || CanUseCoyote))
            {
                endedJumpEarly = false;
                coyoteUsable = false;
                timeLeftGrounded = float.MinValue;
                toReturn = Vector3.up * (jumpForce + -rb.velocity.y);
                targetJumpVel = rb.velocity + toReturn;
                hasBufferedJump = false;
            }


            // End the jump early if button released
            if (!isGrounded && inputs.jumpReleased && !endedJumpEarly
                && rb.velocity.y > targetJumpVel.y * 0.75f)
            {
                endedJumpEarly = true;
            }

            return toReturn;
        }


        private Vector3 CalculateGravity()
        {
            if (isGrounded)
            {
                return Vector3.zero;
            }

            float gravity = maxGravity;

            if (inputs.jumpDown)
            {
                gravity = minGravity;
            }

            var magnitude = gravity * Time.deltaTime;

            // if pressing jump but falling too fast for slow jump speed
            if (rb.velocity.y < fallLimitMin && inputs.jumpDown)
            {
                magnitude = -maxGravity * Time.deltaTime;
            }

            return Vector3.up * magnitude;
        }


        private Vector3 CalculateGroundVelocity()
        {
            var currentVel = rb.velocity;

            float currentX = Vector3.Dot(currentVel, transform.right);
            float currentZ = Vector3.Dot(currentVel, transform.forward);

            float acceleration = isGrounded ? groundAccel : airAccel;
            float deceleration = isGrounded ? groundDecel : airDecel;

            

            float newX, newZ;

            if (inputs.inputVector.x != 0)
            {
                var a = acceleration * currentBoostMultipAccel;
                var s = isGrounded ? groundMaxSpeed : airMaxSpeed;

                var targetVel = inputs.inputVector.x * s * currentBoostMultipMaxVel;

                newX = Mathf.MoveTowards(currentX, targetVel, a * Time.deltaTime);
            }
            else
            {
                var a = deceleration * currentBoostMultipAccel;
                newX = Mathf.MoveTowards(currentX, 0, a * Time.deltaTime);
            }


            if (inputs.inputVector.y != 0)
            {
                var s = isGrounded ? groundMaxSpeed : airMaxSpeed;
                var a = acceleration * currentBoostMultipAccel;

                var targetVel = inputs.inputVector.y * s * currentBoostMultipMaxVel;

                newZ = Mathf.MoveTowards(currentZ, targetVel, a * Time.deltaTime);
            }
            else
            {
                var a = deceleration * currentBoostMultipAccel;
                newZ = Mathf.MoveTowards(currentZ, 0, a * Time.deltaTime);
            }

            // Calculate the velocities in the direction we are looking
            Vector3 xVel = transform.right * (newX - currentX);
            Vector3 zVel = transform.forward * (newZ - currentZ);

            Vector3 vel = xVel + zVel;

            return vel;
        }


        #endregion
    }

}


