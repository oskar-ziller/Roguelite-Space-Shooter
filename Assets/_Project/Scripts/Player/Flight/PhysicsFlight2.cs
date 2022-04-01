using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Flight
{
    public class PhysicsFlight2 : MonoBehaviour
    {

        #region Variables

        [SerializeField] private float accel = 200f;
        
        [Tooltip("Accel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipAccel = 2f;

        [Tooltip("Speed at which the maxDrag is applied")]
        [SerializeField] private float maxSpeed = 200f;

        [Tooltip("Max drag to apply at max speed")]
        [SerializeField] private float maxDrag = 2.5f;
        [Tooltip("Min drag to apply at zero speed")]
        [SerializeField] private float minDrag = 1f;


        private FrameInput inputs;
        private float boostAccel = 1f;
        private Rigidbody rigidbody;
        private BoostManager boostManager;
        private Transform flyerTransform;

        public float Speed => (float)Math.Round(rigidbody.velocity.magnitude, 3);

        #endregion

        #region Unity Methods

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            TryGetComponent(out boostManager);
            flyerTransform = rigidbody.transform;
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (GameManager.Instance.IsGamePaused)
            {
                return;
            }


        }

        private void FixedUpdate()
        {
            GatherInput();


            var fwd = flyerTransform.forward;
            var right = flyerTransform.right;
            var up = flyerTransform.up;

            ForceMode mode = ForceMode.Acceleration;
            rigidbody.AddForce(fwd * accel * boostAccel * inputs.inputVector.y, mode);
            rigidbody.AddForce(right * accel * boostAccel * inputs.inputVector.x, mode);
            rigidbody.AddForce(up * accel * boostAccel * inputs.jump, mode);

            float dragVal = Helper.Map(Speed, 0, maxSpeed, minDrag, maxDrag);

            rigidbody.drag = (float)Math.Round(dragVal, 3);
        }



        #endregion

        #region Methods

        private void GatherInput()
        {
            inputs = new FrameInput();

            if (boostManager != null)
            {
                inputs.isBoosting = boostManager.IsBoosting;
            }
            else
            {
                inputs.isBoosting = Input.GetKey(KeyCode.LeftShift);
            }

            inputs.x = Input.GetAxisRaw("Horizontal");
            inputs.z = Input.GetAxisRaw("Vertical");
            inputs.jump = Input.GetAxisRaw("Jump");

            boostAccel = 1f;

            if (inputs.isBoosting)
            {
                boostAccel = boostMultipAccel;
            }
        }
        #endregion

    }
}
