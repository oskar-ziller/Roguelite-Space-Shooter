using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame
{
    public class CurrencyUI : MonoBehaviour
    {

        #region Variables
        [SerializeField] private TextMeshProUGUI cucrrencyTMP;
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
            string currency = string.Format("{0:00000}", Player.Instance.TweeningCurrency);
            cucrrencyTMP.text = currency;
        }

        #endregion

        #region Methods

        #endregion

    }
}
