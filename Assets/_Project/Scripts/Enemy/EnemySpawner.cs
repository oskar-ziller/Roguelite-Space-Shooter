using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame
{

    [RequireComponent(typeof(PackSpawner))]
    public class EnemySpawner : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Transform enemyHolder;
        [SerializeField] private Transform packTestCube, packTestSphere;


        [Tooltip("Starting height for enemies at level 0")]
        [SerializeField] private int startPosMinHeight;

        [Tooltip("How much further should max level enemies spawn on top of startPosMinHeight")]
        [SerializeField] private int startPosVariance;



        [Tooltip("Min delay between two pack spawns in seconds")]
        [SerializeField] private float minDelayBeteenPacks;

        [Tooltip("Max delay between two pack spawns in seconds")]
        [SerializeField] private float maxDelayBeteenPacks;

        [Tooltip("Delay between each pack in seconds")]
        [SerializeField] private AnimationCurve delayBetweenPacksCurve;

        [Tooltip("Pack spawn height curve")]
        [SerializeField] private AnimationCurve packSpawnPosHeightCurve;


        [Tooltip("Pack spawner min money")]
        [SerializeField] private float minMoney;

        [Tooltip("Pack spawner max money")]
        [SerializeField] private float moneyVariance;

        [Tooltip("Pack spawner money curve")]
        [SerializeField] private AnimationCurve packSpawnerMoneyCurve;

        private Coroutine spawnLoop_Co;


        [Tooltip("Spacing between two enemies in a pack")]
        [SerializeField] private float spacingBetweenEnemies;


        public Action<EnemyPack> SpawnedPack;


        public Enemy testPrefab;

        private PackSpawner packSpawner;



        //public ObjectPool<Enemy> pooledEnemies;


        [Tooltip("Multiplier of enemy HP as level goes from 0->maxGameLevel")]
        [SerializeField] private AnimationCurve enemyHPMultipCurve;



        private void Awake()
        {
            //pooledEnemies = new ObjectPool<Enemy>(CreatePooledEnemy, OnTakeFromPool, OnReturnToPool);
            packSpawner = GetComponent<PackSpawner>();
            packSpawner.SpawnedPack += OnPackSpawned;
        }


        /// <summary>
        /// Packspawner raises an action and we pass it along
        /// </summary>
        /// <param name="pack"></param>
        private void OnPackSpawned(EnemyPack pack)
        {
            SpawnedPack?.Invoke(pack);
        }

        public float CalculateEnemyHPMultip()
        {
            var currLevel = GameManager.Instance.GameLevel;
            var maxLevel = GameManager.Instance.MaxGameLevel;

            var maxMultip = EnemyManager.Instance.MaxHpMultiplier;

            //var currentMultip = Helper.Map(currLevel, 0, maxMultip, 1, maxMultip);

            var curveVal = enemyHPMultipCurve.Evaluate(currLevel / maxLevel);


            var currentMultip = curveVal * maxMultip;

            return currentMultip;
        }









        private float CurrentLevelRatio { get { return GameManager.Instance.GameLevel / GameManager.Instance.MaxGameLevel; } }

        private float PackMoneyAtCurrentGameLevel
        {
            get
            {
                return minMoney + (EvalCurveForCurrentLevel(packSpawnerMoneyCurve) * moneyVariance);
            }
        }

        private float PackHeightAtCurrentGameLevel
        {
            get
            {
                return startPosMinHeight + (EvalCurveForCurrentLevel(packSpawnPosHeightCurve) * moneyVariance);
            }
        }

        private void Update()
        {

        }








        #endregion


        #region Unity Methods






        private float CalculateNextWaveDelay()
        {
            float level = GameManager.Instance.GameLevel;
            float maxLevel = GameManager.Instance.MaxGameLevel;

            var val = delayBetweenPacksCurve.Evaluate(level / maxLevel);
            var delay = minDelayBeteenPacks + val * maxDelayBeteenPacks;
            return delay;
        }



        public void SpawnPack()
        {
            StartCoroutine(SpawnPackCoroutine());
        }


        private IEnumerator SpawnPackCoroutine()
        {
            var info = new PackSpawnInfo();

            info.enemyHolder = enemyHolder;
            info.enemySpacing = spacingBetweenEnemies;
            info.packHeight = PackHeightAtCurrentGameLevel;
            info.spawnerMoney = PackMoneyAtCurrentGameLevel;
            info.packShape = PackShape.Sphere;

            yield return packSpawner.SpawnPack(info);


        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return SpawnPackCoroutine();
                yield return new WaitForSeconds(CalculateNextWaveDelay());
            }
        }


        private float EvalCurveForCurrentLevel(AnimationCurve c)
        {
            return c.Evaluate(CurrentLevelRatio);
        }


        public void BeginSpawning()
        {
            if (spawnLoop_Co != null)
            {
                StopCoroutine(spawnLoop_Co);
            }

            spawnLoop_Co = StartCoroutine(SpawnLoop());
        }


        #endregion

        #region Methods


        public Enemy SpawnEnemyFromPool()
        {
            var e = EnemyManager.Instance.EnemyPool.Get();
            EnemyManager.Instance.AddEnemy(e);
            return e;
        }

        /// <summary>
        /// Parse enemy holder and destroy all child holders
        /// </summary>
        internal void DestroyPackHolders()
        {
            foreach (Transform child in enemyHolder)
            {
                Destroy(child.gameObject);
            }
        }






        #endregion

    }














}
