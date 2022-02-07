using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public partial class BallLightningProjectile : ProjectileBase
    {
        IMover _projectileMover;

        [Tooltip("Mesh around ball lightning")]
        [SerializeField] private Transform ballMesh;

        [Tooltip("Default radius of the ball")]
        [SerializeField] private float defaultRadius;

        [Tooltip("How often does the projectile zap enemies in radius (milliseconds)")]
        [SerializeField] private float zapInterval;

        [Tooltip("List of particles to play, on every zap")]
        [SerializeField] private List<ParticleSystem> zapParticles;

        [Tooltip("Collider to scale to bubble size")]
        [SerializeField] private SphereCollider sphereCollider;

        private Coroutine zapLoop;


        private void Begin()
        {
            float increasedBy = CastBy.GetTotal("IncreasedAoe");
            var totalRadi = defaultRadius * (1 + increasedBy * 2);

            ballMesh.localScale *= totalRadi;
            sphereCollider.radius *= totalRadi;
            ScaleZapEffects(totalRadi);

            zapLoop = StartCoroutine(ZapLoop());
        }

        public override void Awake()
        {
            base.Awake();
            _projectileMover = new LinearMover(this);
        }



        public override bool HandleEnemyCollision()
        {
            if (base.HandleEnemyCollision())
            {
                return true;
            }

            return false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Enemy collidedEnemy = other.transform.gameObject.GetComponent<Enemy>();
                float now = Time.time;

                if (!collidedEnemy.zapDict.ContainsKey(CastID))
                {
                    //collidedEnemy.TakeDamage(DamageType.DoT);
                    collidedEnemy.zapDict.Add(CastID, now);
                    collidedEnemy.TakeHit(CastBy);
                    return;
                }

                if (now - collidedEnemy.zapDict[CastID] > zapInterval / 1000f)
                {
                    collidedEnemy.zapDict[CastID] = now;
                    collidedEnemy.TakeHit(CastBy);
                    return;
                }
            }
        }

        private void ScaleZapEffects(float scale)
        {
            foreach (ParticleSystem ps in zapParticles)
            {
                ps.transform.localScale *= scale / 4;
            }
        }

        private void PlayZapEffects()
        {
            foreach (ParticleSystem ps in zapParticles)
            {
                ps.Play();
            }
        }

        private IEnumerator ZapLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(zapInterval / 1000f);
                PlayZapEffects();
                yield return null;
            }
        }










    }

}
