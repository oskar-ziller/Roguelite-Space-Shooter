using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public struct RayRange
    {
        public RayRange(float x1, float y1, float x2, float y2, Vector2 dir)
        {
            Start = new Vector2(x1, y1);
            End = new Vector2(x2, y2);
            Dir = dir;
        }

        public readonly Vector2 Start, End, Dir;
    }



    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// Right now it only contains movement and jumping, but it should be pretty easy to expand... I may even do it myself
    /// if there's enough interest. You can play and compete for best times here: https://tarodev.itch.io/
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/GqeHHnhHpz
    /// </summary>
    public class PlayerControllerNew : MonoBehaviour
    {
        // Public for external hooks
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public bool Grounded => isGrounded;

        private Vector3 _lastPosition;
        private float newX, newZ, verticalVel;

        //[Header("WALKING")]
        //[SerializeField] private float acceleration = 90;

        [Tooltip("Maximum velocity")]
        [SerializeField] private float moveClamp = 13;
        [SerializeField] private float apexBonusMultiplier = 2;


        [Header("COLLISION")] [SerializeField] private Bounds _characterBounds;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private int _detectorCount = 3;
        [SerializeField] private float _detectionRayLength = 0.1f;
        [SerializeField] [Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground


        [SerializeField] private Rigidbody rigidBody;




        [SerializeField] private float maxGroundAcceleration = 50f;
        [SerializeField] private float maxAirAcceleration = 50f;

        [SerializeField] private float maxGroundDeAcceleration = 60f;






        [Header("MOVE")]
        [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
        private int _freeColliderIterations = 10;









        private bool isGrounded;
        private float _timeLeftGrounded;



        



        private void OnCollisionEnter(Collision collision)
        {
            if (!isGrounded && collision.collider.tag == "Ground")
            {
                isGrounded = true;
                LandingThisFrame = true;
                _coyoteUsable = true; // Only trigger when first touching
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (isGrounded && collision.collider.tag == "Ground")
            {
                isGrounded = false;
            }
        }

        private void Update()
        {
            newX = 0;
            newZ = 0;

            // Calculate velocity
            Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            GatherInput();
            CalculateWalk(); 

            /*
            CalculateJumpApex(); // Affects fall speed, so calculate before gravity
            CalculateGravity(); // Vertical movement

            */

            CalculateJump(); // Possibly overrides vertical

            MoveCharacter(); // Actually perform the axis movement
        }

        private void GatherInput()
        {
            Input = new FrameInput
            {
                jumpPressed = UnityEngine.Input.GetButtonDown("Jump"),
                jumpReleased = UnityEngine.Input.GetButtonUp("Jump"),
                jumpDown = UnityEngine.Input.GetButton("Jump"),
                x = UnityEngine.Input.GetAxisRaw("Horizontal"),
                z = UnityEngine.Input.GetAxisRaw("Vertical")
            };

            if (Input.jumpPressed)
            {
                _lastJumpPressed = Time.time;
            }
        }


        private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
        {
            for (var i = 0; i < _detectorCount; i++)
            {
                var t = (float)i / (_detectorCount - 1);
                yield return Vector2.Lerp(range.Start, range.End, t);
            }
        }

        private void CalculateWalk()
        {
            Vector3 currentVel = rigidBody.velocity;
            float currentVelMag = rigidBody.velocity.magnitude;

            float currentX = Vector3.Dot(currentVel, transform.right);
            float currentZ = Vector3.Dot(currentVel, transform.forward);
            float acceleration = isGrounded ? maxGroundAcceleration : maxAirAcceleration;


            if (Input.x != 0)
            {
                if (currentX < moveClamp)
                {
                    newX = Input.x * acceleration * Time.deltaTime;
                }

                // Apply bonus at the apex of a jump
                var apexBonus = Mathf.Sign(Input.x) * apexBonusMultiplier * _apexPoint;
                newX += apexBonus * Time.deltaTime;
            }
            else // No input. Let's slow the character down
            {
                var target = Mathf.MoveTowards(currentX, 0, maxGroundDeAcceleration * Time.deltaTime);
                var slowDownAmount = currentX - target;

                newX -= slowDownAmount;
            }

            if (Input.z != 0)
            {
                if (currentZ < moveClamp)
                {
                    newZ = Input.z * acceleration * Time.deltaTime;
                }

                // Apply bonus at the apex of a jump
                var apexBonus = Mathf.Sign(Input.z) * apexBonusMultiplier * _apexPoint;
                newZ += apexBonus * Time.deltaTime;
            }
            else // No input. Let's slow the character down
            {
                var target = Mathf.MoveTowards(currentZ, 0, maxGroundDeAcceleration * Time.deltaTime);
                var slowDownAmount = currentZ - target;

                newZ -= slowDownAmount;
            }
        }


        

        // We cast our bounds before moving to avoid future collisions
        private void MoveCharacter()
        {
            //RawMovement = new Vector3(newX * transform.right, 0, newZ); // Used externally

            var right = newX * transform.right;
            var fwd = newZ * transform.forward;



            var currMoveVel = rigidBody.velocity.magnitude;

            rigidBody.velocity += right;
            rigidBody.velocity += fwd;

        }


        #region Gravity

        [Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
        [SerializeField] private float _minFallSpeed = 80f;
        [SerializeField] private float _maxFallSpeed = 120f;
        private float _fallSpeed;

        private void CalculateGravity()
        {
            if (isGrounded)
            {
                // Move out of the ground
                if (verticalVel < 0) verticalVel = 0;
            }
            else
            {
                // Add downward force while ascending if we ended the jump early
                var fallSpeed = _endedJumpEarly && verticalVel > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

                // Fall
                verticalVel -= fallSpeed * Time.deltaTime;

                // Clamp
                if (verticalVel < _fallClamp) verticalVel = _fallClamp;
            }
        }

        #endregion

        #region Jump

        [Header("JUMPING")] [SerializeField] private float _jumpHeight = 30;
        [SerializeField] private float _jumpApexThreshold = 10f;
        [SerializeField] private float _coyoteTimeThreshold = 0.1f;
        [SerializeField] private float _jumpBuffer = 0.1f;
        [SerializeField] private float _jumpEndEarlyGravityModifier = 3;
        private bool _coyoteUsable;
        private bool _endedJumpEarly = true;
        private float _apexPoint; // Becomes 1 at the apex of a jump
        private float _lastJumpPressed;




        private bool CanUseCoyote => _coyoteUsable && !isGrounded && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;
        private bool HasBufferedJump => isGrounded && _lastJumpPressed + _jumpBuffer > Time.time;

        private void CalculateJumpApex()
        {
            if (!isGrounded)
            {
                // Gets stronger the closer to the top of the jump
                _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
                _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
            }
            else
            {
                _apexPoint = 0;
            }
        }

        private void CalculateJump()
        {
            // Jump if: grounded or within coyote threshold || sufficient jump buffer
            if (Input.jumpPressed && CanUseCoyote || HasBufferedJump)
            {
                verticalVel = _jumpHeight;
                _endedJumpEarly = false;
                _coyoteUsable = false;
                _timeLeftGrounded = float.MinValue;
                JumpingThisFrame = true;
            }
            else
            {
                JumpingThisFrame = false;
            }

            // End the jump early if button released
            if (!isGrounded && Input.jumpReleased && !_endedJumpEarly && Velocity.y > 0)
            {
                // _currentVerticalSpeed = 0;
                _endedJumpEarly = true;
            }
        }

        
        
        #endregion





    }
}
