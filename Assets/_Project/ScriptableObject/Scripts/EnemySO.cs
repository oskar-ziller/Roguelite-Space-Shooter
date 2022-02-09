using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{

    [CreateAssetMenu(fileName = "New Enemy", menuName = "ScriptableObjects/Enemy")]
    public class EnemySO : ScriptableObject
    {
        public string name = "unnamed";

        [Min(1f)] public float HealthMultiplier = 1;
        [Min(1f)] public float SpeedMultiplier = 1;
        [Min(1f)] public float ShapeRadi = 0.5f;

        public Enemy Prefab;
        public EnemySpawnEntry SpawnEntry;

        [Tooltip("Uncheck to disable from loading")]
        public bool IsEnabled = true;
    }
}
