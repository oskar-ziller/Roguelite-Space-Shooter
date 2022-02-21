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
        public PackSpawnInfo Info;
        public Vector3 Position;
        public bool IsVisible;

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
            e.KilledByPlayer += OnPackEnemyDeath;
            Enemies.Add(e);
        }

        private void OnPackEnemyDeath(Enemy e)
        {
            Enemies.Remove(e);

            if (Enemies.Count == 0)
            {
                EnemyManager.Instance.OnPackDeath(this);
                Destroy(gameObject);
            }
        }
    }
}
