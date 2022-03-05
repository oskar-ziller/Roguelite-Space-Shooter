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
        public Vector3 Position { get; internal set; }
        public bool IsVisible { get; internal set; }
        public float Speed { get; internal set; }

        private List<Enemy> Enemies;


        private void Update()
        {
            if (Enemies.Count > 0)
            {
                // use 1st alive enemy in pack to
                // update IsVisible and Position of the pack
                var e = Enemies.First();
                Position = e.transform.position;
                IsVisible = e.IsVisible();
            }
        }

        private void Awake()
        {
            print("enemypack awake");
            Enemies = new List<Enemy>();
        }

        public void AddEnemy(Enemy e)
        {
            e.Died += OnPackEnemyDeath;
            Enemies.Add(e);
        }

        public void ForceDie()
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemy e = Enemies[i];
                e.ForceDie();
            }
        }

        private void OnPackEnemyDeath(Enemy e, bool _)
        {
            Enemies.Remove(e);

            if (Enemies.Count == 0)
            {
                EnemyManager.Instance.OnPackDeath(this);
                Destroy(gameObject);
            }
        }

        internal void DoSpawn()
        {
            foreach (var e in Enemies)
            {
                e.DoSpawn();
            }
        }

        internal void CalculatePackMovementSpeed()
        {
            // calculate average movement speed
            var avgTestVal = 0f;
            foreach (Enemy e in Enemies)
            {
                avgTestVal += e.SO.HealthMultiplier / e.SO.SpeedMultiplier;
            }


            avgTestVal /= Enemies.Count;


            Speed = 1f / avgTestVal;
        }

    }
}
