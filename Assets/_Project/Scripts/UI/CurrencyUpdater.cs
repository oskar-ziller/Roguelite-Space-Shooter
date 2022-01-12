using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame
{
    public class CurrencyUpdater : MonoBehaviour
    {

        #region Variables
        private TextMeshProUGUI currencyTMP;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            currencyTMP = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
        
        }

        private void Update()
        {
            currencyTMP.text = Player.Instance.currency.ToString("N0");
        }

        #endregion

        #region Methods

        #endregion

    }
}
