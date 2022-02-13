using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MeteorGame
{
    public class SpinAround : MonoBehaviour
    {
        private float moveRange;
        private Vector3 velocity = Vector3.one;
        private Vector3 progression;

        public float minSpeed, maxSpeed, minRange, maxRange;
        public float speed = 0, range = 0;

        public float distFromCenter = 0.11f;

        public float rotateSpeed = 2f;
        public float startDeg = 0f;

        private Vector3 offset = Vector3.zero;


        public int projID { get; set; }
        public int totalCount { get; set; }


        public void ResetSelf()
        {
            progression = Vector3.one * Random.value;

            CalculateOffset();
            RandomizeVel();
            RandomizeRange();
        }



        private void OnValidate()
        {
            //ResetSelf();
        }

        private void CalculateOffset()
        {
            if (totalCount == 1 || projID == 0)
            {
                offset = Vector3.zero;
                return;
            }

            var degBetweenn = 360f / totalCount;

            var eulerAmount = startDeg + (degBetweenn * (projID - 1));

            offset = Quaternion.Euler(0, 0, eulerAmount) * Vector3.up * distFromCenter * transform.localScale.x;
        }

        private void RandomizeProgress()
        {
            progression = Vector3.one * Random.Range(0, 100);
        }

        private void RandomizeRange()
        {
            moveRange = range == 0 ? Random.Range(minRange, maxRange) : range;
        }

        private void RandomizeVel()
        {
            var x = speed == 0 ? Random.Range(minSpeed, maxSpeed) : speed;
            var y = speed == 0 ? Random.Range(minSpeed, maxSpeed) : speed;
            var z = speed == 0 ? Random.Range(minSpeed, maxSpeed) : speed;

            velocity = new Vector3(x, y, z);
        }

        private void UpdateRotation()
        {
            var x = Mathf.Sin(progression.x);
            var y = Mathf.Cos(progression.y);
            var z = Mathf.Cos(progression.z) + Mathf.Sin(progression.z);

            transform.localRotation = Quaternion.Euler(x, y, z);
        }

        void UpdatePosition()
        {
            var x = Mathf.Sin(progression.x);
            var y = Mathf.Cos(progression.y);
            var z = Mathf.Cos(progression.z) + Mathf.Sin(progression.z);

            transform.localPosition = moveRange * new Vector3(x, y, z) + offset;
        }

        void Update()
        {
            //UpdateRotation();
            UpdatePosition();
            //UpdateRotation();

            CalculateOffset();
            //RandomizeVel();
            //RandomizeRange();

            progression += Time.deltaTime * velocity;
            startDeg += Time.deltaTime * rotateSpeed;
        }
    }
}