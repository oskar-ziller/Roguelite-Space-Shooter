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

        public Vector2 inputVector => Vector2.ClampMagnitude(new Vector2(x, z), 1f);
    }



    public class PlayerController : MonoBehaviour
    {
        #region Variables


        [SerializeField] private float groundAccel = 10f;
        [SerializeField] private float airAccel = 1f;
        [SerializeField] private float groundDecel = 10f;
        [SerializeField] private float airDecel = 10f;

        [SerializeField] private float boostMultipAccel = 2f; // 
        [SerializeField] private float boostMultipMaxVel = 2f; // 


        [SerializeField] private CapsuleCollider playerCollider;


        [SerializeField]
        private float maxSpeed = 10f;

        [SerializeField]
        private float jumpHeight = 2f;

        //[SerializeField, Range(5f, 20f)]
        //private float gravity = 9.81f;



        private bool firstTime = true;

        //[SerializeField]
        //private Transform ground;


        //public CameraController cam;

        private bool wantsToJump = false;

        private Rigidbody rb;
        private JumpState jumpState;
        private bool isGrounded;
        private float timeLeftGrounded;

        private float currentBoostMultipAccel = 1f;
        private float currentBoostMultipMaxVel = 1f;

        int layerMaskOverlapSphere;


        //private List<GameObject> attractorsInScene;
        //private Transform currentPlanet;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            layerMaskOverlapSphere = LayerMask.GetMask(new string[] { "Arena" });
        }

        GameObject debugObj;

        void Start()
        {
            debugObj = Instantiate(UNgroundedPrefab);
        }


        public GameObject groundedPrefab, UNgroundedPrefab;

        private void CreateObjectAtCollision(Collision collision, bool grounded)
        {
            Vector3 pos = collision.contactCount == 0 ? rb.position
                                                        : collision.GetContact(0).point;

            GameObject toCreate = grounded ? groundedPrefab : UNgroundedPrefab;
            Instantiate(toCreate, pos, Quaternion.identity);
        }

        private void OnCollisionEnter(Collision collision)
        {

            if (collision.collider.tag == "Ground")
            {
                print($"-coll- enter isGrounded: {isGrounded} -> true");

                isGrounded = true;
                coyoteUsable = true;
                hasBufferedJump = lastJumpPressed + _jumpBuffer > Time.time;
                //CreateObjectAtCollision(collision, true);
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
                print($"-coll- enter isGrounded: {isGrounded} -> false");

                isGrounded = false;
                timeLeftGrounded = Time.time;
                //CreateObjectAtCollision(collision, false);
            }
        }

        Vector3 groundVel;

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

            CalculateJumpApex();
            CalculateFallSpeed();

            var groundVel = CalculateGroundVelocity();
            var jumpVel = CalculateJump();
            var grav = CalculateGravity();

            rb.velocity += groundVel;
            rb.velocity += jumpVel;
            rb.velocity += grav;
        }


        public FrameInput inputs;
        private float lastJumpPressed = float.MinValue;
        private float _apexPoint;
        private float _fallSpeed;

        [SerializeField] private float minGravityDecel = -1;
        [SerializeField] private float maxGravityDecel = -2;
        private float _jumpApexThreshold = 10;


        private void GatherInput()
        {
            inputs = new FrameInput
            {
                jumpPressed = UnityEngine.Input.GetButtonDown("Jump"),
                jumpReleased = UnityEngine.Input.GetButtonUp("Jump"),
                jumpDown = UnityEngine.Input.GetButton("Jump"),
                boostDown = Input.GetKey(KeyCode.LeftShift),
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



        public void TurnHorizontal(float amount)
        {
            transform.Rotate(Vector3.up, amount);
        }

        private void UpdateJumpState()
        {
            Vector3 currentVel = rb.velocity;
            //float dot = Mathf.Round(Vector3.Dot(transform.up, currentVel));

            if (!isGrounded)
            {
                if (currentVel.y <= 0)
                {
                    jumpState = JumpState.Falling;
                }
            }
            else
            {
                if (jumpState == JumpState.Falling)
                {
                    wantsToJump = false;
                    jumpState = JumpState.Grounded;
                }
            }
        }

        private void HandleMovement()
        {
            UpdateJumpState();

            Vector3 currentVel = rb.velocity;

            //Vector3 groundVel = CalculateGroundVelocity(currentVel);
            //Vector3 gravityVel = Physics.gravity.y * Time.deltaTime * transform.up;

            Vector3 jumpVel = Vector3.zero;
            //Vector3 lowJumpVel = Vector3.zero;
            
            Vector3 gravity = Physics.gravity.y * Time.deltaTime * transform.up;

            if (jumpState == JumpState.Grounded)
            {
                if (wantsToJump)
                {
                    wantsToJump = false;
                    jumpVel = CalculateJumpVelocity(currentVel);
                    jumpState = JumpState.Jumping;
                }
            }


            if (jumpState == JumpState.Falling)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    if (rb.velocity.y < -5)
                    {
                        gravity += new Vector3(0, 1f, 0);
                        rb.drag = .5f;

                    }
                }
                else
                {
                    gravity += new Vector3(0, -1f, 0);

                    if (rb.velocity.y < -20)
                    {
                        rb.drag = 1.5f;
                    }
                }
            }

            if (jumpState == JumpState.Jumping)
            {
                if (!Input.GetKey(KeyCode.Space))
                {
                    rb.drag = 3f;
                    //gravity += new Vector3(0, -lowJumpMultiplier, 0);
                }
                else
                {
                    rb.drag = 0.2f;
                }
            }


            // set velocity
            rb.velocity = groundVel;
            rb.velocity += gravity;
            rb.velocity += jumpVel;
        }



        private void CalculateJumpApex()
        {
            if (!isGrounded)
            {
                // Gets stronger the closer to the top of the jump
                _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(rb.velocity.y));
            }
            else
            {
                _apexPoint = 0;
            }
        }

        private void CalculateFallSpeed()
        {
            _fallSpeed = Mathf.Lerp(maxGravityDecel, minGravityDecel, _apexPoint);
        }

        private Vector3 CalculateJumpVelocity(Vector3 currentVel)
        {
            //if (!isOnGround)
            //{
            //    return Vector3.zero;
            //}

            Vector3 jumpDirection = Vector3.up;
            float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);

            //float alignedSpeed = Vector3.Dot(currentVel, jumpDirection);

            //// if we are jumping from a sloped surface (i think)
            //// if jumpDirection and currentVel is not perpendicular 
            //if (alignedSpeed > 0.01f)
            //{
            //    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            //}

            return jumpDirection * jumpSpeed;
        }



        private bool CanUseCoyote => coyoteUsable && !isGrounded && timeLeftGrounded + _coyoteTimeThreshold > Time.time;
        private bool coyoteUsable;

        [SerializeField] private float _coyoteTimeThreshold = 0.1f;
        private bool hasBufferedJump = false;
        [SerializeField] private float _jumpBuffer = 0.1f;

        private float verticalVel;

        private Vector3 targetJumpVel = Vector3.zero;


        [Header("JUMPING")] [SerializeField] private float jumpForce = 30;

        private bool endedJumpEarly = true;

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
                // _currentVerticalSpeed = 0;
                endedJumpEarly = true;
            }

            return toReturn;
        }





        [Header("GRAVITY")] [SerializeField] private float fallVelLimitSlow = -30f;
        [SerializeField] private float fallVelLimitFast = -70f;

        private Vector3 CalculateGravity()
        {
            if (isGrounded)
            {
                return Vector3.zero;
            }

            float gravity = maxGravityDecel;

            if (inputs.jumpDown)
            {
                gravity = minGravityDecel;
            }


            float clamp = fallVelLimitFast;

            if (inputs.jumpDown)
            {
                clamp = fallVelLimitSlow;
            }

            //var fallSpeed = !isGrounded && !inputs.jumpDown ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;
            //var gravityVel = !inputs.jumpDown ? maxGravityDecel : minGravityDecel;
            //var clamp = gravityVel == maxGravityDecel ? fallVelLimitFast : fallVelLimitSlow;

            var magnitude = gravity * Time.deltaTime;

            // if pressing jump but falling too fast for slow jump speed
            if (rb.velocity.y < fallVelLimitSlow && inputs.jumpDown)
            {
                magnitude = -maxGravityDecel * Time.deltaTime;
            }


            //var targetVel = rb.velocity.y + vel;
            //// Clamp
            //if (targetVel < clamp)
            //{
            //    vel = 0;

            //    //if (inputs.jumpDown)
            //    //{
            //    //    vel = airDecel;
            //    //}
            //}

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
                var targetVel = inputs.inputVector.x * maxSpeed * currentBoostMultipMaxVel;
                var a = acceleration * currentBoostMultipAccel;

                newX = Mathf.MoveTowards(currentX, targetVel, a * Time.deltaTime);
            }
            else
            {
                var a = deceleration * currentBoostMultipAccel;
                newX = Mathf.MoveTowards(currentX, 0, a * Time.deltaTime);
            }


            if (inputs.inputVector.y != 0)
            {

                var targetVel = inputs.inputVector.y * maxSpeed * currentBoostMultipMaxVel;
                var a = acceleration * currentBoostMultipAccel;

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


