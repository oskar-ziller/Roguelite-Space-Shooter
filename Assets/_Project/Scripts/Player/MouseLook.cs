using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class MouseLook : MonoBehaviour
    {
        [Tooltip("A multiplier to the input. Describes the maximum speed in degrees / second. To flip vertical rotation, set Y to a negative value")]
        [SerializeField] private Vector2 sensitivity;

        [Tooltip("The maximum angle from the horizon the player can rotate, in degrees")]
        [SerializeField] private float maxVerticalAngleFromHorizon;

        private Vector2 rotation; // The current rotation, in degrees
        private bool disabled = false;

        private float ClampVerticalAngle(float angle)
        {
            return Mathf.Clamp(angle, -maxVerticalAngleFromHorizon, maxVerticalAngleFromHorizon);
        }

        private Vector2 GetInput()
        {
            // Get the input vector. This can be changed to work with the new input system or even touch controls
            Vector2 input = new(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );

            return input;
        }

        private void FixedUpdate()
        {
            if (disabled)
            {
                return;
            }

            // The wanted velocity is the current input scaled by the sensitivity
            // This is also the maximum velocity
            Vector2 wantedVelocity = GetInput() * sensitivity;


            rotation += wantedVelocity * Time.deltaTime;
            rotation.y = ClampVerticalAngle(rotation.y);

            // Convert the rotation to euler angles
            transform.localEulerAngles = new Vector3(rotation.y, rotation.x, 0);
        }


        private void Start()
        {
            GameManager.Instance.GameOver += OnGameOver;
            Player.Instance.Ready += OnPlayerReady;
            Disable();
        }


        private void Disable()
        {
            disabled = true;
        }

        private void Enable()
        {
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

            disabled = false;
        }


        private void OnGameOver()
        {
            Disable();
        }

        private void OnPlayerReady()
        {
            Enable();
        }
    }
}
