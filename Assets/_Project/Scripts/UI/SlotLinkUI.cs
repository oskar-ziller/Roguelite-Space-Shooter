using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteorGame
{
    [RequireComponent(typeof(TooltipTrigger))]
    public class SlotLinkUI : MonoBehaviour
    {

        #region Variables

        public int linkNo;

        [Header("Images")]
        [SerializeField] private Image overlay;
        [SerializeField] private Image circleIcon;
        [SerializeField] private Image squareIcon;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI nameTmp;
        [SerializeField] private TextMeshProUGUI emptyTmp;


        [SerializeField] private int unlockCost;

        [Tooltip("Object to display when slot is not yet unlocked")]
        [SerializeField] private GameObject lockedObj;

        public int UnlockCost => unlockCost;

        private TooltipTrigger slotTooltipTrigger;
        private SlotManagerUI manager;

        private bool empty = false;
        private bool locked = false;
        private bool isSpell = false;

        #endregion

        #region Unity Methods


        private void Start()
        {
            slotTooltipTrigger = GetComponent<TooltipTrigger>();
            manager = GetComponentInParent<SlotManagerUI>();

            if (lockedObj != null) // sadece slot2 lockedObj sahip
            {
                lockedObj.GetComponent<TooltipTrigger>().infoText += unlockCost.ToString();
            }
        }


        private void Update()
        {
            if (manager.ownerSlot.MaxLinks == linkNo)
            {
                Lock();
                return;
            }
            else
            {
                Unlock();
            }


            bool isEmpty = manager.ownerSlot.Linked.Count <= linkNo;

            if (isEmpty)
            {
                Empty();
            }
            else
            {
                NotEmpty();
                SetTextAndColors();

                var linked = manager.ownerSlot.Linked;
                GemItem g = linked[linkNo];
                slotTooltipTrigger.SetupGemInfoTooltip(g);
            }

        }

        public void OnClicked()
        {
            if (!locked && !empty)
            {
                GemItem g = manager.ownerSlot.Linked[linkNo];
                manager.ownerSlot.RemoveLinked(g);
            }
        }

        #endregion

        #region Methods

        public void OnUnlockClicked()
        {
            manager.OnUnlockClickedLink(this);
        }

        private void Lock()
        {
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
            circleIcon.enabled = false;
            overlay.enabled = false;
            nameTmp.enabled = false;
            emptyTmp.enabled = true;
            empty = true;
            slotTooltipTrigger.enabled = false;
        }

        private void NotEmpty()
        {
            empty = false;
            emptyTmp.enabled = false;
            slotTooltipTrigger.enabled = true;
        }

        private void SetTextAndColors()
        {
            nameTmp.enabled = true;
            var linked = manager.ownerSlot.Linked;
            GemItem g = linked[linkNo];

            circleIcon.enabled = true;
            overlay.enabled = true;

            circleIcon.color = g.Color;
            overlay.color = g.Color;

            nameTmp.text = g.Name;
        }

        #endregion

    }
}
