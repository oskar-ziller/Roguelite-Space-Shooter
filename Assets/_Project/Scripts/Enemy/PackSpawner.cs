using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame.Enemies
{

    public struct PackSpawnInfo
    {
        public float spawnerMoney;
        public float distFromOrigin;
        public float enemySpacing;
        public PackShape packShape;
    }

    public class PackSpawner : MonoBehaviour
    {

        #region Variables

        [Tooltip("Transform to mark position of 1st wave spawning on new game")]
        [SerializeField] private Transform firstWaveSpawnPos;

        [Tooltip("Trasnparant object to spawn and fade during pack spawn visual effects")]
        [SerializeField] private SpawnAreaVisual spawnAreaVisual;

        [Tooltip("SpawnBox animation controller")]
        [SerializeField] private SpawnBox spawnBox;

        public float waitAfterSpawn = 0.15f;
        public Action<EnemyPack> PackSpawned;

        private WeightedRandomEnemy weightedRandomEnemy;
        private bool isSetup = false;
        private WaitForSeconds waitAfterSpawnCached;
        private int packCount = 0;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            waitAfterSpawnCached = new WaitForSeconds(waitAfterSpawn);
            GameManager.Instance.GameOver += OnGameOver;
        }

        #endregion

        #region Methods

        private void OnGameOver()
        {
            packCount = 0;
        }

        internal void Setup()
        {
            weightedRandomEnemy = new WeightedRandomEnemy();
            isSetup = true;
        }


        public IEnumerator SpawnPack(PackSpawnInfo info)
        {
            if (!isSetup)
            {
                Setup();
            }

            print($"SpawnPack");

            weightedRandomEnemy.totalMoney = info.spawnerMoney;
            List<EnemySO> enemiesToSpawn = weightedRandomEnemy.CreateSpawnList();

            print($"Total enemies to spawn {enemiesToSpawn.Count}");

            var generator = new PositionGenerator(spacing: info.enemySpacing,
                                                  spawnList: enemiesToSpawn,
                                                  packShape: info.packShape);


            yield return generator.Generate();
            yield return CreateAll(info, generator.spawnPositions, generator.regionExtends);
        }




        private EnemyPack CreatePackObject(PackSpawnInfo info)
        {
            var packPos = UnityEngine.Random.onUnitSphere * info.distFromOrigin;

            if (packCount == 0)
            {
                packPos = firstWaveSpawnPos.position;
            }

            var holder = new GameObject("Pack " + packCount);
            var packObj = holder.AddComponent<EnemyPack>();
            packObj.Info = info;
            packObj.Position = packPos;
            holder.transform.parent = EnemyManager.Instance.EnemiesHolder;
            packCount++;


            return packObj;
        }

        private IEnumerator WaitIfNeeded()
        {
            if (waitAfterSpawn > 0)
            {
                yield return waitAfterSpawnCached;
            }
            else
            {
                yield return null;
            }
        }

        private IEnumerator CreateCandidatesInPack(List<FindSpawnPosResult> candidates, EnemyPack pack)
        {
            foreach (FindSpawnPosResult result in candidates)
            {
                var e = EnemyManager.Instance.EnemySpawner.SpawnEnemyFromPool();
                e.transform.SetParent(pack.transform);
                e.gameObject.name = result.EnemySO.Name;

                var spawninfo = new EnemySpawnInfo
                {
                    SO = result.EnemySO,
                    spawnPos = result.SpawnPos,
                    pack = pack,
                };

                e.Create(spawninfo);
                pack.AddEnemy(e);

                yield return WaitIfNeeded();
            }
        }

        private void CreateSpawnBoxAnim(EnemyPack pack)
        {
            SpawnBox box = Instantiate(spawnBox);

            box.SetSize(pack.PackSize * 2.5f);

            box.transform.position = pack.Position;
            box.AnimCompleted += pack.OnSpawnAnimCompleted;
            box.transform.SetParent(pack.transform);
        }

        private SpawnAreaVisual CreateSpawnAreaVisual(EnemyPack pack)
        {
            SpawnAreaVisual visual = Instantiate(spawnAreaVisual);

            visual.transform.localScale = pack.PackSize * 2.1f * Vector3.one;
            visual.transform.position = pack.Position;
            visual.transform.SetParent(pack.transform);

            return visual;
        }

        private IEnumerator CreateAll(PackSpawnInfo info, List<FindSpawnPosResult> candidates, int regionSize)
        {
            EnemyPack pack = CreatePackObject(info);
            pack.PackSize = regionSize;

            yield return CreateCandidatesInPack(candidates, pack);

            pack.CalculatePackMovementSpeed();

            CreateSpawnBoxAnim(pack);
            pack.SpawnAreaVisual = CreateSpawnAreaVisual(pack);

            PackSpawned?.Invoke(pack);
        }




        #endregion

    }
}
