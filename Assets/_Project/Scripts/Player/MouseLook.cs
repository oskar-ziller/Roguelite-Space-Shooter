using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class MouseLook : MonoBehaviour
    {

        #region Variables

        // This enumeration describes which directions this script should control
        [Flags]
        public enum RotationDirection
        {
            None,
            Horizontal = (1 << 0),
            Vertical = (1 << 1)
        }

        [Tooltip("Which directions this object can rotate")]
        [SerializeField] private RotationDirection rotationDirections;

        [Tooltip("The rotation acceleration, in degrees / second")]
        [SerializeField] private Vector2 acceleration;

        [Tooltip("A multiplier to the input. Describes the maximum speed in degrees / second. To flip vertical rotation, set Y to a negative value")]
        [SerializeField] private Vector2 sensitivity;

        [Tooltip("The maximum angle from the horizon the player can rotate, in degrees")]
        [SerializeField] private float maxVerticalAngleFromHorizon;

        [Tooltip("The period to wait until resetting the input value. Set this as low as possible, without encountering stuttering")]
        [SerializeField] private float inputLagPeriod;

        [Tooltip("Player object we want to rotate horizontally")]
        [SerializeField] private PlayerController player;

        private Vector2 velocity; // The current rotation velocity, in degrees
        private Vector2 rotation; // The current rotation, in degrees
        private Vector2 lastInputEvent; // The last received non-zero input value
        private float inputLagTimer; // The time since the last received non-zero input value

        #endregion

        #region Unity Methods

        // When this component is enabled, we need to reset the state
        // and figure out the current rotation
        private void OnEnable()
        {
            // Reset the state
            velocity = Vector2.zero;
            inputLagTimer = 0;
            lastInputEvent = Vector2.zero;

            // Calculate the current rotation by getting the gameObject's local euler angles
            Vector3 euler = transform.localEulerAngles;
            // Euler angles range from [0, 360), but we want [-180, 180)
            if (euler.x >= 180)
            {
                euler.x -= 360;
            }

            euler.x = ClampVerticalAngle(euler.x);
            // Set the angles here to clamp the current rotation
            transform.localEulerAngles = euler;
            // Rotation is stored as (horizontal, vertical), which corresponds to the euler angles
            // around the y (up) axis and the x (right) axis
            rotation = new Vector2(euler.y, euler.x);
        }

        private void Awake()
        {

        }

        private void Start()
        {
        }

        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            Vector2 wantedVelocity = GetInput() * sensitivity;

            rotation += wantedVelocity * Time.deltaTime;
            player.TurnHorizontal((wantedVelocity * Time.deltaTime).x); // rotate player body

            rotation.y = ClampVerticalAngle(rotation.y);

            transform.localEulerAngles = new Vector3(rotation.y, 0, 0);
        }

        #endregion

        #region Methods

        private float ClampVerticalAngle(float angle)
        {
            return Mathf.Clamp(angle, -maxVerticalAngleFromHorizon, maxVerticalAngleFromHorizon);
        }

        private Vector2 GetInput()
        {
            // Add to the lag timer
            inputLagTimer += Time.deltaTime;
            // Get the input vector. This can be changed to work with the new input system or even touch controls
            Vector2 input = new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );

            // Sometimes at fast framerates, Unity will not receive input events every frame, which results
            // in zero values being given above. This can cause stuttering and make it difficult to fine
            // tune the acceleration setting. To fix this, disregard zero values. If the lag timer has passed the
            // lag period, we can assume that the user is not giving any input, so we actually want to set
            // the input value to zero at that time.
            // Thus, save the input value if it is non-zero or the lag timer is met
            if ((Mathf.Approximately(0, input.x) && Mathf.Approximately(0, input.y)) == false || inputLagTimer >= inputLagPeriod)
            {
                lastInputEvent = input;
                inputLagTimer = 0;
            }

            return lastInputEvent;
        }

        #endregion

    }
}
