using MeteorGame.Enemies;
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

    public class EnemySpawner : MonoBehaviour
    {
        #region Variables

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

        [Tooltip("Multiplier of enemy HP as level goes from 0->maxGameLevel")]
        [SerializeField] private AnimationCurve enemyHPMultipCurve;

        [Tooltip("Spacing between two enemies in a pack")]
        [SerializeField] private float spacingBetweenEnemies;

        private List<Spawner> spawners = new();
        private Coroutine spawnLoop_Co;
        private PackSpawner packSpawner;

        private float CurrentLevelRatio { get { return GameManager.Instance.GameLevel / GameManager.Instance.MaxGameLevel; } }
        private float PackMoneyAtCurrentGameLevel
        {
            get
            {
                return minMoney + (EvalCurveForCurrentLevel(packSpawnerMoneyCurve) * moneyVariance);
            }
        }

        #endregion

        #region Unity Methods



        private void Awake()
        {
            //pooledEnemies = new ObjectPool<Enemy>(CreatePooledEnemy, OnTakeFromPool, OnReturnToPool);
            packSpawner = GetComponent<PackSpawner>();
            packSpawner.PackSpawned += OnPackSpawned;
        }



        private void Start()
        {
            GameManager.Instance.GameOver += OnGameOver;
            GameManager.Instance.GameRestart += OnGameStart;
        }


        #endregion

        #region Methods

        public void AddSpawner(Spawner s)
        {
            spawners.Add(s);
        }

        private void OnPackSpawned(EnemyPack pack)
        {
            EnemyManager.Instance.AddPack(pack);
        }

        /// <summary>
        /// Calculates HP multiplier for current GameLevel from enemyHPMultipCurve
        /// </summary>
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


        private Spawner PickRandomSpawner()
        {
            if (packSpawner.SpawnedPacksCount == 0)
            {
                return spawners.First(s => s.InitialSpawner);
            }

            var available = spawners.Where(s => s.CanSpawn);
            var randomIndex = UnityEngine.Random.Range(0, available.Count());
            return available.ElementAt(randomIndex);
        }


        private IEnumerator SpawnPackCoroutine()
        {
            var info = new PackSpawnInfo
            {
                spawner = PickRandomSpawner(),
                enemySpacing = spacingBetweenEnemies,
                spawnerMoney = PackMoneyAtCurrentGameLevel,
                packShape = PackShape.Sphere
            };

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
                UnityEngine.Debug.LogError("Called BeginSpawning when spawnLoop_Co already running");
                StopCoroutine(spawnLoop_Co);
            }

            spawnLoop_Co = StartCoroutine(SpawnLoop());
        }


        private void StopSpawning()
        {
            if (spawnLoop_Co != null)
            {
                StopCoroutine(spawnLoop_Co);
                spawnLoop_Co = null;
            }
        }


        public Enemy SpawnEnemyFromPool()
        {
            var e = EnemyManager.Instance.EnemyPool.Get();
            return e;
        }


        private void OnGameOver()
        {
            StopSpawning();
        }

        private void OnGameStart()
        {
            BeginSpawning();
        }


        #endregion

    }














}
