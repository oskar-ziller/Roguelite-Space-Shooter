using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public partial class CreepingFrostProjectile : ProjectileBase
    {
        [SerializeField] private ChillingArea chillingArea;

        public override void Awake()
        {
            base.Awake();
            ProjectileMover = new LinearMover(this);
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


        public void DoExplode() // explode and spawn chilling area
        {
            //print("DoExplode()");
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.isKinematic = true;

            HideBody();
            DisableCollider();

            var expRadiSqr = CastBy.ExpRadi * CastBy.ExpRadi;

            var potentials = EnemyManager.Instance.aliveEnemies.Where(e => e != null
            && e != collidingWith.gameObject
            && (e.transform.position - transform.position).sqrMagnitude < expRadiSqr);

            foreach (Enemy e in potentials)
            {
                e.TakeHit(CastBy, applyAilment: false);
            }

            chillingArea.Init(CastBy);
        }

    }

}
