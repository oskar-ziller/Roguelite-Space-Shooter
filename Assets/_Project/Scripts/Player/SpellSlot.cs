using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    [System.Serializable]
    public class SpellSlot
    {

        /// <summary>
        /// Used to store all modifiers that come from gems and spells in this slot
        /// </summary>
        [Serializable]
        private class ModifierFromEquipped
        {
            public Modifier m;
            public float val = float.MinValue;

            public ModifierFromEquipped(Modifier m, float val)
            {
                this.m = m;
                this.val = val;
            }

        }

        [SerializeField] private bool isUnlocked = false;

        private List<GemItem> gems = new List<GemItem>();

        public int MaxLinks { get; private set; }

        public bool CanLinkMore => CanLink();

        public SpellItem Spell { get; private set; }
        public List<GemItem> Linked => gems;

        public bool IsUnlocked => isUnlocked;

        public Action<SpellSlot, GemItem> GemLinkedOrRemoved;
        public Action<SpellSlot, SpellItem> SpellChanged;

        public int slotNo;

        [SerializeField] private List<ModifierFromEquipped> allModifiers = new List<ModifierFromEquipped>();

        [SerializeField] private int fireDamage;
        [SerializeField] private int lightningDamage;
        [SerializeField] private int coldDamage;
        [SerializeField] private int totalIncreased;
        [SerializeField] private int totalReduced;

        [SerializeField] private float projLifetime;
        [SerializeField] private float expRadius;

        [SerializeField] private int chillingAreaLimit;


        [SerializeField] private int damageOverTime;


        [SerializeField] private float projSpeed;
        [SerializeField] private int projCount;





        private int fireDoT;
        private int coldDoT;
        private int lightDoT;

        public int FireBaseDamage => fireDamage;
        public int LightningBaseDamage => lightningDamage;
        public int ColdBaseDamage => coldDamage;
        public int TotalIncreasedDamage => totalIncreased;
        public int TotalReducedDamage => totalReduced;













        public int FireEffectiveDamage => (int)(fireDamage * (1 + totalIncreased / 100f) * (1 - totalReduced / 100f));
        public int ColdEffectiveDamage => (int)(coldDamage * (1 + totalIncreased / 100f) * (1 - totalReduced / 100f));
        public int LightningEffectiveDamage => (int)(lightningDamage * (1 + totalIncreased / 100f) * (1 - totalReduced / 100f));



        public int EffectiveDamage => (fireDamage + lightningDamage + coldDamage) * (1 + totalIncreased) * (1 - totalReduced);


        public float ProjLifetime => projLifetime;
        public float ExpRadi => expRadius;

        public int ChillingAreaLimit => chillingAreaLimit;

        public int DamageOverTime => damageOverTime;
        public int FireDoT => fireDoT;
        public int ColdDoT => coldDoT;
        public int LightDoT => lightDoT;

        public float ProjectileSpeed => projSpeed;
        public int ProjectileCount => projCount;


        private float CalculateProjSpeed()
        {
            float baseSpeed = Spell.ProjectileSpeed;

            float increasedBy = GetTotal("IncreasedProjectileSpeed") / 100f;
            float reducedBy = GetTotal("ReducedProjectileSpeed") / 100f;

            return baseSpeed * (1 + increasedBy) * (1 - reducedBy);
        }

        private int CalculateProjCount()
        {
            float baseCount = Spell.ProjectileCount;

            float increasedBy = GetTotal("AdditionalProjectiles");

            return (int)(baseCount + increasedBy);
        }



        private float CalculateProjLifetime()
        {
            // setup destruction
            float baseDur = GetTotal("BaseDuration");
            float increasedBy = GetTotal("IncreasedSkillEffectDuration") / 100f;
            float reducedBy = GetTotal("ReducedSkillEffectDuration") / 100f;
            return baseDur * (1 + increasedBy) * (1 - reducedBy);
        }


        private int CalculateDoT()
        {
            var inc = GetTotal("IncreasedDamageOverTime") / 100f;
            var red = GetTotal("ReducedDamageOverTime") / 100f;

            float fireBase = GetTotal("FireDamagePerSecond");
            float coldBase = GetTotal("ColdDamagePerSecond");
            float lightningBase = GetTotal("LightningDamagePerSecond");

            inc += TotalIncreasedDamage;
            red += TotalReducedDamage;

            fireDoT = (int)(fireBase * (1 + inc) * (1 - red));
            coldDoT = (int)(coldBase * (1 + inc) * (1 - red));
            lightDoT = (int)(lightningBase * (1 + inc) * (1 - red));

            return (int)(fireDoT + coldDoT + lightDoT);
        }

        private float CalculateExpolisonRadius()
        {
            float baseRadius = GetTotal("ExplosionRadius");

            if (baseRadius > 0)
            {
                float inc = GetTotal("IncreasedAoe") / 100f;
                float red = GetTotal("ReducedAoE") / 100f;
                return baseRadius * (1 + inc) * (1 - red);
            }

            return 0;
        }







        public SpellSlot(int slotNr)
        {
            MaxLinks = 0;
            slotNo = slotNr;
        }

        public float GetTotal(string s)
        {
            var m = allModifiers.FirstOrDefault(mod => mod.m.internalName == s);

            return m != null ? m.val : 0;
        }

        private void UpdateDamageValues()
        {
            var increasedDamage = GetTotal("IncreasedDamage");
            var increased = increasedDamage;

            var reducedDamage = GetTotal("ReducedDamage");
            var reduced = reducedDamage;

            float fire = GetTotal("DealFireDamage");
            float lightning = GetTotal("DealLightningDamage");
            float cold = GetTotal("DealColdDamage");

            fireDamage = (int)fire;
            lightningDamage = (int)lightning;
            coldDamage = (int)cold;
            totalIncreased = (int)increased;
            totalReduced = (int)reduced;


            projLifetime = CalculateProjLifetime();
            projSpeed = CalculateProjSpeed();
            projCount = CalculateProjCount();

            expRadius = CalculateExpolisonRadius();

            chillingAreaLimit = (int)GetTotal("ChillingAreaLimit");

            damageOverTime = CalculateDoT();
        }







        // Keep track of all modifiers in a dictionary
        // And update it accordingly when gem is added or removed
        private void SaveToModifierDict(GemItem gem)
        {
            foreach (ModifierWithValue m in gem.Modifiers)
            {
                if (!allModifiers.Any(mod => mod.m == m.modifier))
                {
                    if (m.modifier.hasValues)
                    {
                        float val = gem.GetModifierValueForCurrentLevel(m.modifier);
                        allModifiers.Add(new ModifierFromEquipped(m.modifier, val));
                    }
                    else
                    {
                        // if it has no value, increase count by 1 for each instance
                        allModifiers.Add(new ModifierFromEquipped(m.modifier, 1));
                    }
                }
                else
                {
                    if (m.modifier.hasValues)
                    {
                        float val = gem.GetModifierValueForCurrentLevel(m.modifier);
                        allModifiers.First(mod => mod.m == m.modifier).val += val;
                    }
                    else
                    {
                        // if it has no value, increase count by 1 for each instance
                        allModifiers.First(mod => mod.m == m.modifier).val += 1; 
                    }
                }
            }

            UpdateDamageValues();
        }

        private void RemoveFromModifierDict(GemItem gem)
        {
            foreach (ModifierWithValue m in gem.Modifiers)
            {
                var modifierObj = allModifiers.First(mod => mod.m == m.modifier);

                if (m.modifier.hasValues)
                {
                    float val = gem.GetModifierValueForCurrentLevel(m.modifier);
                    modifierObj.val -= val;
                }
                else
                {
                    // if it has no value, increase count by 1 for each instance
                    modifierObj.val -= 1;
                }

                if (allModifiers.First(mod => mod.m == m.modifier).val <= 0)
                {
                    allModifiers.Remove(modifierObj);
                }
            }

            UpdateDamageValues();
        }

        private void AddLinked(GemItem gem)
        {
            if (!CanLinkMore || !IsUnlocked)
            {
                return;
            }

            gems.Add(gem);
            gem.Equip();
            SaveToModifierDict(gem);
            GemLinkedOrRemoved?.Invoke(this, gem);
        }

        public void RemoveLinked(GemItem gem)
        {
            gem.UnEquip();
            gems.Remove(gem);
            RemoveFromModifierDict(gem);
            GemLinkedOrRemoved?.Invoke(this, gem);
        }

        private void ChangeSpell(SpellItem spellToEquip)
        {
            if (Spell != null)
            {
                Spell.UnEquip();
                RemoveFromModifierDict(Spell.Gem);
            }

            Spell = spellToEquip;

            if (spellToEquip != null)
            {
                gems.Remove(spellToEquip.Gem);
                spellToEquip.Equip(this);
                SaveToModifierDict(spellToEquip.Gem);
            }

            SpellChanged?.Invoke(this, spellToEquip);
        }

        public void RemoveSpell()
        {
            ChangeSpell(null);
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

        internal void UnlockSpellSlot()
        {
            isUnlocked = true;
        }
    }
}
