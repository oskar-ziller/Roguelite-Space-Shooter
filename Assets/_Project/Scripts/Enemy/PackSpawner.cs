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
        public Transform enemyHolder;
    }





    public class PackSpawner : MonoBehaviour
    {

        #region Variables

        private WeightedRandomEnemy weightedRandomEnemy;


        public float waitAfterSpawn = 0.15f;


        private bool isSetup = false;


        private WaitForSeconds waitAfterSpawnTimer;


        public Action<EnemyPack> SpawnedPack;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            waitAfterSpawnTimer = new WaitForSeconds(waitAfterSpawn);
        }

        #endregion

        #region Methods

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
            yield return SpawnAll(info, generator.spawnPositions);
        }

        private float CalculatePackMovementSpeed(List<FindSpawnPosResult> candidates)
        {

            // calculate average movement speed
            var avgTestVal = 0f;
            foreach (FindSpawnPosResult result in candidates)
            {
                avgTestVal += result.EnemySO.HealthMultiplier / result.EnemySO.SpeedMultiplier;
            }


            avgTestVal /= candidates.Count;


            return 1f/avgTestVal;

        }




        private IEnumerator SpawnAll(PackSpawnInfo info, List<FindSpawnPosResult> candidates)
        {
            print("SpawnAll");

            var packCenter = UnityEngine.Random.onUnitSphere * info.packHeight;

            var packHolder = new GameObject("Pack");
            packHolder.transform.parent = info.enemyHolder;


            var pack = packHolder.AddComponent<EnemyPack>();
            pack.Info = info;
            pack.Holder = packHolder.transform;


            Vector3 spawnPos;

            var packSpeed = CalculatePackMovementSpeed(candidates);

            foreach (FindSpawnPosResult result in candidates)
            {
                spawnPos = packCenter + result.SpawnPos;
                pack.Position = spawnPos;

                var e = EnemyManager.Instance.EnemySpawner.SpawnEnemyFromPool();
                e.transform.parent = packHolder.transform;
                e.gameObject.name = result.EnemySO.Name;
                e.Init(result.EnemySO, spawnPos, packCenter, packSpeed, pack);
                pack.Enemies.Add(e);


                if (waitAfterSpawn > 0)
                {
                    yield return waitAfterSpawnTimer;
                }
            }

            SpawnedPack?.Invoke(pack);
        }


        #endregion

    }
}
