using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    [Serializable]
    public class SpellItem
    {
        [NonSerialized] private GemItem gemItem;

        private string prettyName;
        private float timeBetweenCasts;
        private float castTime;

        private float projectileSpeed;
        private float projectileLifetimeSeconds;
        private float nextCastTime = 0;
        private float lastCastTime = 0;
        private int projectileCount;

        private SpellSlot slottedAt;

        public GemItem Gem => gemItem;

        public string Name { get; private set; }
        //public float CastTime => castTime;
        //public int TimeBetweenCasts => timeBetweenCasts;
        public float ProjectileSpeed => projectileSpeed;
        public int ProjectileCount => projectileCount;
        public float LifeTime => projectileLifetimeSeconds;

        public float TotalCastTime => timeBetweenCasts + (castTime * (1 - (slottedAt.IncreasedCastSpeed / 100f)));

        public ProjectileBase projPrefab;
        public ProjectileDummy dummyPrefab;

        //public float ExplosionRadius => CalculateExplosionRadius();


        #region Methods


        public SpellItem(SpellSO spellSO, GemItem gemItem)
        {
            this.gemItem = gemItem;

            projPrefab = spellSO.projectilePrefab;
            dummyPrefab = spellSO.dummyPrefab;

            Name = spellSO.internalName;
            prettyName = spellSO.prettyName;

            timeBetweenCasts = spellSO.timeBetweenCasts;
            castTime = spellSO.castTime;

            projectileSpeed = spellSO.projectileSpeed;
            projectileCount = spellSO.projectileCount;

            projectileLifetimeSeconds = spellSO.projectileLifetimeSeconds;
        }

        //private int CalculateProjectileCount()
        //{
        //    int increasedBy = (int)ModifierHelper.GetTotal("AdditionalProjectiles", slottedAt);
        //    return projectileCount + increasedBy;
        //}

        //private int CalculateCastSpeed()
        //{
        //    float increasedBy = ModifierHelper.GetTotal("IncreasedCastSpeed", slottedAt) / 100f;
        //    return (int)(msBetweenCasts * (1 - increasedBy));
        //}

        //public float CalculateProjectileSpeed()
        //{
        //    float increasedBy = ModifierHelper.GetTotal("IncreasedProjectileSpeed", slottedAt) / 100f;
        //    float reducedBy = ModifierHelper.GetTotal("ReducedProjectileSpeed", slottedAt) / 100f;

        //    return projectileSpeed * (1 + increasedBy) * (1 - reducedBy);
        //}


        //public int GetModifierValueForCurrentLevel(Modifier modifier)
        //{
        //    return gemItem.GetModifierValueForCurrentLevel(modifier);
        //}

        //internal bool ModifierExists(Modifier modifier)
        //{
        //    return gemItem.ModifierExists(modifier);
        //}


        public bool CanCast()
        {
            return Time.time > nextCastTime;
        }

        //public int CalculateExplosionRadius()
        //{
        //    float radi = gemItem.GetModifierValueForCurrentLevel("ExplosionRadius");
        //    float increasedBy = ModifierHelper.GetTotal("IncreasedAoe", slottedAt) / 100f;
        //    float totalRadi = radi * (1 + increasedBy);

        //    return (int)totalRadi;
        //}

        public void Cast()
        {
            lastCastTime = Time.time;
            nextCastTime = lastCastTime + TotalCastTime;
        }


        private void SetSlottedAt(SpellSlot slot)
        {
            slottedAt = slot;
        }


        public void Equip(SpellSlot spellSlot)
        {
            SetSlottedAt(spellSlot);
            gemItem.Equip();
        }

        public void UnEquip()
        {
            gemItem.UnEquip();
            slottedAt = null;
        }


        #endregion

    }
}
