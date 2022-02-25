using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Enemies
{
    public class EnemySpawnInfo
    {
        internal Vector3 spawnPos;
        internal Vector3 packCenter;
        internal float packSpeed;
        internal EnemyPack pack;
        internal EnemySO SO;
    }
}
