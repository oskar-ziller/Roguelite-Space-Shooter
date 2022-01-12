using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MeteorGame
{
    public class WandAnim : MonoBehaviour
    {

        #region Variables

        public int belongsToSlot;
        Tween idleTween;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {
            Idle();
        }

        private void Update()
        {
            //transform.rotation = transform.rotation * Quaternion.Euler(0, 1, 0);
        }


        public void Idle()
        {
            idleTween.Kill();
            idleTween = transform.DOLocalRotate(new Vector3(0, 360, 0), 10, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).OnComplete(Idle);
        }

        public void Shoot(float dur)
        {
            idleTween.Kill();
            transform.DOLocalRotate(new Vector3(0, 180, 0), dur, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuint).OnComplete(Idle);
        }

        #endregion

        #region Methods

        #endregion

    }
}
