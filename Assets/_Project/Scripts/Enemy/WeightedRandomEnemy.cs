using System.Collections.Generic;
using System.Linq;


namespace MeteorGame
{
    public class WeightedRandomEnemy
    {
        public List<EnemySO> entries;
        public float totalMoney;

        public WeightedRandomEnemy()
        {
            entries = GameManager.Instance.ScriptableObjects.Enemies;
        }

        public List<EnemySO> CreateSpawnList()
        {
            var toReturn = new List<EnemySO>();

            float totalWeight = entries.Sum(e => e.SpawnEntry.weight);

            while (totalMoney > 0)
            {
                float randomWeight = UnityEngine.Random.Range(0, totalWeight);
                float currentWeight = 0f;

                foreach (var e in entries)
                {
                    currentWeight += e.SpawnEntry.weight;

                    if (randomWeight <= currentWeight)
                    {
                        totalMoney -= e.SpawnEntry.cost;
                        toReturn.Add(e); // selected one
                        break;
                    }
                }
            }

            return toReturn;
        }

    }














}
