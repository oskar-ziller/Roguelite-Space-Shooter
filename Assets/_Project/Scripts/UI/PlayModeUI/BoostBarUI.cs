using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame.UI
{
    public class BoostBarUI : MonoBehaviour
    {

        #region Variables
        
        private Slider boostSlider;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            boostSlider = GetComponent<Slider>();
        }

        private void Start()
        {
            Player.Instance.BoostManager.BoostAmountChanged += OnBoostAmountChanged;
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods

        private void OnBoostAmountChanged()
        {
            boostSlider.value = Player.Instance.BoostManager.BoostPercentage;
        }

        #endregion

    }
}
