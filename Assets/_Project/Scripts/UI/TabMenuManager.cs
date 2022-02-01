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

        private void Awake()
        {
            errorDisplayer = GetComponentInChildren<ErrorDisplayer>();
            invManager = GetComponentInChildren<UIInventoryManager>();
            slotManagers = GetComponentsInChildren<SlotManagerUI>().ToList();
        }

        public void DisplayError(UIError error)
        {
            errorDisplayer.DisplayError(error);
        }

        public void RebuildTabMenu()
        {
            invManager.Rebuild();

            foreach (SlotManagerUI sm in slotManagers)
            {
                sm.UpdateUI();
            }

        }
    }
}
