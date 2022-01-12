using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class SlotHeaderUI : MonoBehaviour
    {

        #region Variables

        private TextMeshProUGUI nameTmp;
        public Image overlay;

        private SlotManagerUI owner;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            nameTmp = GetComponentInChildren<TextMeshProUGUI>();
            owner = GetComponentInParent<SlotManagerUI>();
        }

        private void Start()
        {
        
        }

        private void Update()
        {
            if (owner.ownerSlot.Spell == null)
            {
                overlay.color = new Color(0, 0, 0, 0);
                nameTmp.text = "- EMPTY - ";
            }
            else
            {
                overlay.color = owner.ownerSlot.Spell.Gem.Color;
                nameTmp.text = owner.ownerSlot.Spell.Gem.Name;
            }
        }

        #endregion

        #region Methods

        #endregion

    }
}
