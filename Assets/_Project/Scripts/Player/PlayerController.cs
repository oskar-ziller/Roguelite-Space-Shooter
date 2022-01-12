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


    public class PlayerController : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private float maxAcceleration = 10f, maxAirAcceleration = 1f;

        [SerializeField]
        private float maxSpeed = 10f;

        [SerializeField]
        private float jumpHeight = 2f;

        //[SerializeField, Range(5f, 20f)]
        //private float gravity = 9.81f;

        [SerializeField]
        private float fallMultiplier = 2f;

        [SerializeField]
        private float lowJumpMultiplier = 2f;

        //[SerializeField]
        //private Transform ground;


        //public CameraController cam;

        private bool isOnGround => groundContactCount > 0;
        private int groundContactCount;
        private bool wantsToJump = false;
        private bool wantsToStopJump = false;

        private Vector2 inputVector;
        private Rigidbody rb;
        private JumpState jumpState;


        //private List<GameObject> attractorsInScene;
        //private Transform currentPlanet;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            //attractorsInScene = GameObject.FindGameObjectsWithTag("Attractor").ToList();
        }

        void Start()
        {
            Physics.gravity = new Vector3(0, -9.0f, 0);
        }

        void Update()
        {
            //CheckPlanetChange();
            //UpdateJumpState()


            if (Input.GetKey(KeyCode.Mouse0))
            {
                Player.Instance.SpellSlot(1).Cast();
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                Player.Instance.SpellSlot(2).Cast();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                StopJump();
            }


        }

        private void FixedUpdate()
        {
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.y = Input.GetAxis("Vertical");
            HandleMovement();
            groundContactCount = 0;
        }

        void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        #endregion

        #region Methods

        public void Jump()
        {
            wantsToJump = true;
            wantsToStopJump = false;
        }

        public void StopJump()
        {
            wantsToJump = false;
            wantsToStopJump = true;
        }


        public void TurnHorizontal(float amount)
        {
            transform.Rotate(Vector3.up, amount);
        }

        private void UpdateJumpState()
        {
            Vector3 currentVel = rb.velocity;
            //float dot = Mathf.Round(Vector3.Dot(transform.up, currentVel));

            if (!isOnGround)
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

            Vector3 groundVel = CalculateGroundVelocity(currentVel);
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

        private Vector3 CalculateGroundVelocity(Vector3 currentVel)
        {
            inputVector = Vector2.ClampMagnitude(inputVector, 1f) * maxSpeed;

            float currentX = Vector3.Dot(currentVel, transform.right);
            float currentZ = Vector3.Dot(currentVel, transform.forward);

            float acceleration = isOnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, inputVector.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, inputVector.y, maxSpeedChange);

            currentVel += transform.right * (newX - currentX) + transform.forward * (newZ - currentZ);

            return currentVel;
        }

        private void EvaluateCollision(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                float upDot = Vector3.Dot(transform.up, normal);

                if (upDot >= 0.7f)
                {
                    groundContactCount++;
                }
            }
        }

        #endregion
    }

}


