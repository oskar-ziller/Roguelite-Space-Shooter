using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class FireBallProjectile : ProjectileBase
    {

        [SerializeField] private ParticleSystem explosionPS;

        public override void Awake()
        {
            base.Awake();
            ProjectileMover = new ArcMover(this);
        }

        public override bool HandleEnemyCollision()
        {
            collidingWith.TakeHit(CastBy);

            if (base.HandleEnemyCollision())
            {
                return true;
            }

            DoExplode();
            return false;
        }

        public void DoExplode()
        {
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.isKinematic = true;

            HideBody();
            DisableCollider();


            var sizeVariation = Random.Range(0.9f, 1.1f);

            // Set the particle starting size to radi * 2 and some variation
            
            // Particle uses vertex streams to communicate with the shader
            // to fade out over time
            ParticleSystem.MainModule main = explosionPS.main;
            main.startSize = CastBy.ExpRadi * 2 * sizeVariation;

            explosionPS.Play();

            var expRadiSqr = CastBy.ExpRadi * CastBy.ExpRadi;

            var potentials = EnemyManager.Instance.aliveEnemies.Where(e => e != null
            && e != collidingWith.gameObject
            && (e.transform.position - transform.position).sqrMagnitude < expRadiSqr).ToList();

            foreach (Enemy e in potentials)
            {
                e.TakeHit(CastBy, applyAilment: false);
            }
        }
    }
}
