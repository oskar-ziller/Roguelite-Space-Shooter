using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteorGame
{
    public class SlotLinkUI : MonoBehaviour
    {

        #region Variables

        public int linkNo;

        [Header("Images")]
        [SerializeField] private Image overlay;
        [SerializeField] private Image circleIcon;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI nameTmp;
        [SerializeField] private TextMeshProUGUI emptyTmp;


        [SerializeField] private int unlockCost;

        [Tooltip("Object to display when slot is not yet unlocked")]
        [SerializeField] private GameObject lockedObj;


        [SerializeField] private Button levelUpButton;

        [Tooltip("TooltipTrigger for showing gem info.")]
        [SerializeField] private TooltipTrigger slotTooltipTrigger;


        public int UnlockCost => unlockCost;

        private SlotManagerUI slotManager;

        private bool empty = false;
        private bool locked = false;

        private GemItem gem;
        private TabMenuManager tabMenuManager;

        private bool isSetup = false;

        #endregion

        #region Unity Methods


        private void Awake()
        {

        }



        #endregion

        #region Methods


        private void Setup()
        {
            slotManager = GetComponentInParent<SlotManagerUI>();
            tabMenuManager = GetComponentInParent<TabMenuManager>();

            if (lockedObj != null)
            {
                lockedObj.GetComponent<TooltipTrigger>().infoText = "Unlock for " + unlockCost.ToString();
            }

            isSetup = true;
        }


        public void UpdateUI()
        {
            if (!isSetup)
            {
                Setup();
            }

            if (slotManager.ownerSlot.MaxLinksUnlocked == linkNo)
            {
                Lock();
                return;
            }
            else
            {
                Unlock();
            }

            bool isEmpty = slotManager.ownerSlot.Linked.Count <= linkNo;

            if (isEmpty)
            {
                Empty();
            }
            else
            {
                NotEmpty();
                SetTextAndColors();

                var linked = slotManager.ownerSlot.Linked;
                GemItem g = linked[linkNo];
                slotTooltipTrigger.SetupGemInfoTooltip(g);
            }
        }


        public void OnClicked()
        {
            if (!locked && !empty)
            {
                slotManager.ownerSlot.RemoveLinked(gem);
            }
        }

        public void OnUnlockClicked()
        {
            slotManager.OnUnlockClickedLink(this);

        }

        private void Lock()
        {
            levelUpButton.gameObject.SetActive(false);
            emptyTmp.enabled = false;
            lockedObj.SetActive(true);
            slotTooltipTrigger.enabled = false;
            locked = true;
        }
        private void Unlock()
        {
            lockedObj.SetActive(false);
            locked = false;
        }

        private void Empty()
        {
            levelUpButton.gameObject.SetActive(false);
            circleIcon.enabled = false;
            overlay.enabled = false;
            nameTmp.enabled = false;
            emptyTmp.enabled = true;
            empty = true;
            slotTooltipTrigger.enabled = false;
            gem = null;
        }

        private void NotEmpty()
        {
            levelUpButton.gameObject.SetActive(true);
            empty = false;
            emptyTmp.enabled = false;
            slotTooltipTrigger.enabled = true;

            var linked = slotManager.ownerSlot.Linked;
            GemItem g = linked[linkNo];
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
            
            circleIcon.enabled = true;
            overlay.enabled = true;

            circleIcon.color = gem.Color;
            overlay.color = gem.Color;

            nameTmp.text = gem.Name + $" ({gem.Level})";
        }

        public void SetUnlockCost(int amount)
        {
            unlockCost = amount;
        }


        public void TryLevelup()
        {
            if (gem.Level >= GemItem.MaxGemLevel)
            {
                tabMenuManager.DisplayError(UIError.GemAlreadyMaxLevel);
                return;
            }

            bool res = slotManager.TryBuy(gem.LevelUpCost);

            if (res)
            {
                slotManager.ownerSlot.Levelup(gem);
            }
        }

        #endregion

    }
}
