using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{


    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        public List<Enemy> aliveEnemies { get; private set; }

        public Transform enemiesHolder;

        public GoldCoinDrop coinDropBig, coinDropMedium, coinDropSmall;

        public Transform goldDropHolder;


        private DropManager dropManager = new DropManager();

        #region Variables

        [SerializeField] private float enemySpeed = 3f;

        public float EnemySpeed => enemySpeed;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            aliveEnemies = new List<Enemy>();
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
        }

       

        private void Start()
        {
            
        }

        private void Update()
        {

            if (GameManager.Instance.waitingForChallenge)
            {
                if (aliveEnemies.Count == 0)
                {
                    GameManager.Instance.StopChallenge();
                }
            }
        
        }

        #endregion

        #region Methods

        private void SpawnGoldDrop(CoinSize whichCoin, Vector3 pos)
        {
            GoldCoinDrop prefab = coinDropSmall;

            if (whichCoin == CoinSize.Medium)
            {
                prefab = coinDropMedium;
            }

            if (whichCoin == CoinSize.Big)
            {
                prefab = coinDropBig;
            }

            Instantiate(prefab, pos, Quaternion.identity, goldDropHolder);
        }


        private void DropGold(Enemy dropFrom)
        {
            var howMany = dropManager.PickRandomNumber(dropFrom);

            for (int i = 0; i < howMany; i++)
            {
                var whichCoin = dropManager.PickRandomCoinSize(dropFrom);
                SpawnGoldDrop(whichCoin, dropFrom.transform.position);
            }
        }

        internal void OnEnemyDeath(Enemy e)
        {
            DropGold(e);
            aliveEnemies.Remove(e);
        }

        internal void AddEnemy(Enemy e)
        {
            aliveEnemies.Add(e);
        }

        internal Enemy ClosestEnemyTo(Vector3 pos)
        {
            float minDist = float.MaxValue;
            Enemy closestEnemy = null;

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                Enemy e = aliveEnemies[i];

                float dist = Vector3.Distance(e.transform.position, pos);

                if (dist > 0 && dist < minDist)
                {
                    minDist = dist;
                    closestEnemy = e;
                }
            }

            return closestEnemy;
        }


        internal List<Enemy> EnemiesInRange(Enemy e, float range, bool fromShell = false)
        {
            return EnemiesInRange(e.transform.position, range, fromShell);
        }



        internal List<Enemy> EnemiesInRange(Vector3 pos, float range, bool fromShell = false)
        {
            var toreturn = new List<Enemy>();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                Enemy e = aliveEnemies[i];

                var vec = (e.transform.position - pos);
                float dist = vec.sqrMagnitude;

                if (fromShell)
                {
                    if (e.rarity == EnemyRarity.Normal)
                    {
                        dist -= normalSpawnInfo.r * normalSpawnInfo.r;
                    }

                    if (e.rarity == EnemyRarity.Magic)
                    {
                        dist -= magicSpawnInfo.r * magicSpawnInfo.r;
                    }

                    if (e.rarity == EnemyRarity.Unique)
                    {
                        dist -= uniqueSpawnInfo.r * uniqueSpawnInfo.r;
                    }

                    if (e.rarity == EnemyRarity.Rare)
                    {
                        dist -= rareSpawnInfo.extends.sqrMagnitude;
                    }
                }

                if (dist <= range * range)
                {
                    toreturn.Add(e);
                }
            }

            return toreturn;
        }

        internal void DestroyAllEnemies()
        {
            var copy = new List<Enemy>(aliveEnemies);

            foreach (var e in copy)
            {
                e.ForceDie();
            }


            foreach (Transform child in enemiesHolder)
            {
                Destroy(child.gameObject);
            }
        }

        internal Enemy PickEnemyToChainTo(Enemy from, Enemy except = null)
        {
            List<Enemy> potential = EnemiesInRange(from, GameManager.Instance.chainRange, true);

            if (potential.Count == 0)
            {
                return null;
            }

            var refined = potential.Where(p => p != except && p != from);

            if (refined.Count() == 0)
            {
                return null;
            }

            return refined.ElementAt(UnityEngine.Random.Range(0, refined.Count()));
        }




        private SpawnInfo uniqueSpawnInfo, rareSpawnInfo, magicSpawnInfo, normalSpawnInfo;



        internal SpawnInfo GetSpawnInfo(EnemyRarity e)
        {
            if (uniqueSpawnInfo == null)
            {
                uniqueSpawnInfo = new SpawnInfo(13.2f);
                rareSpawnInfo = new SpawnInfo(new Vector3(21f, 9f, 21f)/2f);
                magicSpawnInfo = new SpawnInfo(5.2f);
                normalSpawnInfo = new SpawnInfo(2.6f);
            }

            if (e == EnemyRarity.Unique)
            {
                return uniqueSpawnInfo;
            }

            if (e == EnemyRarity.Rare)
            {
                return rareSpawnInfo;
            }

            if (e == EnemyRarity.Magic)
            {
                return magicSpawnInfo;
            }

            if (e == EnemyRarity.Normal)
            {
                return normalSpawnInfo;
            }

            return null;
        }

        #endregion

    }
}
