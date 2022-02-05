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

            foreach (Enemy e in EnemyManager.Instance.EnemiesInRange(transform.position, CastBy.ExpRadi, fromShell: true))
            {
                if (e == collidingWith)
                {
                    continue;
                }

                e.TakeHit(CastBy, applyAilment: false);
            }

            chillingArea.Init(CastBy);
        }

    }

}
