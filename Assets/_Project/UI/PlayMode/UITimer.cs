using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame
{
    public class UITimer : MonoBehaviour
    {
        private float startTime;
        public TextMeshProUGUI minutesT, minutesT2;
        public TextMeshProUGUI secondsT, secondsT2;
        public TextMeshProUGUI millisecondsT, millisecondsT2;


        #region Variables

        #endregion

        #region Unity Methods

        private void Awake()
        {

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

            float t = (float)Math.Round(GameManager.Instance.HowFarIntoDifficulty().TotalSeconds, 2);

            int totalSeconds = (int)((t * 100) / 100);
            float millisecondsF = (float)Math.Round(t - totalSeconds, 2) * 100;

            string minutes = ((int)totalSeconds / 60).ToString("00");
            string seconds = (totalSeconds % 60).ToString("00");
            string milliseconds = millisecondsF.ToString("00");

            minutesT.text = minutes;
            secondsT.text = seconds;
            millisecondsT.text = milliseconds;

            minutesT2.text = minutesT.text;
            secondsT2.text = secondsT.text;
            millisecondsT2.text = millisecondsT.text;
        }

        #endregion

        #region Methods



        #endregion

    }
}
