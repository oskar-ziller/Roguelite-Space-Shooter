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
        public Transform Holder;
        public List<Enemy> Enemies = new List<Enemy>();
        public Vector3 Position;
        public bool IsVisible;

        private void Update()
        {
            if (Enemies.Count > 0)
            {
                var e = Enemies.First();
                Position = e.transform.position;
                IsVisible = e.IsVisible();
            }

        }
    }
}
