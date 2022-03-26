using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MeteorGame.Flight
{
    public class FlightController : MonoBehaviour
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
        
        [Tooltip("Decel multiplier wheen boost key is down")]
        [SerializeField] private float boostMultipDec = 2f;


        [Header("SPEED")]
        [Range(0, 50)] [SerializeField] private float maxSpeed = 5f;


        private FrameInput inputs;
        private float boostAccel = 1f;
        private float boostDec = 1f;
        private float boostSpeed = 1f;

        private Vector3 frameTravelVec = Vector3.zero;

        private float maxSpeedReal => maxSpeed / 100f;

        public float Speed { get; private set; }


        #endregion

        #region Unity Methods

        private void Start()
        {
            GameManager.Instance.GameOver += OnGameOver;
        }

        private void Update()
        {
            if (GameManager.Instance.IsGamePaused)
            {
                return;
            }

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
            //var down = CalculateDown();


            //var v = new Vector3(fwd.x + rgh.x + up.x, fwd.y + rgh.y + up.y, fwd.z + rgh.z + up.z);
            var v = fwd + rgh + up;

            frameTravelVec += v;

            transform.position += frameTravelVec;

            Speed = frameTravelVec.magnitude;

            //Debug.Log("Speed: " + Speed);
        }




        #endregion

        #region Methods

        private void OnGameOver()
        {
            frameTravelVec = Vector3.zero;
        }

        private Vector3 CalculateUp()
        {
            var locVel = transform.InverseTransformDirection(frameTravelVec);
            var targetSpeed = inputs.jump * maxSpeedReal * boostSpeed;

            var accel = airAccel * boostAccel;

            if (inputs.jump == 0)
            {
                accel = airDecel * boostDec;
            }

            locVel.y = Mathf.MoveTowards(locVel.y, targetSpeed, accel * Time.deltaTime);

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - frameTravelVec;
        }


        private Vector3 CalculateForward()
        {
            var locVel = transform.InverseTransformDirection(frameTravelVec);
            var targetSpeed = inputs.inputVector.y * maxSpeedReal * boostSpeed;

            var accel = airAccel * boostAccel;

            if (Math.Abs(targetSpeed) < Math.Abs(locVel.z))
            {
                accel = airDecel * boostDec;
            }

            locVel.z = Mathf.MoveTowards(locVel.z, targetSpeed, accel * Time.deltaTime);

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - frameTravelVec;
        }


        private Vector3 CalculateRight()
        {
            var locVel = transform.InverseTransformDirection(frameTravelVec);
            var targetSpeed = inputs.inputVector.x * maxSpeedReal * boostSpeed;

            var accel = airAccel * boostAccel;

            if (Math.Abs(targetSpeed) < Math.Abs(locVel.x))
            {
                accel = airDecel * boostDec;
            }

            locVel.x = Mathf.MoveTowards(locVel.x, targetSpeed, accel * Time.deltaTime);

            var globalVel = transform.TransformDirection(locVel);

            return globalVel - frameTravelVec;
        }

        private void GatherInput()
        {
            inputs = new FrameInput
            {
                boostDown = Input.GetKey(KeyCode.LeftShift),
                x = Input.GetAxisRaw("Horizontal"),
                z = Input.GetAxisRaw("Vertical"),
                jump = Input.GetAxisRaw("Jump")
            };
        }

        public void TurnHorizontal(float amount)
        {
            transform.Rotate(Vector3.up, amount);
        }



        #endregion
    }

}


