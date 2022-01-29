using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class LevelUpButton : MonoBehaviour
    {

        #region Variables

        private SlotLinkUI ownerSlotLink;
        private SlotHeaderUI ownerSlotHeader;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ownerSlotLink = GetComponentInParent<SlotLinkUI>();
            ownerSlotHeader = GetComponentInParent<SlotHeaderUI>();
        }


        #endregion

        #region Methods

        public void OnLevelUpClicked()
        {
            if (ownerSlotLink != null)
            {
                ownerSlotLink.TryLevelup();
                return;
            }

            if (ownerSlotHeader != null)
            {
                ownerSlotHeader.TryLevelup();
                return;
            }
        }

        #endregion

    }
}
