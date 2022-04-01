using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame.UI
{
    public class PlayTimeUI : MonoBehaviour
    {

        #region Variables

        [SerializeField] private TextMeshProUGUI minutes;
        [SerializeField] private TextMeshProUGUI seconds;

        #endregion

        #region Unity Methods

        private void Update()
        {
            var playTime = GameManager.Instance.PlayTime;

            string mins = string.Format("{0:00}", playTime.Minutes);
            string secs = string.Format("{0:00}", playTime.Seconds);

            minutes.text = mins;
            seconds.text = secs;
        }

        #endregion

    }
}
