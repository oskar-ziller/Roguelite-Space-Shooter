using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required when using UI elements.

namespace MeteorGame
{
    public class GameLevelText : MonoBehaviour
    {

        #region Variables

        private TextMeshProUGUI tmp;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
        
        }

        private void Update()
        {
            tmp.text = GameManager.Instance.GameLevel.ToString();
        }

        #endregion

        #region Methods

        #endregion

    }
}
