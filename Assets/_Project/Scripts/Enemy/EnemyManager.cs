using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        public List<Enemy> aliveEnemies { get; private set; }

        public Transform enemiesHolder;

        #region Variables

        public const float normalRarityScale = 3f;
        public const float magicRarityScale = 6f;
        public const float rareRarityScale = 20f;
        public const float uniqueRarityScale = 25f;

        public float enemySpeed = 0.5f;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            aliveEnemies = new List<Enemy>();
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void Start()
        {
        
        }

        private void Update()
        {

            if (GameManager.Instance.waitingForChallenge)
            {
                if (aliveEnemies.Count == 0)
                {
                    GameManager.Instance.StopChallenge();
                }
            }
        
        }

        #endregion

        #region Methods

        internal void HandleEnemyDeath(Enemy e)
        {
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


        internal List<Enemy> EnemiesInRange(Enemy e, float range, bool fromShell = false)
        {
            return EnemiesInRange(e.transform.position, range, fromShell);
        }



        internal List<Enemy> EnemiesInRange(Vector3 pos, float range, bool fromShell = false)
        {
            var toreturn = new List<Enemy>();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                Enemy e = aliveEnemies[i];

                float dist = Vector3.Distance(e.transform.position, pos);

                if (fromShell)
                {
                    dist -= e.transform.localScale.x / 2;
                }

                if (dist <= range)
                {
                    toreturn.Add(e);
                }
            }

            return toreturn;
        }

        internal void DestroyAllEnemies()
        {
            var copy = new List<Enemy>(aliveEnemies);

            foreach (var e in copy)
            {
                e.ForceDie();
            }


            foreach (Transform child in enemiesHolder)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        internal Enemy PickEnemyToChainTo(Enemy from, Enemy except = null)
        {
            List<Enemy> potential = EnemiesInRange(from, GameManager.Instance.chainRange, true);

            if (potential.Count == 0)
            {
                return null;
            }

            var refined = potential.Where(p => p != except && p != from);

            if (refined.Count() == 0)
            {
                return null;
            }

            return refined.ElementAt(UnityEngine.Random.Range(0, refined.Count()));
        }

        #endregion

    }
}
