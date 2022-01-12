using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{ 

    public class TabMenuManager : MonoBehaviour
    {
        public UIInventoryManager uiInventoryManager;

        private ErrorDisplayer errorDisplayer;
        private UIInventoryManager invManager;

        private bool isShowing = false;

        private void Awake()
        {
            errorDisplayer = GetComponentInChildren<ErrorDisplayer>();
            invManager = GetComponentInChildren<UIInventoryManager>();
            RebuildInventoryUI();
        }

        public void DisplayError(UIError error)
        {
            errorDisplayer.DisplayError(error);
        }

        public void RebuildInventoryUI()
        {
            if (invManager != null)
            {
                invManager.Rebuild();
            }
        }
    }
}
