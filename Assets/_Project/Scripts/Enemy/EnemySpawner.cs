using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace MeteorGame
{
    public class EnemySpawner : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Transform enemyHolder;
        [SerializeField] private Transform packTestCube, packTestSphere;


        [Tooltip("Starting height for enemies at level 0")]
        [SerializeField] private int startPosMinHeight;

        [Tooltip("How much further should max level enemies spawn on top of startPosMinHeight")]
        [SerializeField] private int startPosVariance;

        private WeightedRandomEnemy spawnDirector;


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








        #endregion

        #region Unity Methods


        public void Setup()
        {
            spawnDirector = new WeightedRandomEnemy();
        }

        private float CalculateNextWaveDelay()
        {
            float level = GameManager.Instance.GameLevel;
            float maxLevel = GameManager.Instance.MaxGameLevel;

            var val = delayBetweenPacksCurve.Evaluate(level / maxLevel);
            var delay = minDelayBeteenPacks + val * maxDelayBeteenPacks;
            return delay;
        }


        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return PackSpawnStart();
                yield return new WaitForSeconds(CalculateNextWaveDelay());
            }
        }


        private float EvalCurveForCurrentLevel(AnimationCurve c)
        {
            return c.Evaluate(CurrentLevelRatio);
        }


        private IEnumerator GeneratePositions(List<EnemySO> enemiesToSpawn)
        {
            bool cointoss = Random.value < 0.5f;
            PackShape randShape = PackShape.Sphere;

            if (cointoss)
            {
                randShape = PackShape.Cube;
            }

            var randomSpacingRange = Random.Range(-spacingBetweenEnemies / 2, spacingBetweenEnemies / 2);

            var generator = new PositionGenerator(packShape: randShape,
                                                  spacing: spacingBetweenEnemies + randomSpacingRange,
                                                  spawnList: enemiesToSpawn);

            yield return generator.Generate();
        }



        public IEnumerator PackSpawnStart()
        {

            print($"PackSpawnStart");


            spawnDirector.totalMoney = PackMoneyAtCurrentGameLevel;
            List<EnemySO> enemiesToSpawn = spawnDirector.CreateSpawnList();

            print($"Total enemies to spawn {enemiesToSpawn.Count}");
           
            yield return GeneratePositions(enemiesToSpawn);


            var height = PackHeightAtCurrentGameLevel;
            var pos = Random.onUnitSphere * height;

            var holder = new GameObject("Pack");
            holder.transform.parent = enemyHolder;

            Transform packTest;

            if (randShape == PackShape.Cube)
            {
                packTest = Instantiate(packTestCube);
            }
            else
            {
                packTest = Instantiate(packTestSphere);
            }

            packTest.localScale = Vector3.one * generator.regionExtends * 2;

            packTest.SetParent(holder.transform);
            packTest.position = pos;

            yield return SpawnCandidatesAroundPoint(pos, generator.spawnPositions, holder.transform);
        }

        public void BeginSpawning()
        {
            if (spawnLoop_Co != null)
            {
                StopCoroutine(spawnLoop_Co);
            }

            spawnLoop_Co = StartCoroutine(SpawnLoop());
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Backspace))
            {

            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                EnemyManager.Instance.DestroyAllEnemies();
            }

        }

        #endregion

        #region Methods


        private Enemy SpawnEnemy(EnemySO enemySO)
        {
            Enemy e = Instantiate(enemySO.Prefab, Vector3.zero, Quaternion.identity);
            e.gameObject.layer = LayerMask.NameToLayer("Enemies");
            return e;
        }

        private IEnumerator SpawnCandidatesAroundPoint(Vector3 point, List<FindSpawnPosResult> candidates, Transform parent)
        {
            print("SpawnCandidatesAroundPoint");

            Vector3 spawnPos;

            foreach (FindSpawnPosResult result in candidates)
            {
                spawnPos = point + result.SpawnPos;

                Enemy e = SpawnEnemy(result.EnemySO);

                yield return new WaitForSeconds(0.02f);
            }

            yield return null;
        }


        #endregion

    }














}
