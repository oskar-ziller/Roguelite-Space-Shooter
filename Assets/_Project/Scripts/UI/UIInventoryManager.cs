using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class UIInventoryManager : MonoBehaviour
    {

        #region Variables

        public InventoryItem itemPrefab;
        public RectTransform holder;

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

        public void Rebuild()
        {
            ClearInventory();
            FillInventory();
        }

        private void ClearInventory()
        {
            foreach (RectTransform child in holder)
            {
                Destroy(child.gameObject);
            }
        }

        private void FillInventory()
        {
            // first spells
            foreach (GemItem g in Player.Instance.Inv.Gems)
            {
                if (g.HasSpell && !g.IsEquipped)
                {
                    InventoryItem i = Instantiate(itemPrefab);
                    i.Init(g, holder);
                }
            }

            foreach (GemItem g in Player.Instance.Inv.Gems)
            {
                if (!g.HasSpell && !g.IsEquipped)
                {
                    InventoryItem i = Instantiate(itemPrefab);
                    i.Init(g, holder);
                }
            }

        }

        #endregion

    }
}
