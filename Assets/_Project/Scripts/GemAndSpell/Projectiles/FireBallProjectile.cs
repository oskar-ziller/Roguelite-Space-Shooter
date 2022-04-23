using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class FireBallProjectile : ProjectileBase
    {

        [SerializeField] private Explosion explosionPrefab;

        [Tooltip("Trail before chain or fork")]
        [SerializeField] private TrailRenderer normalTrail;

        [SerializeField] private TrailRenderer forkTrail;
        [SerializeField] private TrailRenderer chainTrail;




        public override void Awake()
        {
            base.Awake();
            ProjectileMover = new ArcMover(this);
        }

        public override void Update()
        {
            base.Update();

            if (ForkingFrom != null && ChainedFrom.Count == 0 && !forkTrail.emitting)
            {
                normalTrail.emitting = false;
                forkTrail.emitting = true;
            }
        }


        public override bool HandleEnemyCollision()
        {
            collidingWith.TakeHit(spawnInfo.CastBy);

            if (base.HandleEnemyCollision())
            {
                return true;
            }

            DoExplode();
            return false;
        }

        public override bool DoChain()
        {
            OverrideExpireDuration(2f);

            if (base.DoChain())
            {
                if (!chainTrail.emitting)
                {
                    chainTrail.emitting = true;
                    forkTrail.emitting = false;
                    normalTrail.emitting = false;
                }

                return true;
            }

            return false;
        }

        public override bool DoFork()
        {
            OverrideExpireDuration(2f);
            return base.DoFork();
        }

        public void DoExplode()
        {
            var expHandler = Instantiate(explosionPrefab);
            expHandler.transform.parent = transform.parent;
            expHandler.transform.position = transform.position;
            expHandler.Setup(spawnInfo.CastBy);
            expHandler.DoExplode();

            Die();
        }


        private void SetTrailParents()
        {
            chainTrail.transform.SetParent(null);
            forkTrail.transform.SetParent(null);
        }


        protected override void Die()
        {
            SetTrailParents();
            base.Die();
        }

    }
}
