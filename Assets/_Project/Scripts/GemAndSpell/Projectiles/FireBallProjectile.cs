using DG.Tweening;
using System.Collections;
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

            var totalRadi = CastBy.ExpRadi;

            var sizeVariation = Random.Range(0.9f, 1.1f);

            var main = explosionPS.main;
            main.startSize = totalRadi * 2 * sizeVariation;

            explosionPS.Play();

            foreach (Enemy e in EnemyManager.Instance.EnemiesInRange(transform.position, totalRadi, fromShell: true))
            {
                if (e == collidingWith)
                {
                    continue;
                }

                e.TakeHit(CastBy, applyAilment: false);
            }
        }
    }
}
