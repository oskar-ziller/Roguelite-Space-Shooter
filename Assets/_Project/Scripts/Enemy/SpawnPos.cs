using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MeteorGame
{
    public class SpawnPos
    {
        public Vector3 center;
        public SpawnInfo spawnInfo;
        public EnemyRarity rarity;

        public SpawnPos(Vector3 center, SpawnInfo spawnInfo, EnemyRarity rarity)
        {
            this.center = center;
            this.spawnInfo = spawnInfo;
            this.rarity = rarity;
        }
    }
}
