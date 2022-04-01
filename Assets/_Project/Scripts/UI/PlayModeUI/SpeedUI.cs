using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MeteorGame.UI
{
    public class SpeedUI : MonoBehaviour
    {

        #region Variables
        [SerializeField] private TextMeshProUGUI speedTMP;
        #endregion

        #region Unity Methods
        private void FixedUpdate()
        {
            UpdateUI();
        }

        #endregion

        #region Methods

        private void UpdateUI()
        {
            string speed = string.Format("{0:000}", Mathf.RoundToInt(Player.Instance.Speed));
            speedTMP.text = speed;
        }

        #endregion

    }
}
