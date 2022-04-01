using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame.UI
{
    public class DifficultyUI : MonoBehaviour
    {

        #region Variables
        [SerializeField] private TextMeshProUGUI difficultyTMP;
        #endregion

        #region Unity Methods

        private void Start()
        {
            GameManager.Instance.OnDifficultyChanged += UpdateUI;
        }

        #endregion

        #region Methods

        private void UpdateUI(int newDiff)
        {
            string diff = string.Format("{0:00}", newDiff);
            difficultyTMP.text = diff;
        }

        #endregion

    }
}
