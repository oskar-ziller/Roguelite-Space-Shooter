using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI; // Required when using UI elements.

namespace MeteorGame
{
    public class DifficultyBodyMover : MonoBehaviour
    {

        #region Variables

        public RectTransform rect;

        private float startX;
        private float startY;

        //public Vector3 startPos, endPos;


        [Tooltip("How many times per second should position be updated")]
        private float updateInterval = 2f;

        //private Vector3 startPos;

        private float nextUpdateTime;

        private HorizontalLayoutGroup layoutGroup;


        #endregion

        #region Unity Methods

        private void Awake()
        {
        }

        private void Start()
        {
            //startY = rect.anchoredPosition.y;
            //startX = rect.anchoredPosition.x;
            //rect.anchoredPosition = startPos;
            nextUpdateTime = Time.time + (1f/updateInterval);
            layoutGroup = rect.gameObject.GetComponent<HorizontalLayoutGroup>();
            //rect.DOAnchorPos(endPos, GameManager.Instance.gameDurationMin * 60f);
        }

        private void Update()
        {
            if (Time.time > nextUpdateTime)
            {
                
                var minsToMax = GameManager.Instance.minutesToHitMaxGameLevel;
                var secondsSinceStart = (float)GameManager.Instance.HowFarIntoDifficulty().TotalSeconds;

                var percentage = secondsSinceStart / (minsToMax * 60);

                //var target = Helper.Map(secondsSinceStart, 0, minsToMax * 60, 0, -layoutGroup.preferredWidth);

                var target = percentage * -layoutGroup.preferredWidth;

                rect.anchoredPosition = new Vector3(target, 0, 0);

                nextUpdateTime = Time.time + (1f / updateInterval);
            }
            
        }

        private void FixedUpdate()
        {
            
        }

        #endregion

        #region Methods

        #endregion

    }
}
