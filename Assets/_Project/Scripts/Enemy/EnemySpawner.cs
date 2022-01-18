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
        [SerializeField] private int spawnAreaSize;

        [Tooltip("Starting height for enemies at level 0")]
        [SerializeField] private int startPosMinHeight;

        [Tooltip("Starting height for enemies at Max Level")]
        [SerializeField] private int startPosMaxHeight;

        private WeightedRandomEnemy spawnDirector =  new WeightedRandomEnemy();

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

        //[Tooltip("Break duration in seconds")]
        //[SerializeField] private int breakDuration;

        //[Tooltip("Break interval in seconds")]
        //[SerializeField] private int breakInterval;

        [Tooltip("Interval between two consequtive enemy spawns in seconds")]
        [SerializeField] private float spawnInterval;

        private Coroutine spawnLoop_Co;
        //private float packSizeFactor = 1f;


        private Stopwatch timeSinceBreak = Stopwatch.StartNew();

        public AudioClip uniqueSpawnClip;
        public AudioClip rareSpawnClip;
        public AudioClip magicSpawnClip;
        public AudioClip normalSpawnClip;



        
        [Tooltip("Spacing between two enemies in a pack")]
        [SerializeField] private float spacingBetweenEnemies;


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

        private IEnumerator SpawnLoop()
        {
            while(true)
            {
                if (!GameManager.Instance.waitingForChallenge)
                {
                    yield return PackSpawnStart();

                    float level = GameManager.Instance.GameLevel;
                    float maxLevel = GameManager.Instance.MaxGameLevel;

                    var val = delayBetweenPacksCurve.Evaluate(level / maxLevel);
                    var delay = minDelayBeteenPacks + val * maxDelayBeteenPacks;

                    yield return new WaitForSeconds(delay);
                }

                yield return null;
            }
        }




        private IEnumerator PackSpawnStart()
        {
            print("PackSpawnStart");

            var random = new System.Random();


            //packSizeFactor = (1 + GameManager.Instance.gameLevel) * 0.04f;

            float level = GameManager.Instance.GameLevel;
            float maxLevel = GameManager.Instance.MaxGameLevel;

            var val1 = packSpawnerMoneyCurve.Evaluate(level / maxLevel);
            var target1 = minMoney + val1 * maxMoney;

            spawnDirector.totalMoney = target1;
            print($"totalMoney: {spawnDirector.totalMoney}");

            List<EnemyRarity> enemiesToSpawn = spawnDirector.CreateSpawnList();

            print($"total enemies to spawn {enemiesToSpawn.Count}");


            var normals = enemiesToSpawn.Where(e => e == EnemyRarity.Normal).ToList();
            var magics = enemiesToSpawn.Where(e => e == EnemyRarity.Magic).ToList();
            var rares = enemiesToSpawn.Where(e => e == EnemyRarity.Rare).ToList();
            var uniques = enemiesToSpawn.Where(e => e == EnemyRarity.Unique).ToList();

            print($"normals: {normals.Count}");
            print($"magics: {magics.Count}");
            print($"rares: {rares.Count}");
            print($"uniques: {uniques.Count}");

            var spawnList = RarityToRadius(enemiesToSpawn);
            spawnList.Sort();

            bool cointoss = UnityEngine.Random.value < 0.5f;
            PackShape randShape = PackShape.Sphere;

            if (cointoss)
            {
                randShape = PackShape.Cube;
            }

            var generator = new PositionGenerator(shape: randShape, maxRegionSize: spawnAreaSize - 1, spacing: spacingBetweenEnemies, spawnList: spawnList, iterationsBeforeWait: 50);
            generator.Generate();


            var val = packSpawnPosHeightCurve.Evaluate(level / maxLevel);
            var target = startPosMinHeight + val * startPosMaxHeight;

            var randx = random.Next(-spawnAreaSize/2 + generator.regionSize / 2, spawnAreaSize/2 - generator.regionSize / 2);
            var randz = random.Next(-spawnAreaSize/2 + generator.regionSize / 2, spawnAreaSize/2 - generator.regionSize / 2);
            var pos = new Vector3(randx, target, randz);

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

            if (generator.regionSize > 1)
            {
                packTest.localScale = Vector3.one * generator.regionSize;
            }
            else
            {
                packTest.localScale = Vector3.one * 1;
            }

            packTest.SetParent(holder.transform);
            packTest.position = pos;

            yield return SpawnCandidatesAroundPoint(pos, generator.spheres, holder.transform);
        }

        public void BeginSpawning()
        {
            if (spawnLoop_Co != null)
            {
                StopCoroutine(spawnLoop_Co);
            }

            spawnLoop_Co = StartCoroutine(SpawnLoop());
        }



        /// <summary>
        /// Converts List<EnemyRarity> to List<float> based on EnemyManager.normalRarityScale etc.
        /// </summary>
        /// <param name="enemiesToSpawn"></param>
        /// <returns></returns>
        private List<float> RarityToRadius(List<EnemyRarity> enemiesToSpawn)
        {
            List<float> spawnListForSampler = new List<float>();

            foreach (var item in enemiesToSpawn)
            {
                if (item == EnemyRarity.Normal)
                {
                    spawnListForSampler.Add(EnemyManager.normalRarityScale);
                }

                if (item == EnemyRarity.Magic)
                {
                    spawnListForSampler.Add(EnemyManager.magicRarityScale);
                }

                if (item == EnemyRarity.Rare)
                {
                    spawnListForSampler.Add(EnemyManager.rareRarityScale);
                }

                if (item == EnemyRarity.Unique)
                {
                    spawnListForSampler.Add(EnemyManager.uniqueRarityScale);
                }
            }

            return spawnListForSampler;
        }



        private List<float> CreateDebugSpawnList()
        {
            print("CreateDebugSpawnList");

            spawnDirector.totalMoney = 1;
            List<EnemyRarity> enemiesToSpawn = spawnDirector.CreateSpawnList();

            print($"total enemies to spawn {enemiesToSpawn.Count}");
            print($"normal: {enemiesToSpawn.Count(e => e == EnemyRarity.Normal)}");
            print($"magic: {enemiesToSpawn.Count(e => e == EnemyRarity.Magic)}");
            print($"rare: {enemiesToSpawn.Count(e => e == EnemyRarity.Rare)}");
            print($"unique: {enemiesToSpawn.Count(e => e == EnemyRarity.Unique)}");


            List<float> spawnListForSampler = new List<float>();

            foreach (var item in enemiesToSpawn)
            {
                if (item == EnemyRarity.Normal)
                {
                    spawnListForSampler.Add(EnemyManager.normalRarityScale);
                }

                if (item == EnemyRarity.Magic)
                {
                    spawnListForSampler.Add(EnemyManager.magicRarityScale);
                }

                if (item == EnemyRarity.Rare)
                {
                    spawnListForSampler.Add(EnemyManager.rareRarityScale);
                }

                if (item == EnemyRarity.Unique)
                {
                    spawnListForSampler.Add(EnemyManager.uniqueRarityScale);
                }
            }

            return spawnListForSampler;
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                var spawnlist = CreateDebugSpawnList();
                var toSpawnCount = spawnlist.Count;

                var regionSize = 30f;
                var padding = 0f;

                List<PDiscSampler.Obj> samplerPoints = Sampler(regionSize, new List<float>(spawnlist), padding);

                int tries = 0;
                int limit = 4;

                if (samplerPoints.Count < toSpawnCount)
                {
                    while (samplerPoints.Count < toSpawnCount)
                    {
                        tries++;
                        print("points.count: " + samplerPoints.Count + " increase region size: " + regionSize + " tries: " + tries);

                        regionSize++;
                        samplerPoints = Sampler(regionSize, new List<float>(spawnlist), padding);

                        if (tries > 20)
                        {
                            throw new System.Exception("something wrong with enemy spawn");
                        }
                    }
                }




                if (samplerPoints.Count < toSpawnCount)
                {
                    print("failed spawning enemies");
                }



                var l = GameManager.Instance.GameLevel;
                var target = Helper.Map(l, 0, 100, startPosMinHeight, startPosMaxHeight);

                var randx = UnityEngine.Random.Range(-spawnAreaSize, spawnAreaSize);
                var randz = UnityEngine.Random.Range(-spawnAreaSize, spawnAreaSize);

                var pos = new Vector3(randx, target, randz);

                print("debug enemies start pos: " + pos);


                //StartCoroutine(SpawnCandidatesAroundPoint(pos, samplerPoints));
                //SpawnCandidatesAroundPoint(pos, samplerPoints);



                //Vector3 p = Vector3.zero;

                //p.x = debugKeySpawnPos.x + regionSize / 2;
                //p.y = debugKeySpawnPos.y + regionSize / 2;
                //p.z = debugKeySpawnPos.z + regionSize / 2;

                //enemySpawnerTest.position = p;

                //enemySpawnerTest.localScale = Vector3.one * regionSize;
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


        private Enemy SpawnEnemy(Enemy prefab, Transform parent, float size, EnemyRarity rarity, Vector3 startPos)
        {
            Enemy e = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            e.gameObject.layer = LayerMask.NameToLayer("Enemies");
            e.transform.parent = parent;

            e.OnDeath += EnemyManager.Instance.HandleEnemyDeath;

            EnemyManager.Instance.AddEnemy(e);

            e.ChangeSize(size);
            e.SetRarity(rarity);
            e.Init(startPos);

            return e;
        }


        private IEnumerator SpawnCandidatesAroundPoint(Vector3 point, List<MySphere> candidates, Transform parent)
        {
            print("SpawnCandidatesAroundPoint");
            var uniques = candidates.Where(c => c.radi == EnemyManager.uniqueRarityScale).ToList();
            var rares = candidates.Where(c => c.radi == EnemyManager.rareRarityScale).ToList();
            var magics = candidates.Where(c => c.radi == EnemyManager.magicRarityScale).ToList();
            var normals = candidates.Where(c => c.radi == EnemyManager.normalRarityScale).ToList();

            Vector3 startPos;


            foreach (var c in uniques)
            {
                if (uniqueEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(uniqueEnemyPrefab, parent, size: c.radi, EnemyRarity.Unique, startPos);
                yield return null;
            }

            foreach (var c in rares)
            {
                if (rareEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(rareEnemyPrefab, parent, size: c.radi, EnemyRarity.Rare, startPos);
                yield return null;
            }

            foreach (var c in magics)
            {
                if (magicEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(magicEnemyPrefab, parent, size: c.radi, EnemyRarity.Magic, startPos);
                yield return null;
            }

            foreach (var c in normals)
            {
                if (normalEnemyPrefab == null)
                {
                    break;
                }

                startPos = point + c.center;

                Enemy e = SpawnEnemy(normalEnemyPrefab, parent, size: c.radi, EnemyRarity.Normal, startPos);
                yield return null;
            }
        }


        private List<PDiscSampler.Obj> Sampler(float regionSize, List<float> spawnSizeList, float padding = 0f)
        {
            PDiscSampler sampler = new PDiscSampler(300, spawnSizeList, Vector3.one * regionSize, padding);
            return sampler.FindPoints();
        }

        #endregion

    }

    public class WeightedRandomEnemy
    {
        public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();
        public float totalMoney;

        public WeightedRandomEnemy()
        {
            entries.Add(new EnemySpawnEntry(EnemyRarity.Normal, 1, 0.9f));
            entries.Add(new EnemySpawnEntry(EnemyRarity.Magic, 10, 0.08f));
            entries.Add(new EnemySpawnEntry(EnemyRarity.Rare, 12, 0.0195f));
            entries.Add(new EnemySpawnEntry(EnemyRarity.Unique, 50, 0.0005f));
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










        /*







        function [ c r ] = randomSphere( dims )
        % creating one sphere at random inside [0..dims(1)]x[0..dims(2)]x...
        % radius and center coordinates are sampled from a uniform distribution 
        % over the relevant domain.
        %
        % output: c - center of sphere (vector cx, cy,... )
        %         r - radius of sphere (scalar)
        r = rand(1); % you might want to scale this w.r.t dims or other consideration
        c = r + rand( size(dims) )./( dims - 2*r ); % make sure sphere does not exceed boundaries






        */


    }











}
