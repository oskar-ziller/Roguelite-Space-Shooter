using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{

    [System.Serializable]
    public class Inventory
    {
        [SerializeField] private List<GemItem> gems = new List<GemItem>();

        public List<GemItem> Gems => gems;

        public List<SpellItem> Spells => GetSpells();

        //public event Action<GemItem> GemAdded;

        public Inventory()
        {
        }

        public void AddGem(GemItem g)
        {
            gems.Add(g);
            //GemAdded?.Invoke(g);
        }

        private List<SpellItem> GetSpells()
        {
            var toReturn = new List<SpellItem>();

            foreach (var g in gems)
            {
                if (g.HasSpell)
                {
                    toReturn.Add(g.Spell);
                }
            }

            return toReturn;
        }


    }
}
