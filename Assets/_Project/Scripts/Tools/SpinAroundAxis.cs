using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace MeteorGame
{
    public class SpinAroundAxis : MonoBehaviour
    {
        public float spinDurInSeconds = 10f;
        public Ease easing = Ease.Linear;

        private void StartSpin()
        {
            transform.DOLocalRotate(new Vector3(360, 0, 0), spinDurInSeconds, RotateMode.LocalAxisAdd).SetEase(easing).SetLoops(-1);
        }

        private void Start()
        {
            StartSpin();
        }

        private void OnValidate()
        {
            transform.DOKill();
            StartSpin();
        }
    }
}
