using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class SlotManagerUI : MonoBehaviour
    {

        #region Variables

        public SpellSlot ownerSlot { get; private set; }
        private SlotHeaderUI header;
        private List<SlotLinkUI> links = new List<SlotLinkUI>();
        private TabMenuManager tabMenuManager;
        private RectTransform rectTransform;

        public int slotNo;
        public SlotLinkUI prefab;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ownerSlot = Player.Instance.SpellSlot(slotNo);
            tabMenuManager = GetComponentInParent<TabMenuManager>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            CreateSlotLinkUIs();
        }

        private void Update()
        {
            DeactivateUnusedLinkSlots();
        }

        #endregion

        #region Methods


        private void CreateSlotLinkUIs()
        {
            for (int i = 0; i < GameManager.Instance.MaxLinks; i++)
            {
                var slotLinkUI = Instantiate(prefab);
                slotLinkUI.transform.SetParent(this.transform);
                slotLinkUI.linkNo = i;
                links.Add(slotLinkUI);

                var rt = slotLinkUI.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;

            }
        }

        private void DeactivateUnusedLinkSlots()
        {
            var max = ownerSlot.MaxLinks;

            if (!ownerSlot.IsUnlocked)
            {
                max = -1;
            }

            foreach (SlotLinkUI link in links)
            {
                if (link.linkNo <= max)
                {
                    link.gameObject.SetActive(true);
                }
                else
                {
                    link.gameObject.SetActive(false);
                }
            }
        }

        private bool TryBuy(int cost)
        {
            if (Player.Instance.CanAfford(cost))
            {
                Player.Instance.ChangeCurrency(-cost);
                return true;
            }
            else
            {
                tabMenuManager.DisplayError(UIError.CantAfford);
                return false;
            }
        }

        public void OnUnlockClickedSlotHeader(SlotHeaderUI slotHeaderUI)
        {
            bool res = TryBuy(slotHeaderUI.UnlockCost);

            if (res)
            {
                ownerSlot.UnlockSpellSlot();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        // Called when a SlotLinkUI is clicked when it's in locked state
        public void OnUnlockClickedLink(SlotLinkUI slotLinkUI)
        {
            //var cost = slotLinkUI.UnlockCost;

            //if (Player.Instance.CanAfford(cost))
            //{
            //    Player.Instance.ChangeCurrency(-cost);
            //    ownerSlot.IncreaseMaxLinks();
            //    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            //}
            //else
            //{
            //    tabMenuManager.DisplayError(UIError.CantAfford);
            //}


            bool res = TryBuy(slotLinkUI.UnlockCost);

            if (res)
            {
                ownerSlot.IncreaseMaxLinks();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }


        #endregion

    }
}
