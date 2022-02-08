using UnityEngine;

namespace MeteorGame
{
    public class FindSpawnPosResult
    {
        public FindSpawnPosResult(Vector3 randomPos, EnemySO toSpawn)
        {
            SpawnPos = randomPos;
            EnemySO = toSpawn;
        }

        public Vector3 SpawnPos { get; }
        public EnemySO EnemySO { get; }
    }
}
