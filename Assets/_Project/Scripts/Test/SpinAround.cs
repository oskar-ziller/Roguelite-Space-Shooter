using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MeteorGame
{
    public class SpinAround : MonoBehaviour
    {
        private float moveRange;
        private Vector3 speed = Vector3.one;
        private Vector3 progression;

        public float minSpeed, maxSpeed, minRange, maxRange;

        private void OnEnable()
        {
            progression += Vector3.one * Random.Range(0, 100);

            var x = Random.Range(minSpeed, maxSpeed);
            var y = Random.Range(minSpeed, maxSpeed);
            var z = Random.Range(minSpeed, maxSpeed);

            speed = new Vector3(x, y, z);

            moveRange = Random.Range(minRange, maxRange);
            moveRange = 0.1f;
        }

        private void UpdateRotation()
        {
            var x = Mathf.Sin(progression.x);
            var y = Mathf.Cos(progression.y);
            var z = Mathf.Cos(progression.z) + Mathf.Sin(progression.z);

            transform.rotation = Quaternion.Euler(x, y, z);
        }

        void UpdatePosition()
        {
            var x = Mathf.Sin(progression.x);
            var y = Mathf.Cos(progression.y);
            var z = Mathf.Cos(progression.z) + Mathf.Sin(progression.z);

            transform.localPosition = moveRange * new Vector3(x, y, z);
        }

        void Update()
        {
            //UpdateRotation();
            UpdatePosition();
            progression += Time.deltaTime * speed;
        }
    }
}