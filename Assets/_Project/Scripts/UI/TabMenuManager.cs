using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{ 

    public class TabMenuManager : MonoBehaviour
    {
        public UIInventoryManager uiInventoryManager;

        private ErrorDisplayer errorDisplayer;
        private UIInventoryManager invManager;

        private List<SlotManagerUI> slotManagers;
        private bool isSetup;
        private bool isShowing;


        public bool IsShowing => isShowing;

        public void Setup()
        {
            errorDisplayer = GetComponentInChildren<ErrorDisplayer>();
            invManager = GetComponentInChildren<UIInventoryManager>();
            slotManagers = GetComponentsInChildren<SlotManagerUI>().ToList();
            isSetup = true;
        }

        public void DisplayError(UIError error)
        {
            errorDisplayer.DisplayError(error);
        }

        private void RebuildTabMenu()
        {
            invManager.Rebuild();

            foreach (SlotManagerUI slot in slotManagers)
            {
                slot.UpdateUI();
            }
        }

        public void Show()
        {
            if (!isSetup)
            {
                return;
            }

            gameObject.SetActive(true);

            GameManager.Instance.SetCursorMode(CursorLockMode.Confined);

            RebuildTabMenu();
            isShowing = true;
        }

        public void Hide()
        {
            GameManager.Instance.SetCursorMode(CursorLockMode.Locked);
            isShowing = false;
            gameObject.SetActive(false);
        }

        internal void TriedEquipInventoryItem()
        {
            RebuildTabMenu();
        }

        internal void TriedUnequipSlotLink()
        {
            RebuildTabMenu();
        }

        internal void TriedUnequipSpell()
        {
            RebuildTabMenu();
        }
    }
}
