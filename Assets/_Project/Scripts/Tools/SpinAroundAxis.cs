using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace MeteorGame
{
    public class SpinAroundAxis : MonoBehaviour
    {
        public float spinDurInSeconds = 10f;

        [Tooltip("Randomization that will be added to spin dur")]
        public float spinDurVariance = 0f;

        [Tooltip("Spin direction")]
        public bool clockWise = true;

        [Tooltip("Randomize spin direction")]
        public bool randomDirection = false;

        [Tooltip("Random starting rotation")]
        public bool randomizeStartingRot = false;

        public Ease easing = Ease.Linear;

        [Header("Spin axis")]
        public bool xAxis, yAxis, zAxis;

        public void StopSpin()
        {
            transform.DOKill();
        }

        public void StartSpin()
        {
            if (randomizeStartingRot)
            {
                RandomizeStartingRot();
            }

            int x = xAxis ? 360 : 0;
            int y = yAxis ? 360 : 0;
            int z = zAxis ? 360 : 0;

            if (!clockWise)
            {
                x = -x;
                y = -y;
                z = -z;
            }

            if (randomDirection) // coin toss for reversing direction
            {
                x = Random.value > 0.5f ? -x : x;
                y = Random.value > 0.5f ? -y : y;
                z = Random.value > 0.5f ? -z : z;
            }

            transform.DOLocalRotate(new Vector3(x, y, z), spinDurInSeconds, RotateMode.LocalAxisAdd).SetEase(easing).SetLoops(-1);
        }

        private void RandomizeStartingRot()
        {
            int x = xAxis ? Random.Range(0,360) : 0;
            int y = yAxis ? Random.Range(0, 360) : 0;
            int z = zAxis ? Random.Range(0, 360) : 0;

            transform.localRotation = Quaternion.Euler(x, y, z);
        }

        private void Start()
        {
            StartSpin();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                StopSpin();
                StartSpin();
            }
        }
    }
}
