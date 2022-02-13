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

        private float projectileSpeed;
        private float projectileLifetime;
        private int projectileCount;
        private float shootTime;


        private float nextCastTime = 0;
        private float lastCastTime = 0;

        private SpellSlot slottedAt;

        public GemItem Gem => gemItem;

        public string Name { get; private set; }
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public int ProjectileCount => projectileCount;
        public float ShootTime => shootTime;

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

            shootTime = spellSO.shootTime;

            projectileSpeed = spellSO.projectileSpeed;
            projectileCount = spellSO.projectileCount;
            projectileLifetime = spellSO.projectileLifetime;
        }


        public bool CanCast()
        {
            return Time.time > nextCastTime;
        }

        public void Cast()
        {
            lastCastTime = Time.time;
            nextCastTime = lastCastTime + slottedAt.Modifiers.CastTimeCalcd;
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
