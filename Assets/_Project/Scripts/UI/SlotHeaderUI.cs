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

        [Tooltip("Object to display when slot is not yet unlocked. " +
            "Only SLOT2 has this since SLOT1 is unlocked from start.")]
        [SerializeField] private GameObject lockedObj;  

        [SerializeField] private Button levelUpButton;

        [Tooltip("TooltipTrigger for showing gem info.")]
        [SerializeField] private TooltipTrigger headerTooltipTrigger;

        public int UnlockCost => unlockCost;


        private TabMenuManager tabMenuManager;
        private SlotManagerUI manager;

        private bool empty = false;
        private GemItem gem; // gem slotted at header (spell)

        private bool isSetup = false;

        #endregion


        #region Unity Methods


        private void Setup()
        {
            if (lockedObj != null) // Only SLOT2 has lockedObj since SLOT1 is unlocked from start
            {
                lockedObj.GetComponent<TooltipTrigger>().infoText += unlockCost.ToString();
            }

            manager = GetComponentInParent<SlotManagerUI>();
            tabMenuManager = GetComponentInParent<TabMenuManager>();

            if (locked)
            {
                lockedObj.SetActive(true);

                headerTooltipTrigger.enabled = false;

                nameTmp.enabled = false;
                emptyTmp.enabled = false;

                squareIcon.enabled = false;
                overlay.enabled = true;
            }

            isSetup = true;
        }



        public void UpdateUI()
        {
            if (!isSetup)
            {
                Setup();
            }

            if (locked)
            {
                if (manager.ownerSlot.IsUnlocked)
                {
                    Unlock();
                }
            }

            if (!locked)
            {
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
                    headerTooltipTrigger.SetupGemInfoTooltip(g);
                }
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
            headerTooltipTrigger.enabled = true;
            nameTmp.enabled = true;
            emptyTmp.enabled = true;
            squareIcon.enabled = true;
            overlay.enabled = true;
            locked = false;
        }

        private void Empty()
        {
            levelUpButton.gameObject.SetActive(false);

            squareIcon.enabled = false;
            overlay.enabled = false;

            nameTmp.enabled = false;
            emptyTmp.enabled = true;

            headerTooltipTrigger.enabled = false;

            slotBG.color = new Color(0, 0, 0, 0.023f);

            empty = true;
            gem = null;

        }

        private void NotEmpty()
        {
            levelUpButton.gameObject.SetActive(true);

            overlay.enabled = true;
            nameTmp.enabled = true;
            emptyTmp.enabled = false;
            headerTooltipTrigger.enabled = true;
            squareIcon.enabled = true;
            empty = false;


            GemItem g = manager.ownerSlot.Spell.Gem;
            gem = g;

            UpdateLevelUpButtonTooltip();
        }


        private void UpdateLevelUpButtonTooltip()
        {
            TooltipTrigger tooltipTrigger = levelUpButton.gameObject.GetComponent<TooltipTrigger>();
            tooltipTrigger.infoText = "Level up for " + gem.LevelUpCost;
        }

        private void SetTextAndColors()
        {
            nameTmp.enabled = true;

            squareIcon.enabled = true;
            overlay.enabled = true;

            overlay.color = new Color(gem.Color.r, gem.Color.g, gem.Color.b, overlayOpacity);
            slotBG.color = new Color(gem.Color.r, gem.Color.g, gem.Color.b, 0.023f);

            squareIcon.color = gem.Color;

            nameTmp.text = gem.Name + $" ({gem.Level})";
        }

        public void TryLevelup()
        {
            if (gem.Level >= GemItem.MaxGemLevel)
            {
                tabMenuManager.DisplayError(UIError.GemAlreadyMaxLevel);
                return;
            }

            bool res = manager.TryBuy(gem.LevelUpCost);

            if (res)
            {
                manager.ownerSlot.Levelup(gem);
            }
        }

        #endregion
    }
}
