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

        private SlotManagerUI manager;
        private TooltipTrigger trigger;


        #endregion

        #region Unity Methods

        private void Awake()
        {
            nameTmp = GetComponentInChildren<TextMeshProUGUI>();
            manager = GetComponentInParent<SlotManagerUI>();
            trigger = GetComponent<TooltipTrigger>();
        }

        private void Start()
        {
            manager.ownerSlot.SpellChanged += SpellChanged;
        }

        private void SpellChanged(SpellSlot _, SpellItem spell)
        {
            if (spell == null)
            {
                overlay.color = new Color(0, 0, 0, 0);
                nameTmp.text = "- EMPTY - ";
            }
            else
            {
                overlay.color = spell.Gem.Color;
                nameTmp.text = spell.Gem.Name;
                trigger.SetupGemInfoTooltip(spell.Gem);
            }
        }

        private void Update()
        {
            
        }

        #endregion

        #region Methods

        #endregion

    }
}
