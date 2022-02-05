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

        [SerializeField] private Vector3 debugKeySpawnPos;
        [SerializeField] private Transform parentGroup;


        [SerializeField] private Transform packTestCube, packTestSphere;


        [SerializeField] private Enemy normalEnemyPrefab;
        [SerializeField] private Enemy magicEnemyPrefab;
        [SerializeField] private Enemy rareEnemyPrefab;
        [SerializeField] private Enemy uniqueEnemyPrefab;


        [Tooltip("How far enemy pack can spawn from Arena center")]
        [SerializeField] private int spawnAreaExtends;

        [Tooltip("Starting height for enemies at level 0")]
        [SerializeField] private int startPosMinHeight;

        [Tooltip("Starting height for enemies at Max Level")]
        [SerializeField] private int startPosMaxHeight;

        private WeightedRandomEnemy spawnDirector = new WeightedRandomEnemy();

        //[Tooltip("Delay between each pack in seconds")]
        //[SerializeField] private int delayBetweenPacks;


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
        [SerializeField] private float maxMoney;

        [Tooltip("Pack spawner money curve")]
        [SerializeField] private AnimationCurve packSpawnerMoneyCurve;

        private Coroutine spawnLoop_Co;


        private Stopwatch timeSinceBreak = Stopwatch.StartNew();

        public AudioClip uniqueSpawnClip;
        public AudioClip rareSpawnClip;
        public AudioClip magicSpawnClip;
        public AudioClip normalSpawnClip;




        [Tooltip("Spacing between two enemies in a pack")]
        [SerializeField] private float spacingBetweenEnemies;

        private int totalSpawned = 0;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {

        }

        private void BreakStart()
        {
            print("BreakStart");
        }

        private void BreakEnd()
        {
            print("BreakEnd");
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
                if (!GameManager.Instance.waitingForChallenge)
                {
                    yield return PackSpawnStart();
                    yield return new WaitForSeconds(CalculateNextWaveDelay());
                }

                yield return new WaitForSeconds(1);
            }
        }


        private float CalculatePackMoneyForCurrentGameLevel()
        {
            float level = GameManager.Instance.GameLevel;
            float maxLevel = GameManager.Instance.MaxGameLevel;

            var curveVal = packSpawnerMoneyCurve.Evaluate(level / maxLevel);
            return minMoney + curveVal * maxMoney;
        }


        private float CalculatePackHeightForCurrentGameLevel()
        {
            float level = GameManager.Instance.GameLevel;
            float maxLevel = GameManager.Instance.MaxGameLevel;

            var curveVal = packSpawnPosHeightCurve.Evaluate(level / maxLevel);
            return startPosMinHeight + curveVal * startPosMaxHeight;
        }

        public IEnumerator PackSpawnStart()
        {
            print("PackSpawnStart");

            var random = new System.Random();

            spawnDirector.totalMoney = CalculatePackMoneyForCurrentGameLevel();
            List<EnemyRarity> enemiesToSpawn = spawnDirector.CreateSpawnList();


            var normals = enemiesToSpawn.Where(e => e == EnemyRarity.Normal).ToList();
            var magics = enemiesToSpawn.Where(e => e == EnemyRarity.Magic).ToList();
            var rares = enemiesToSpawn.Where(e => e == EnemyRarity.Rare).ToList();
            var uniques = enemiesToSpawn.Where(e => e == EnemyRarity.Unique).ToList();

            print($"totalMoney: {spawnDirector.totalMoney}");
            print($"total enemies to spawn {enemiesToSpawn.Count}");
            print($"normals: {normals.Count}");
            print($"magics: {magics.Count}");
            print($"rares: {rares.Count}");
            print($"uniques: {uniques.Count}");

            
            bool cointoss = Random.value < 0.5f;
            PackShape randShape = PackShape.Sphere;

            if (cointoss)
            {
                randShape = PackShape.Cube;
            }

            var generator = new PositionGenerator(shape: randShape,
                                                  spacing: spacingBetweenEnemies,
                                                  spawnList: enemiesToSpawn,
                                                  maxExtends: spawnAreaExtends);


            yield return generator.Generate();

            //var randomPackCenterPosX = random.Next(-spawnAreaExtends + generator.regionExtends, spawnAreaExtends - generator.regionExtends);
            //var randomPackCenterPosZ = random.Next(-spawnAreaExtends + generator.regionExtends, spawnAreaExtends - generator.regionExtends );


            var height = CalculatePackHeightForCurrentGameLevel();
            var pos = Random.onUnitSphere * height;

            var holder = new GameObject("Pack");
            holder.transform.parent = parentGroup;

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

        private Enemy GetPrefab(EnemyRarity r)
        {
            if (r == EnemyRarity.Normal)
            {
                return normalEnemyPrefab;
            }

            if (r == EnemyRarity.Magic)
            {
                return magicEnemyPrefab;
            }

            if (r == EnemyRarity.Rare)
            {
                return rareEnemyPrefab;
            }

            if (r == EnemyRarity.Unique)
            {
                return uniqueEnemyPrefab;
            }

            throw new System.Exception("Undefined rarity");
        }


        private Enemy SpawnEnemy(Enemy prefab, Transform parent, EnemyRarity rarity, Vector3 startPos)
        {
            Enemy e = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            e.gameObject.layer = LayerMask.NameToLayer("Enemies");
            e.transform.parent = parent;

            e.OnDeath += EnemyManager.Instance.OnEnemyDeath;

            EnemyManager.Instance.AddEnemy(e);

            //e.ChangeSize(size);
            e.SetRarity(rarity);
            e.Init(startPos, totalSpawned);

            totalSpawned++;

            return e;
        }

        private IEnumerator SpawnCandidatesAroundPoint(Vector3 point, List<SpawnPos> candidates, Transform parent)
        {
            print("SpawnCandidatesAroundPoint");
            var uniques = candidates.Where(c => c.rarity == EnemyRarity.Unique).ToList();
            var rares = candidates.Where(c => c.rarity == EnemyRarity.Rare).ToList();
            var magics = candidates.Where(c => c.rarity == EnemyRarity.Magic).ToList();
            var normals = candidates.Where(c => c.rarity == EnemyRarity.Normal).ToList();

            Vector3 startPos;


            foreach (var c in uniques)
            {
                if (uniqueEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(uniqueEnemyPrefab, parent, EnemyRarity.Unique, startPos);
                print("spawning enemy");
                yield return new WaitForSeconds(0.02f);
            }

            foreach (var c in rares)
            {
                if (rareEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(rareEnemyPrefab, parent, EnemyRarity.Rare, startPos);
                print("spawning enemy");
                yield return new WaitForSeconds(0.02f);
            }

            foreach (var c in magics)
            {
                if (magicEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(magicEnemyPrefab, parent, EnemyRarity.Magic, startPos);
                print("spawning enemy");
                yield return new WaitForSeconds(0.02f);
            }

            foreach (var c in normals)
            {
                if (normalEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(normalEnemyPrefab, parent, EnemyRarity.Normal, startPos);
                yield return new WaitForSeconds(0.02f);
            }
        
            yield return null;
        }


        #endregion

    }

    public class WeightedRandomEnemy
    {
        public class EnemySpawnEntry
        {
            public EnemyRarity Rarity;
            public float weight;
            public float cost;

            public EnemySpawnEntry(EnemyRarity r, float cost, float weight)
            {
                Rarity = r;
                this.cost = cost;
                this.weight = weight;
            }
        }

        public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();
        public float totalMoney;

        public WeightedRandomEnemy()
        {
            float normalOccurence = 9400f / 10000f;
            float magicOccurence = 550f / 10000f;
            float rareOccurence = 50f / 10000f;
            float uniqueOccurence = 5f / 10000f;

            entries.Add(new EnemySpawnEntry(EnemyRarity.Normal, 1, normalOccurence));
            entries.Add(new EnemySpawnEntry(EnemyRarity.Magic, 10, magicOccurence));
            entries.Add(new EnemySpawnEntry(EnemyRarity.Rare, 30, rareOccurence));
            entries.Add(new EnemySpawnEntry(EnemyRarity.Unique, 150, uniqueOccurence));
        }

        public List<EnemyRarity> CreateSpawnList()
        {
            var toReturn = new List<EnemyRarity>();

            float totalWeight = entries.Sum(e => e.weight);

            while (totalMoney > 0)
            {
                float randomWeight = UnityEngine.Random.Range(0, totalWeight);
                float currentWeight = 0f;

                foreach (var e in entries)
                {
                    currentWeight += e.weight;

                    if (randomWeight <= currentWeight)
                    {
                        totalMoney -= e.cost;
                        toReturn.Add(e.Rarity); // selected one
                        break;
                    }
                }
            }

            return toReturn;
        }

    }














}
