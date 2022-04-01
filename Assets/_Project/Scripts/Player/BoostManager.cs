using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace MeteorGame.Flight
{
    public class BoostManager : MonoBehaviour
    {

        #region Variables
        [Tooltip("Boost duration in seconds")]
        [SerializeField] private float maxBoost = 2f;

        [Tooltip("Charge duration in seconds")]
        [SerializeField] private float chargeDuration = 2f;

        [Tooltip("Can boost only when full charge")]
        [SerializeField] private bool canOnlyBoostWhenFull = false;


        public float MaxBoost => maxBoost;
        public float BoostPercentage => remainingBoost / maxBoost;

        public bool IsBoosting { get; private set; }

        public event Action BoostAmountChanged;



        private float remainingBoost, remainingBoostTweening;
        private Tween tweenDown, tweenUp;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {
            GameManager.Instance.GameRestart += OnGameRestart;
            remainingBoost = maxBoost;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                BoostDown();
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                StopBoostAndStartRecharge();
            }
        }

        #endregion

        #region Methods

        private void ResetTweens()
        {
            if (tweenDown != null && tweenDown.active)
            {
                tweenDown.Kill();
                tweenDown = null;
            }

            if (tweenUp != null && tweenUp.active)
            {
                tweenUp.Kill();
                tweenUp = null;
            }
        }

        private void OnGameRestart()
        {
            ResetTweens();
            SetRemainingBoost(maxBoost);
        }

        public void BoostDown()
        {
            if (canOnlyBoostWhenFull)
            {
                if (remainingBoost == maxBoost)
                {
                    StartBoost();
                }
                else
                {
                    BoostNotReady();
                }
            }
            else
            {
                StartBoost();
            }
        }


        private void BoostNotReady()
        {
            Debug.Log("BoostNotReady");
        }

        private void BoostRecharged()
        {
            Debug.Log("BoostRecharged");
        }


        private void StartBoost()
        {
            IsBoosting = true;
            ResetTweens();
            float dur = maxBoost - (maxBoost - remainingBoost);

            remainingBoostTweening = remainingBoost;

            tweenDown = DOTween.To(() => remainingBoostTweening, x => remainingBoostTweening = x, 0, dur)
                .OnUpdate(() => SetRemainingBoost(remainingBoostTweening))
                .OnComplete(() => OnBoostDepleted());
        }

        public void StopBoostAndStartRecharge()
        {
            ResetTweens();
            float percentage = remainingBoost / maxBoost;
            float dur = chargeDuration * (1 - percentage);

            remainingBoostTweening = remainingBoost;

            tweenDown = DOTween.To(() => remainingBoostTweening, x => remainingBoostTweening = x, maxBoost, dur)
                .OnUpdate(() => SetRemainingBoost(remainingBoostTweening))
                .OnComplete(() => BoostRecharged());

            IsBoosting = false;
        }

        private void SetRemainingBoost(float amount)
        {
            remainingBoost = amount;
            BoostAmountChanged?.Invoke();
        }

        private void OnBoostDepleted()
        {
            Debug.Log("OnBoostDepleted");
            StopBoostAndStartRecharge();
        }

        #endregion

    }
}
