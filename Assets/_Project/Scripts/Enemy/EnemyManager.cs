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

        public List<Enemy> aliveEnemies { get; private set; }

        public Transform enemiesHolder;

        public GoldCoinDrop coinDropBig, coinDropMedium, coinDropSmall;

        public Transform goldDropHolder;

        [SerializeField] private EnemySpawner enemySpawner;



        [SerializeField] private int maxEnemyLevel;


        [Tooltip("Speed at which enemies go with 1f speed multiplier")]
        [SerializeField] private int baseEnemySpeed;


        [Tooltip("Enemy health curve between 0-MaxEnemyLevel")]
        [SerializeField] private AnimationCurve enemyHealthCurve;


        [Tooltip("Base hp for enemies at level = 0 and multiplier = 1")]
        [SerializeField] private int baseHP;







        public Dictionary<EnemySO, ObjectPool<Enemy>> poolDict = new Dictionary<EnemySO, ObjectPool<Enemy>>();

        public Dictionary<EnemySO, EnemyCreator> creatorDict = new Dictionary<EnemySO, EnemyCreator>();




        void OnGUI()
        {
            var startY = 10;
            var margin = 12;

            foreach (var item in poolDict)
            {
                GUI.Label(new Rect(10, startY + margin * 0, 200, 20), $"Enemy: {item.Key.name}");
                GUI.Label(new Rect(10, startY + margin * 1, 200, 20), $"CountActive: {item.Value.CountActive}");
                GUI.Label(new Rect(10, startY + margin * 2, 200, 20), $"CountInactive: {item.Value.CountInactive}");
                GUI.Label(new Rect(10, startY + margin * 3, 200, 20), $"CountAll: {item.Value.CountAll}");
                GUI.Label(new Rect(10, startY + margin * 4, 200, 20), "--------------------------------");

                startY += margin * 7;
            }

        }




        public ObjectPool<Enemy> GetPoolForTypeOfEnemy(EnemySO enemySO)
        {
            return poolDict[enemySO];
        }


        /// <summary>
        /// For each type of enemy, setup an EnemyCreator and point the pool to that.
        /// Have a pool for each type of enemy.
        /// </summary>
        private void CreatePools()
        {
            foreach (EnemySO enemySO in GameManager.Instance.ScriptableObjects.Enemies)
            {
                // setup a creator
                var creator = new EnemyCreator();

                // create a new pool which gets its objects from the newly setup EnemyCreator 
                var p = new ObjectPool<Enemy>(creator.CreatePooledEnemy, OnTakeFromPool, OnReturnToPool);
                poolDict.Add(enemySO, p);


                // fill the EnemyCreator with relevant info.
                creator.enemySO = enemySO;
                creator.spawner = enemySpawner;
                creator.pool = p;
                creatorDict.Add(enemySO, creator);
            }
        }


        public void Setup()
        {
            CreatePools();
        }


        private void OnTakeFromPool(Enemy e)
        {
            print("OnTakeFromPool");
            e.gameObject.SetActive(true);
        }

        private void OnReturnToPool(Enemy e)
        {
            print("OnReturnToPool");
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
            aliveEnemies = new List<Enemy>();
            Instance = this;


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


        //internal List<Enemy> EnemiesInRange(Enemy e, float range, bool fromShell = false)
        //{
        //    return EnemiesInRange(e.transform.position, range, fromShell);
        //}


        //internal List<Enemy> EnemiesInRange(Vector3 pos, float range, bool fromShell = false)
        //{
        //    var toreturn = new List<Enemy>();

        //    for (int i = 0; i < aliveEnemies.Count; i++)
        //    {
        //        Enemy e = aliveEnemies[i];

        //        var vec = (e.transform.position - pos);
        //        float dist = vec.sqrMagnitude;

        //        if (fromShell)
        //        {
        //            if (e.rarity == EnemyRarity.Normal)
        //            {
        //                dist -= normalSpawnInfo.r * normalSpawnInfo.r;
        //            }

        //            if (e.rarity == EnemyRarity.Magic)
        //            {
        //                dist -= magicSpawnInfo.r * magicSpawnInfo.r;
        //            }

        //            if (e.rarity == EnemyRarity.Unique)
        //            {
        //                dist -= uniqueSpawnInfo.r * uniqueSpawnInfo.r;
        //            }

        //            if (e.rarity == EnemyRarity.Rare)
        //            {
        //                dist -= rareSpawnInfo.extends.sqrMagnitude;
        //            }
        //        }

        //        if (dist <= range * range)
        //        {
        //            toreturn.Add(e);
        //        }
        //    }

        //    return toreturn;
        //}

        internal void DestroyAllEnemies()
        {
            


            //foreach (Transform child in enemiesHolder)
            //{
            //    Destroy(child.gameObject);
            //}
        }

        //internal Enemy PickEnemyToChainTo(Enemy from, Enemy except = null)
        //{
        //    List<Enemy> potential = EnemiesInRange(from, GameManager.Instance.chainRange, true);

        //    if (potential.Count == 0)
        //    {
        //        return null;
        //    }

        //    var refined = potential.Where(p => p != except && p != from);

        //    if (refined.Count() == 0)
        //    {
        //        return null;
        //    }

        //    return refined.ElementAt(UnityEngine.Random.Range(0, refined.Count()));
        //}






        #endregion

    }
}
