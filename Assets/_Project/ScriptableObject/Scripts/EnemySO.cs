using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{

    [CreateAssetMenu(fileName = "New Enemy", menuName = "ScriptableObjects/Enemy")]
    public class EnemySO : ScriptableObject
    {
        public string name;

        [Min(1f)] public float HealthMultiplier;
        [Min(1f)] public float SpeedMultiplier;

        public Enemy Prefab;
        public float ShapeRadi;


        public EnemySpawnEntry SpawnEntry;


        [Tooltip("Uncheck to disable from loading")]
        public bool IsEnabled = true;
    }
}
