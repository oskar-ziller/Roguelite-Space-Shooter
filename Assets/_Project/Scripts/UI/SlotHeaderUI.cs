using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    [RequireComponent(typeof(TooltipTrigger))]
    public class SlotHeaderUI : MonoBehaviour
    {

        #region Variables

        [Header("Images")]
        [SerializeField] private Image overlay;
        [SerializeField] private Image squareIcon;
        [SerializeField] private Image slotBG;
        [SerializeField] private float overlayOpacity;


        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI nameTmp;
        [SerializeField] private TextMeshProUGUI emptyTmp;

        [Header("Unlocking")]
        [SerializeField] private int unlockCost;
        [SerializeField] private bool locked = true;

        [Space(50)]

        [Tooltip("Object to display when slot is not yet unlocked")]
        [SerializeField] private GameObject lockedObj;

        public int UnlockCost => unlockCost;


        private TooltipTrigger slotTooltipTrigger;
        private SlotManagerUI manager;

        private bool empty = false;

        #endregion


        #region Unity Methods


        private void Start()
        {
            slotTooltipTrigger = GetComponent<TooltipTrigger>();

            if (lockedObj != null)
            {
                lockedObj.GetComponent<TooltipTrigger>().infoText += unlockCost.ToString();
            }

            manager = GetComponentInParent<SlotManagerUI>();

            if (locked)
            {
                lockedObj.SetActive(true);

                slotTooltipTrigger.enabled = false;

                nameTmp.enabled = false;
                emptyTmp.enabled = false;

                squareIcon.enabled = false;
                overlay.enabled = true;
            }
        }





        private void Update()
        {
            if (locked)
            {
                if (manager.ownerSlot.IsUnlocked)
                {
                    Unlock();
                }

                return;
            }

            bool isEmpty = manager.ownerSlot.Spell == null;

            if (isEmpty)
            {
                Empty();
            }
            else
            {
                NotEmpty();
                SetTextAndColors();

                GemItem g = manager.ownerSlot.Spell.Gem;
                slotTooltipTrigger.SetupGemInfoTooltip(g);
            }
        }

        public void OnClicked()
        {
            if (!locked && !empty)
            {
                manager.ownerSlot.RemoveSpell();
            }
        }

        #endregion

        #region Methods

        public void OnUnlockClicked()
        {
            manager.OnUnlockClickedSlotHeader(this);
        }


        private void Unlock()
        {
            lockedObj.SetActive(false);
            slotTooltipTrigger.enabled = true;
            nameTmp.enabled = true;
            emptyTmp.enabled = true;
            squareIcon.enabled = true;
            overlay.enabled = true;
            locked = false;
        }

        private void Empty()
        {
            squareIcon.enabled = false;
            overlay.enabled = false;

            nameTmp.enabled = false;
            emptyTmp.enabled = true;

            slotTooltipTrigger.enabled = false;

            slotBG.color = new Color(0, 0, 0, 0.023f);

            empty = true;
        }

        private void NotEmpty()
        {
            overlay.enabled = true;
            nameTmp.enabled = true;
            emptyTmp.enabled = false;
            slotTooltipTrigger.enabled = true;
            squareIcon.enabled = true;
            empty = false;
        }

        private void SetTextAndColors()
        {
            nameTmp.enabled = true;
            GemItem g = manager.ownerSlot.Spell.Gem;

            squareIcon.enabled = true;
            overlay.enabled = true;

            overlay.color = new Color(g.Color.r, g.Color.g, g.Color.b, overlayOpacity);
            slotBG.color = new Color(g.Color.r, g.Color.g, g.Color.b, 0.023f);

            squareIcon.color = g.Color;
            //overlay.color = g.Color;

            nameTmp.text = g.Name;
        }

        #endregion
    }
}
