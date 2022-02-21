using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame
{


    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        public List<Enemy> AliveEnemies { get; private set; }
        public List<EnemyPack> AlivePacks { get; private set; }

        public Transform EnemiesHolder, PooledEnemiesHolder;

        public GoldCoinDrop coinDropBig, coinDropMedium, coinDropSmall;

        public Transform goldDropHolder;

        public Enemy enemyPrefab;


        [SerializeField] private EnemySpawner enemySpawner;



        [SerializeField] private int maxEnemyLevel;


        [Tooltip("Speed at which enemies go with 1f speed multiplier")]
        [SerializeField] private int baseEnemySpeed;


        [Tooltip("Enemy health curve between 0-MaxEnemyLevel")]
        [SerializeField] private AnimationCurve enemyHealthCurve;


        [Tooltip("Base hp for enemies at level = 0 and multiplier = 1")]
        [SerializeField] private int baseHP;


        [Tooltip("HP Multiplier when gamelevel = max")]
        [SerializeField] private int maxHpMultiplier;


        [Tooltip("Transform holding all the particle systems to spawn when an enemy dies")]
        [SerializeField] private Transform enemyExplosionPrefab;

        [Tooltip("Transform holding all the explosions that spawn after Enemy death")]
        [SerializeField] Transform enemyExplosionHolder;

        public EnemySpawner EnemySpawner => enemySpawner;


        public ObjectPool<Enemy> EnemyPool;


        public int MaxHpMultiplier => maxHpMultiplier;



        //void OnGUI()
        //{
        //    GUI.Label(new Rect(10, 10, 200, 20), $"CountActive: {EnemyPool.CountActive}");
        //    GUI.Label(new Rect(10, 30, 200, 20), $"CountInactive: {EnemyPool.CountInactive}");
        //    GUI.Label(new Rect(10, 50, 200, 20), $"CountAll: {EnemyPool.CountAll}");
        //}






        public void Setup()
        {
            EnemyPool = new ObjectPool<Enemy>(OnCreateEnemy, OnTakeFromPool, OnReturnToPool);
        }


        private Enemy OnCreateEnemy()
        {
            var e = Instantiate(enemyPrefab);
            e.transform.parent = PooledEnemiesHolder;
            return e;
        }

        private void OnTakeFromPool(Enemy e)
        {
            e.KilledByPlayer += OnEnemyKilledByPlayer;
            e.gameObject.SetActive(true);
        }

        private void OnReturnToPool(Enemy e)
        {
            e.transform.parent = null;
            e.transform.position = Vector3.zero;
            e.transform.parent = PooledEnemiesHolder;
            e.ResetOnKilledEvent(); // remove listeners because we add them again on spawn
            e.gameObject.SetActive(false);
        }

        internal void SpawnPack()
        {
            enemySpawner.SpawnPack();
        }






        //private DropManager dropManager = new DropManager();


        #region Variables

        public AnimationCurve EnemyHealthCurve => enemyHealthCurve;

        internal void BeginSpawning()
        {
            enemySpawner.BeginSpawning();
        }

        public int MaxEnemyLevel => maxEnemyLevel;


        public int BaseEnemySpeed => baseEnemySpeed;

        public int BaseHP => baseHP;


        #endregion

        #region Unity Methods

        private void Awake()
        {
            AliveEnemies = new List<Enemy>();
            AlivePacks = new List<EnemyPack>();
            Instance = this;

            enemySpawner.SpawnedPack += OnPackSpawn;
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


        /// <summary>
        /// Gets called when enemy dies and drops gold prefab.
        /// </summary>
        /// <param name="dropFrom"></param>
        private void DropGold(Enemy dropFrom)
        {
            //var howMany = dropManager.PickRandomNumber(dropFrom);

            //for (int i = 0; i < howMany; i++)
            //{
            //    var whichCoin = dropManager.PickRandomCoinSize(dropFrom);
            //    SpawnGoldDrop(whichCoin, dropFrom.transform.position);
            //}
        }

        public void OnPackDeath(EnemyPack p)
        {
            AlivePacks.Remove(p);
        }

        internal void OnEnemyKilledByPlayer(Enemy e)
        {
            //DropGold(e);
            AliveEnemies.Remove(e);

            // spawn explosion effect
            SpawnExplosion(e);

            EnemyPool.Release(e);
        }

        private void SpawnExplosion(Enemy e)
        {
            Transform explosion = Instantiate(enemyExplosionPrefab);

            explosion.position = e.WorldPos;
            explosion.localScale = e.TransformScale;
            explosion.SetParent(enemyExplosionHolder, true);
        }


        private void OnPackSpawn(EnemyPack p)
        {
            AlivePacks.Add(p);
        }

        internal void AddEnemy(Enemy e)
        {
            AliveEnemies.Add(e);
        }

        internal Enemy ClosestEnemyTo(Vector3 pos)
        {
            float minDist = float.MaxValue;
            Enemy closestEnemy = null;

            for (int i = 0; i < AliveEnemies.Count; i++)
            {
                Enemy e = AliveEnemies[i];

                float dist = Vector3.Distance(e.transform.position, pos);

                if (dist > 0 && dist < minDist)
                {
                    minDist = dist;
                    closestEnemy = e;
                }
            }

            return closestEnemy;
        }

        internal void DestroyAllEnemies()
        {
            for (int i = AliveEnemies.Count - 1; i >= 0; i--)
            {
                //DropGold(e);
                EnemyPool.Release(AliveEnemies[i]);
                AliveEnemies.Remove(AliveEnemies[i]);
            }


            AlivePacks.Clear();

        }


        #endregion

    }
}
