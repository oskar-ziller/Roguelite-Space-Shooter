using DG.Tweening;
using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    
    public abstract class ProjectileBase : MonoBehaviour
    {
        [Tooltip("Mesh object of projectile to scale from 0 to meshScale as it gets away from cast point")]
        [SerializeField] private Transform mainMesh;

        [SerializeField] private Transform bodyMesh;

        [Tooltip("Scale to scale the mainMesh to match the Collider size")]
        [SerializeField] private float meshScaleMain;

        [Tooltip("Does projectile need aim assist (snap to enemies if close enough)")]
        [SerializeField] private bool aimAssist = false;

        public ProjectileSpawnInfo spawnInfo { get; private set; }

        public Vector3 Position { get { return rigidbody.position; } set { rigidbody.position = value; } }

        public Rigidbody Rigidbody => rigidbody;

        public float MeshScaleMain => meshScaleMain;





        public int CastProjCount { get; protected set; } // when single cast has multiple projectiles
        public Vector3 StartedMovingFrom { get; protected set; }
        public IMover ProjectileMover { get; protected set; }




        public Transform MainMesh => mainMesh;


        [Tooltip("with LMP etc. how spread each proj should be from their center")]
        public float SpreadFromCenter; 

        public float ScaleDur;
        public float PathDur;
        public float PathLen;

        protected Enemy collidingWith;

        protected Enemy ForkingFrom;
        private Enemy PiercingFrom;

        private float spawnTime;
        private float expireTime;
        private bool isSetup = false;

        private List<Enemy> PiercedFrom;
        protected List<Enemy> ChainedFrom;
        private List<Enemy> ForkedFrom;

        private SphereCollider collider;


        float forkSpeed = 110f;
        float forkRadi = 0.9f;

        private Rigidbody rigidbody;



        internal void Setup(ProjectileSpawnInfo info)
        {
            spawnInfo = info;

            CastProjCount = spawnInfo.CastBy.Spell.ProjectileCount + (int)info.CastBy.GetTotal("AdditionalProjectiles");

            CalculateExpireTime();

            isSetup = true;
        }


        private void CalculateExpireTime()
        {
            spawnTime = Time.time;
            var baseLifetime = spawnInfo.CastBy.Modifiers.ProjectileLifetimeCalcd;
            var inceasedBy = spawnInfo.CastBy.GetTotal("IncreasedEffectDuration");
            var final = baseLifetime * inceasedBy;

            expireTime = spawnTime + final;
        }

        internal void OverrideExpireDuration(float newExpireDuration)
        {
            expireTime = Time.time + newExpireDuration;
        }

        public virtual void Move()
        {
            transform.LookAt(spawnInfo.AimingAt);
            StartedMovingFrom = Rigidbody.position;
            ProjectileMover.Move();
            ScaleProjectileWhileMove();
        }

        /// <summary>
        /// Returns true if collision is handled by base class.
        /// Returns false if base class has no further action to take on collision.
        /// </summary>
        /// <returns></returns>
        public virtual bool HandleEnemyCollision()
        {
            if (ShouldPierce())
            {
                DoPierce();
                return true;
            }

            if (ShouldFork())
            {
                if (DoFork())
                {
                    Die();
                    return true;
                }
            }

            if (ShouldChain())
            {
                if (DoChain())
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void Awake()
        {
            PiercedFrom = new List<Enemy>();
            ChainedFrom = new List<Enemy>();
            ForkedFrom = new List<Enemy>();

            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<SphereCollider>();
        }




        internal virtual void Expire()
        {
            Die();
        }

        private bool ShouldExpire()
        {
            return Time.time > expireTime && isSetup;
        }


        protected virtual void Die()
        {
            transform.DOKill();
            Destroy(gameObject);
        }


        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
            if (ShouldExpire())
            {
                Expire();
            }
        }






        public virtual void OnTriggerEnter(Collider collider)
        {
            if (!collider.gameObject.activeInHierarchy)
            {
                return;
            }

            Enemy collidedEnemy = collider.gameObject.GetComponent<Enemy>();

            if (collidedEnemy != null)
            {
                if (ForkingFrom == collidedEnemy)
                {
                    return;
                }

                if (PiercingFrom == collidedEnemy)
                {
                    return;
                }

                if (collidedEnemy.IsDying)
                {
                    return;
                }

                collidingWith = collidedEnemy;

                bodyMesh.DOKill();
                transform.DOKill();
                HandleEnemyCollision();
            }
        }



        public bool ShouldPierce()
        {
            bool always = spawnInfo.CastBy.GetTotal("AlwaysPierce") > 0;

            if (always)
            {
                return true;
            }

            int count = (int)spawnInfo.CastBy.GetTotal("PierceAdditionalTimes");

            if (count == 0)
            {
                return false;
            }

            if (PiercedFrom.Count >= count)
            {
                return false;
            }

            return true;
        }

        public void DoPierce()
        {
            PiercedFrom.Add(collidingWith);
            PiercingFrom = collidingWith;
        }

        public bool ShouldChain()
        {
            if (ChainedFrom.Count >= spawnInfo.CastBy.GetTotal("ChainAdditionalTimes"))
            {
                return false;
            }

            return true;
        }

        public virtual bool DoChain()
        {
            //var potentials = EnemyManager.Instance.AliveEnemies.Where(e => e != null
            //&& e.gameObject != collidingWith.gameObject
            //&& (e.transform.position - transform.position).sqrMagnitude < chainRangeSqr);

            var pack = collidingWith.BelongsToPack;
            var potentials = pack.EnemiesInPack.Where(p => p != collidingWith);

            Enemy e = potentials.Count() > 0 ?
                potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;

            if (e != null)
            {
                ScaleProjectile(forkRadi);
                SetVelocityTowards(e.transform.position, forkSpeed);
                ChainedFrom.Add(collidingWith);
                return true;
            }

            print("enemyToChainTo == null");
            return false;
        }

        public bool ShouldFork()
        {
            if (ForkedFrom.Count >= spawnInfo.CastBy.GetTotal("ForkAdditionalTimes"))
            {
                return false;
            }

            return true;
        }

        public virtual bool DoFork()
        {
            // * When a projectile forks, it splits into two identical projectiles
            // * that continue travelling at 60 and -60 degree angles
            // * from the projectile's original trajectory. */

            //var chainRangeSqr = GameManager.Instance.ChainAndForkRange * GameManager.Instance.ChainAndForkRange;

            //var potentials = EnemyManager.Instance.AliveEnemies.Where(e => e != null
            //&& e.gameObject != collidingWith.gameObject
            //&& (e.transform.position - transform.position).sqrMagnitude < chainRangeSqr);

            var pack = collidingWith.BelongsToPack;
            var potentials = pack.EnemiesInPack.Where(p => p != collidingWith);

            Enemy e = potentials.Count() > 0 ?
                potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;

            if (e == null)
            {
                print("can't fork, not enough enemies");
                return false;
            }

            ForkedFrom.Add(collidingWith);
            ForkingFrom = collidingWith;

            var potentialExceptFirst = potentials.Where(p => p != e);

            Enemy e2 = potentialExceptFirst.Count() > 0 ?
                potentialExceptFirst.ElementAt(UnityEngine.Random.Range(0, potentialExceptFirst.Count())) : null;

            if (e2 == null)
            {
                print("use same enemy for fork");
                e2 = e;
            }

            ProjectileBase clone = Instantiate(this, transform.position, Quaternion.identity);
            ProjectileBase clone2 = Instantiate(this, transform.position, Quaternion.identity);

            clone.CopyFrom(this);
            clone2.CopyFrom(this);

            clone.ScaleProjectile(forkRadi);
            clone2.ScaleProjectile(forkRadi);

            clone.SetVelocityTowards(e.transform.position, forkSpeed);
            clone2.SetVelocityTowards(e2.transform.position, forkSpeed);

            return true;
        }

        private void SetVelocityTowards(Vector3 position, float speed)
        {
            Rigidbody.isKinematic = false;

            var dir = (position - transform.position).normalized;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.AddForce(dir * speed, ForceMode.VelocityChange);
        }

        private void CopyFrom(ProjectileBase p)
        {
            this.spawnInfo = p.spawnInfo;
            this.ForkingFrom = p.ForkingFrom;
            this.ForkedFrom = new List<Enemy>(p.ForkedFrom);
            this.ChainedFrom = new List<Enemy>(p.ChainedFrom);
            this.PiercedFrom = new List<Enemy>(p.PiercedFrom);
            this.expireTime = p.expireTime;
            this.isSetup = true;
        }



        protected void DisableRigidBody()
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
        }


        //public float scaleTime = 0.5f;

        private void ScaleProjectileWhileMove()
        {
            bodyMesh.localScale = Vector3.zero;
            bodyMesh.DOScale(MeshScaleMain, ScaleDur);
            transform.DOScale(1f, ScaleDur);
        }

        private void ScaleProjectile(float newScale)
        {
            bodyMesh.localScale = Vector3.one * newScale;
            collider.radius = newScale / 2f;
        }

    }
}
