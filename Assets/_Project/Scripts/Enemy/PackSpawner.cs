using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame
{

    public struct PackSpawnInfo
    {
        public float spawnerMoney;
        public float packHeight;
        public float enemySpacing;
        public PackShape packShape;
    }

    public class PackSpawner : MonoBehaviour
    {

        #region Variables

        [Tooltip("Transform to mark position of 1st wave spawning on new game")]
        [SerializeField] private Transform firstWaveSpawnPos;


        [SerializeField] private GameObject spawnPortalPrefab;


        public float waitAfterSpawn = 0.15f;
        public Action<EnemyPack> SpawnedPack;

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



        private Transform CreatePortal(Vector3 packCenter, int regionSize)
        {
            var portal = Instantiate(spawnPortalPrefab);

            var dir = packCenter.normalized;
            var dist = Vector3.Distance(Vector3.zero, packCenter);

            portal.transform.position = dir * (dist + regionSize + 25);

            portal.transform.LookAt(Vector3.zero);

            return portal.transform;
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
            var packPos = UnityEngine.Random.onUnitSphere * info.packHeight;

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

        private IEnumerator CreateCandidatesInPack(List<FindSpawnPosResult> candidates, EnemyPack pack, Transform portal)
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
                    portalTransform = portal
                };

                e.Create(spawninfo);
                pack.AddEnemy(e);

                if (waitAfterSpawn > 0)
                {
                    yield return waitAfterSpawnCached;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator CreateAll(PackSpawnInfo info, List<FindSpawnPosResult> candidates, int regionSize)
        {
            EnemyPack pack = CreatePackObject(info);

            var portal = CreatePortal(pack.Position, regionSize);
            yield return CreateCandidatesInPack(candidates, pack, portal);

            pack.CalculatePackMovementSpeed();


            pack.DoSpawn();

            // signal to anything listening that we have spawned a new pack
            SpawnedPack?.Invoke(pack);
        }


        #endregion

    }
}
