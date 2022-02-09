using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class FireBallProjectile : ProjectileBase
    {

        [SerializeField] private Explosion explosionPrefab;




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
            DestroySelfSoft();

            var expHandler = Instantiate(explosionPrefab);
            expHandler.transform.parent = transform.parent;
            expHandler.transform.position = transform.position;
            expHandler.Setup(CastBy);
            expHandler.DoExplode();





        }
    }
}
