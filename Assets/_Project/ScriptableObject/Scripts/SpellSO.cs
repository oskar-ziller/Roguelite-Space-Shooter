using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    [CreateAssetMenu(fileName = "New Spell", menuName = "ScriptableObjects/Spell")]
    public class SpellSO : ScriptableObject
    {
        public GemSO spellGemSO;

        public string internalName;
        public string prettyName;
        public float timeBetweenCasts;
        public float castTime;
        public float projectileSpeed;
        public float projectileLifetimeSeconds;
        public int projectileCount;

        public ProjectileBase projectilePrefab;
        public ProjectileDummy dummyPrefab;

    }
}
