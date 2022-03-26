using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Flight
{
    public class PhysicsFlight : MonoBehaviour
    {

        #region Variables

        [Header("ACCEL")]
        [SerializeField] private float accel = 1f;
        [SerializeField] private float decel = 10f;

        [Header("BOOST")]
        [Tooltip("Max speed multiplier wheen boost key is down, higher value = faster when boosting")]
        [SerializeField] private float boostMultipMaxVel = 2f;
        [Tooltip("Accel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipAccel = 2f;

        [Tooltip("Decel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipDec = 2f;


        [Header("SPEED")]
        [SerializeField] private float maxSpeed = 5f;


        private FrameInput inputs;
        private float boostAccel = 1f;
        private float boostDec = 1f;
        private float boostSpeed = 1f;
        private Rigidbody rigidbody;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
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
            rigidbody.AddForce(transform.forward * maxSpeed * inputs.z);
        }



        #endregion

        #region Methods

        private void GatherInput()
        {
            inputs = new FrameInput
            {
                boostDown = Input.GetKey(KeyCode.LeftShift),
                x = Input.GetAxisRaw("Horizontal"),
                z = Input.GetAxisRaw("Vertical"),
                jump = Input.GetAxisRaw("Jump")
            };


            boostAccel = 1f;
            boostDec = 1f;
            boostSpeed = 1f;

            if (inputs.boostDown)
            {
                boostAccel = boostMultipAccel;
                boostDec = boostMultipDec;
                boostSpeed = boostMultipMaxVel;
            }

            Vector3 locVel = transform.InverseTransformDirection(rigidbody.velocity);

            float x = CalculateRight(locVel);
            float y = CalculateUp(locVel);
            float z = CalculateForward(locVel);

            Vector3 v = new(x, y, z);

            if (v.magnitude > maxSpeed * boostSpeed)
            {
                v = Vector3.MoveTowards(v, v.normalized * maxSpeed * boostSpeed, accel * boostAccel * Time.fixedDeltaTime);
            }

            //rigidbody.velocity = new Vector3(x, y, z);
            rigidbody.velocity = transform.TransformDirection(v);
        }

        private float CalculateRight(Vector3 locVel)
        {
            float targetSpeed = inputs.inputVector.x * maxSpeed * boostSpeed;
            float acc = accel * boostAccel;

            if (Math.Abs(locVel.x) > Math.Abs(targetSpeed))
            {
                acc = decel * boostDec;
            }

            return Mathf.MoveTowards(locVel.x, targetSpeed, acc * Time.fixedDeltaTime);
        }

        private float CalculateUp(Vector3 locVel)
        {
            float targetSpeed = inputs.jump * maxSpeed * boostSpeed;
            float acc = accel * boostAccel;

            if (inputs.jump == 0)
            {
                acc = decel * boostDec;
            }

            return Mathf.MoveTowards(locVel.y, targetSpeed, acc * Time.fixedDeltaTime);
        }

        private float CalculateForward(Vector3 locVel)
        {
            float targetSpeed = inputs.inputVector.y * maxSpeed * boostSpeed;
            float acc = accel * boostAccel;

            if (Math.Abs(locVel.z) > Math.Abs(targetSpeed))
            {
                acc = decel * boostDec;
            }

            return Mathf.MoveTowards(locVel.z, targetSpeed, acc * Time.fixedDeltaTime);
        }

        #endregion

    }
}
