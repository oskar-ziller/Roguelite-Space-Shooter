using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame.UI
{
    public class CurrencyUI : MonoBehaviour
    {

        #region Variables
        [SerializeField] private TextMeshProUGUI cucrrencyTMP;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            Player.Instance.OnCurrencyChanged += UpdateUI;
        }

        #endregion


        private void UpdateUI(int newCurrency)
        {
            Debug.Log("new: " + newCurrency);
            string currency = string.Format("{0:00000}", Player.Instance.TweeningCurrency);
            cucrrencyTMP.text = currency;
        }

    }
}
