using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MeteorGame
{
    [Serializable]
    public class Modifiers
    {
        public float ProjectileSpeedCalcd => projSpeedCalcd;
        public float ProjectileLifetimeCalcd => projLifetimeCalcd;
        public float ExplosionRadi => expRadiusCalcd;
        public int CombinedDoT => damageOverTimeCalcd;

        public int FireDoT { get => fireDoT; }
        public int LightDoT { get => lightDoT; }
        public int ColdDoT { get => coldDoT; }

        public int FireEffectiveDamage { get => fireEffectiveDamageCalcd; }
        public int ColdEffectiveDamage { get => coldEffectiveDamageCalcd; }
        public int RadiationEffectiveDamage { get => radiationEffectiveDamageCalcd; }
        public float CastTimeCalcd { get => castTimeCalcd; }

        [SerializeField] private float projSpeedCalcd;
        [SerializeField] private float castTimeCalcd;
        [SerializeField] private float projLifetimeCalcd;
        [SerializeField] private float expRadiusCalcd;

        [SerializeField] private int damageOverTimeCalcd;
        [SerializeField] private int fireDoT;
        [SerializeField] private int lightDoT;
        [SerializeField] private int coldDoT;

        [SerializeField] private int fireEffectiveDamageCalcd;
        [SerializeField] private int coldEffectiveDamageCalcd;
        [SerializeField] private int radiationEffectiveDamageCalcd;


        private SpellSlot ownerSlot;
        private Dictionary<string, float> all;
        //private Dictionary<ModifierWithValue, float> all;

        public Modifiers(SpellSlot slot)
        {
            var allSOs = GameManager.Instance.ScriptableObjects.Modifiers;
            all = new Dictionary<string, float>();


            foreach (var dictEntry in allSOs)
            {
                var internalName = dictEntry.Key;
                var modSO = dictEntry.Value;
                
                if (dictEntry.Value.multiplicative)
                {
                    all.Add(dictEntry.Key, 1);
                }
                else
                {
                    all.Add(dictEntry.Key, 0);
                }
            }

            this.ownerSlot = slot;
        }




        private float CalculateProjSpeed()
        {
            float baseSpeed = ownerSlot.Spell.ProjectileSpeed;

            float increasedBy = all["IncreasedProjectileSpeed"];
            float reducedBy = all["ReducedProjectileSpeed"];

            return baseSpeed * increasedBy * reducedBy;
        }


        private float CalculateProjLifetime()
        {
            float baseDur = ownerSlot.Spell.ProjectileLifetime;
            float increasedBy = all["IncreasedEffectDuration"] / 100f;
            float reducedBy = all["ReducedEffectDuration"] / 100f;
            return baseDur * (1 + increasedBy) * (1 - reducedBy);
        }


        private int CalculateDoT()
        {
            var inc = all["IncreasedDamageOverTime"] / 100f;
            var red = all["ReducedDamageOverTime"] / 100f;

            float fireBase = all["FireDamagePerSecond"];
            float coldBase = all["ColdDamagePerSecond"];
            float radiBase = all["RadiationDamagePerSecond"];

            inc += all["IncreasedDamage"] / 100f;
            red += all["ReducedDamage"] / 100f;

            fireDoT = (int)(fireBase * (1 + inc) * (1 - red));
            lightDoT = (int)(radiBase * (1 + inc) * (1 - red));
            coldDoT = (int)(coldBase * (1 + inc) * (1 - red));

            return (int)(fireDoT + coldDoT + lightDoT);
        }

        internal float Get(string s)
        {
            return all[s];
        }

        private float CalculateExpolisonRadius()
        {
            float baseRadius = all["ExplosionRadius"];

            if (baseRadius > 0)
            {
                float inc = all["IncreasedAoe"];
                float red = all["ReducedAoE"];
                return baseRadius * inc * red;
            }

            return 0;
        }


        internal void Add(GemItem gem)
        {
            UpdateModifierDict(gem, add: true);
        }


        internal void Remove(GemItem gem)
        {
            UpdateModifierDict(gem, remove: true);
        }



        public float CalculateCastTime()
        {

            /*




           player is using a skill with a base cast time of 0.8 seconds,
            and the player has a total of 50% increased Cast Speed,
            then the modified cast time can be calculated as follows: 

            1/0.8 = 1.25 casts per second -> base
            1.25 * (1 + 0.5) = 1.88 casts per second -> modified
            1/1.88 = 0.53 seconds -> new cast time


            */


            var shotsPerSecond = 1 / ownerSlot.Spell.ShootTime;
            var modified = shotsPerSecond * all["IncreasedShootingSpeed"];
            var newCastTime = 1 / modified;

            return newCastTime;
        }
        private void UpdateDamageValues()
        {
            var incDmg = all["IncreasedDamage"];
            var redDmg = all["ReducedDamage"];

            var fireDmg = all["DealFireDamage"];
            var coldDmg = all["DealColdDamage"];
            var lightDmg = all["DealRadiationDamage"];


            castTimeCalcd = CalculateCastTime();
            projLifetimeCalcd = CalculateProjLifetime();
            projSpeedCalcd = CalculateProjSpeed();
            expRadiusCalcd = CalculateExpolisonRadius();
            damageOverTimeCalcd = CalculateDoT();

            fireEffectiveDamageCalcd = Mathf.CeilToInt(fireDmg * incDmg * redDmg);
            coldEffectiveDamageCalcd = Mathf.CeilToInt(coldDmg * incDmg * redDmg);
            radiationEffectiveDamageCalcd = Mathf.CeilToInt(lightDmg * incDmg * redDmg);
        }


        private void UpdateModifierDict(GemItem gem, bool add = false, bool remove = false)
        {
            foreach (ModifierWithValue m in gem.Modifiers)
            {
                float val;

                if (m.modifierSO.hasNumericalValue)
                {
                    val = gem.GetModifierValueForCurrentLevel(m.modifierSO);

                    if (m.modifierSO.multiplicative)
                    {
                        val /= 100f;
                    }
                }
                else
                {
                    // if it has no value, increase count by 1 for each instance
                    val = 1;
                }

                if (m.modifierSO.multiplicative)
                {
                    if (m.modifierSO.isReduction)
                    {
                        if (add)
                        {
                            all[m.modifierSO.internalName] *= 1 - val;
                        }
                        if (remove)
                        {
                            all[m.modifierSO.internalName] /= 1 - val;
                        }
                    }
                    else
                    {
                        if (add)
                        {
                            all[m.modifierSO.internalName] *= 1 + val;
                        }
                        if (remove)
                        {
                            all[m.modifierSO.internalName] /= 1 + val;
                        }
                    }
                }
                else
                {
                    if (add)
                    {
                        all[m.modifierSO.internalName] += val;
                    }
                    else
                    {
                        all[m.modifierSO.internalName] -= val;
                    }
                }
            }

            UpdateDamageValues();
        }












    }
}
