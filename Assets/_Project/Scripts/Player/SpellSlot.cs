using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    [System.Serializable]
    public class SpellSlot
    {
        private List<GemItem> gems = new List<GemItem>();

        public int MaxLinks { get; private set; }

        public bool CanLinkMore => CanLink();

        public SpellItem Spell { get; private set; }
        public List<GemItem> Linked => gems;


        public Action<SpellSlot, GemItem> GemAddedOrRemoved;
        public Action<SpellSlot, SpellItem> SpellChanged;

        public int slotNo;

        public SpellSlot(int slotNr)
        {
            MaxLinks = 0;
            slotNo = slotNr;
        }

        private void AddLinked(GemItem gem)
        {
            if (!CanLinkMore)
            {
                return;
            }

            gems.Add(gem);
            gem.Equip();
            GemAddedOrRemoved?.Invoke(this, gem);
        }

        private void ChangeSpell(SpellItem spell)
        {
            if (Spell != null)
            {
                Spell.UnEquip();
                gems.Remove(spell.Gem);
            }

            Spell = spell;
            spell.Equip(this);
            SpellChanged?.Invoke(this, spell);
        }

        public void RemoveLinked(GemItem gem)
        {
            gem.UnEquip();
            gems.Remove(gem);
            GemAddedOrRemoved?.Invoke(this, gem);
        }

        private bool CanLink()
        {
           return gems.Count < MaxLinks;
        }

        public void Cast()
        {
            if (GameManager.Instance.IsGamePaused)
            {
                return;
            }

            SpellCaster.Cast(this);
        }

        public void IncreaseMaxLinks()
        {
            MaxLinks++;
        }

        internal void Equip(GemItem gem)
        {
            if (!gem.HasSpell)
            {
                AddLinked(gem);
            }
            else
            {
                ChangeSpell(gem.Spell);
            }
        }


    }
}
