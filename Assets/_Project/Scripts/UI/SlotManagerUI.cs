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

        [Tooltip("Unlock cost for each link")]
        [SerializeField] private List<int> unlockCosts = new List<int>();


        private bool isSetup = false;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            
        }

        private void Start()
        {
        }

        private void Update()
        {
        }

        #endregion

        #region Methods


        public void Setup()
        {
            ownerSlot = Player.Instance.SpellSlot(slotNo);
            tabMenuManager = GetComponentInParent<TabMenuManager>();
            rectTransform = GetComponent<RectTransform>();
            header = GetComponentInChildren<SlotHeaderUI>();

            isSetup = true;
        }


        /// <summary>
        /// If empty, creates GameManager.MaxLinksAllowed amount of links.
        /// 
        /// If not empty calls update for header and non deactivated links.
        /// </summary>
        public void UpdateUI()
        {
            if (!isSetup)
            {
                Setup();
            }

            if (links.Count == 0)
            {
                CreateSlotLinkUIs();
            }

            DeactivateUnusedLinkSlots();

            header.UpdateUI();


            foreach (SlotLinkUI item in links)
            {
                item.UpdateUI();
            }
        }

        private void CreateSlotLinkUIs()
        {
            for (int i = 0; i < GameManager.Instance.MaxLinksAllowed; i++)
            {
                SlotLinkUI slotLinkUI = Instantiate(prefab);
                slotLinkUI.transform.SetParent(this.transform);
                slotLinkUI.linkNo = i;
                slotLinkUI.SetUnlockCost(unlockCosts[i]);
                links.Add(slotLinkUI);

                var rt = slotLinkUI.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
            }
        }

        private void DeactivateUnusedLinkSlots()
        {
            var max = ownerSlot.MaxLinksUnlocked;

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

        public bool TryBuy(int cost)
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
                UpdateUI();
            }
        }

        // Called when a SlotLinkUI is clicked in locked state
        public void OnUnlockClickedLink(SlotLinkUI slotLinkUI)
        {
            bool res = TryBuy(slotLinkUI.UnlockCost);

            if (res)
            {
                ownerSlot.IncreaseMaxLinks();
                UpdateUI();
            }

        }


        #endregion

    }
}
