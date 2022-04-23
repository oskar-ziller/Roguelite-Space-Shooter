using MeteorGame.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MeteorGame
{
    public class EnemyPack : MonoBehaviour
    {
        public PackSpawnInfo Info { get; internal set; }
        public Vector3 Centroid { get; internal set; }
        public bool IsVisible { get; internal set; }
        public float Speed { get; internal set; }
        public int PackSize { get; internal set; }
        public SpawnAreaVisual SpawnAreaVisual { get; internal set; }
        public PackBox PackBox { get; internal set; }
        public List<Enemy> EnemiesInPack { get; private set; }

        private bool isActivated = false;

        private void Update()
        {
            if (EnemiesInPack.Count > 0)
            {
                // use 1st alive enemy in pack to update IsVisible
                var e = EnemiesInPack.First();
                IsVisible = e.IsVisible();
            }
        }

        private void Awake()
        {
            EnemiesInPack = new List<Enemy>();
        }

        private void FixedUpdate()
        {
            if (isActivated)
            {
                UpdatePackCenter();
            }
        }

        private Vector3 FindCenterOfEnemiesAlive()
        {
            if (EnemiesInPack.Count > 0)
            {
                var bound = new Bounds(EnemiesInPack[0].transform.position, EnemiesInPack[0].SO.ShapeRadi * 2f * Vector3.one);

                for (int i = 1; i < EnemiesInPack.Count; i++)
                {
                    var enemyBound = new Bounds(EnemiesInPack[i].transform.position, EnemiesInPack[i].SO.ShapeRadi * 2f * Vector3.one);
                    bound.Encapsulate(enemyBound);
                }

                PackSize = Mathf.RoundToInt(bound.size.x) + 15;
                return bound.center;
            }
            else
            {
                return Vector3.zero;
            }
        }

        private void UpdatePackCenter()
        {
            Centroid = FindCenterOfEnemiesAlive();
        }


        public void AddEnemy(Enemy e)
        {
            EnemiesInPack.Add(e);
            e.transform.SetParent(transform);
            EnemyManager.Instance.OnPackEnemyCreated(e);
        }


        public void ForceDie()
        {
            for (int i = EnemiesInPack.Count - 1; i >= 0; i--)
            {
                Enemy e = EnemiesInPack[i];
                e.ForceDie();
            }
        }

        public void OnPackEnemyDeath(Enemy e)
        {
            EnemiesInPack.Remove(e);
            EnemyManager.Instance.OnPackEnemyDeath(e);

            if (EnemiesInPack.Count == 0)
            {
                EnemyManager.Instance.OnPackDeath(this);
                Destroy(gameObject);
            }
        }

        private void ActivatePack()
        {
            if (!isActivated)
            {
                foreach (Enemy enemy in EnemiesInPack)
                {
                    enemy.Activate();
                }

                isActivated = true;
            }
            else
            {
                Debug.LogError("Trying to activate an active pack");
            }
        }


        internal void OnSpawnAnimCompleted()
        {
            SpawnAreaVisual.FadeOut();
            PackBox.FadeIn();
            Info.spawner.EndSpawnAnim();
            ActivatePack();
        }

        internal void CalculatePackMovementSpeed()
        {
            // calculate average movement speed
            var avgTestVal = 0f;

            foreach (Enemy e in EnemiesInPack)
            {
                avgTestVal += e.SO.HealthMultiplier / e.SO.SpeedMultiplier;
            }

            avgTestVal /= EnemiesInPack.Count;
            Speed = 1f / avgTestVal;
        }

    }
}
