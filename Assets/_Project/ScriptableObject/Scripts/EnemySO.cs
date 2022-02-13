using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{

    [CreateAssetMenu(fileName = "New Enemy", menuName = "ScriptableObjects/Enemy")]
    public class EnemySO : ScriptableObject
    {
        public string Name = "unnamed";

        [Min(1f)] public float HealthMultiplier = 1;
        [Min(0f)] public float SpeedMultiplier = 1;
        [Min(0.1f)] public float ShapeRadi = 0.5f;

        [Tooltip("Weight and cost for weighted random spawner")]
        public EnemySpawnEntry SpawnEntry;

        [Tooltip("Uncheck to disable from loading")]
        public bool IsEnabled = true;

        [Tooltip("Mat for body (healthbar fill)")]
        public Material BodyMat;
        [Tooltip("Mesh for body")]
        public Mesh BodyMesh;
    }
}
