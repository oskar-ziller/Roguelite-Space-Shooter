using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame
{
    public class EnemyCreator
    {
        public EnemySO enemySO;
        public EnemySpawner spawner;
        public ObjectPool<Enemy> pool;


        public Enemy CreatePooledEnemy()
        {
            return spawner.SpawnEnemy(enemySO, pool);
        }

    }
}
