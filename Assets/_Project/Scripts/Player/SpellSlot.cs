using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    [Serializable]
    public class SpellSlot
    {
        public SpellSlot(int slotNr)
        {
            MaxLinksUnlocked = 0;
            slotNo = slotNr;
            Modifiers = new Modifiers(this);
        }

        [SerializeField] private bool isUnlocked = false;

        private List<GemItem> gems = new();

        public int MaxLinksUnlocked { get; private set; }

        public bool CanLinkMore => CanLink();

        public SpellItem Spell { get; private set; }
        public List<GemItem> Linked => gems;

        public bool IsUnlocked => isUnlocked;

        public Action<SpellSlot, GemItem> GemLinkedOrRemoved;
        public Action<SpellSlot, SpellItem> SpellChanged;

        public int slotNo;

        public int ProjectileCount
        {
            get
            {
                return Spell.ProjectileCount + (int)GetTotal("AdditionalProjectiles");
            }
        }

        public Modifiers Modifiers;

        private void AddLinked(GemItem gem)
        {
            if (!CanLinkMore || !IsUnlocked)
            {
                return;
            }

            gems.Add(gem);
            gems = gems.OrderBy(g => g.Name).ToList();
            gem.Equip();
            Modifiers.Add(gem);
            GemLinkedOrRemoved?.Invoke(this, gem);
        }

        public void RemoveLinked(GemItem gem)
        {
            gem.UnEquip();
            gems.Remove(gem);
            gems = gems.OrderBy(g => g.Name).ToList();
            Modifiers.Remove(gem);
            GemLinkedOrRemoved?.Invoke(this, gem);
        }

        public void Levelup(GemItem gem)
        {
            UnEquip(gem);
            gem.LevelUp();
            Equip(gem);
            //GameManager.Instance.TabMenuManager.RebuildInventoryUI();
        }

        private void ChangeSpell(SpellItem spellToEquip)
        {
            if (Spell != null) // Spell == null when empty
            {
                Spell.UnEquip();
                Modifiers.Remove(Spell.Gem);
            }

            Spell = spellToEquip;

            if (spellToEquip != null)
            {
                gems.Remove(spellToEquip.Gem);
                spellToEquip.Equip(this);
                Modifiers.Add(spellToEquip.Gem);
            }

            SpellChanged?.Invoke(this, spellToEquip);
        }

        public void RemoveSpell()
        {
            ChangeSpell(null);
        }

        private bool CanLink()
        {
           return gems.Count < MaxLinksUnlocked;
        }

        public void Cast()
        {
            if (GameManager.Instance.IsGamePaused)
            {
                return;
            }

            SpellCaster.Instance.Cast(this);
        }

        public void IncreaseMaxLinks()
        {
            MaxLinksUnlocked++;
        }

        internal void Equip(GemItem gem)
        {
            if (gem.HasSpell)
            {
                ChangeSpell(gem.Spell);
            }
            else
            {
                AddLinked(gem);
            }
        }

        internal void UnEquip(GemItem gem)
        {
            if (gem.HasSpell)
            {
                RemoveSpell();
            }
            else
            {
                RemoveLinked(gem);
            }
        }

        internal void UnlockSpellSlot()
        {
            isUnlocked = true;
        }

        internal float GetTotal(string v)
        {
            return Modifiers.Get(v);
        }
    }
}
