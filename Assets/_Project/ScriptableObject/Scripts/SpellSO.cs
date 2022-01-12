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
        public int msBetweenCasts;
        public float castTimeMs;
        public float projectileSpeed;
        public float projectileLifetimeSeconds;
        internal float lastCastTime;
        public int projectileCount;

        private void OnEnable()
        {
            lastCastTime = 0;
        }
    }
}
