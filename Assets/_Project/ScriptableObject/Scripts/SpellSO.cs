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
        //public float timeBetweenCasts;
        public float castTime;
        public float projectileSpeed;
        public float projectileLifetime;
        public int projectileCount;


        [Header("Prefabs")]

        [Tooltip("Actual object that travels")]
        public ProjectileBase projectilePrefab;
        [Tooltip("Dummy object that shows up (at wand etc.) before travel")]
        public ProjectileDummy dummyPrefab;
        [Tooltip("Explosion object to spawn when proj explodes")]
        public Explosion explosionPrefab;


        [Tooltip("Uncheck to disable from loading")]
        public bool isEnabled = true;

    }
}
