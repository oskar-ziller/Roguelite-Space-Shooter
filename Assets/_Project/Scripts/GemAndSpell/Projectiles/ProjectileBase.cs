using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        [Tooltip("The main body group of the projectile")]
        [SerializeField] private GameObject bodyGroup;

        [SerializeField] private Rigidbody rigidBody;

        [Tooltip("We need to desroy projectile's collider when begin explode etc")]
        [SerializeField] private SphereCollider projectileCollider;

        [Tooltip("Does projectile need aim assist (snap to enemies if close enough)")]
        [SerializeField] private bool aimAssist = false;

        public Vector3 Position { get { return rigidBody.position; } set { rigidBody.position = value; } }

        public Rigidbody Rigidbody { get { return rigidBody; } }

        public Enemy AimingAtEnemy { get; protected set; }
        public int CastID { get; protected set; }
        public int ProjectileID { get; protected set; } // when single cast has multiple projectiles
        public int TotalProjectiles { get; protected set; } // when single cast has multiple projectiles
        public float StartingSpeed { get; protected set; }
        public Vector3 MovingTowards { get; protected set; }
        public Vector3 StartedMovingFrom { get; protected set; }
        public IMover ProjectileMover { get; protected set; }
        public SpellSlot CastBy { get; protected set; }

        protected Enemy collidingWith;

        private Enemy ForkingFrom;
        private Enemy PiercingFrom;

        private float spawnTime;
        private float expireTime;
        private bool collided;
        private bool isSetup = false;
        private bool isDummy = false;

        private List<Enemy> PiercedFrom;
        private List<Enemy> ChainedFrom;
        private List<Enemy> ForkedFrom;

        private List<TrailRenderer> trailRenderers;
        private float maxTrailDur = 0f;

        private SpinAround spinner;

        public void Setup(SpellSlot castBySlot, Vector3 aimingAt, Enemy hitEnemy, int castID, int projectileID)
        {
            var spell = castBySlot.Spell;

            StartingSpeed = spell.ProjectileSpeed;
            TotalProjectiles = spell.ProjectileCount;
            CastBy = castBySlot;
            MovingTowards = aimingAt;
            AimingAtEnemy = hitEnemy;
            CastID = castID;
            ProjectileID = projectileID;  // when single cast has multiple projectiles

            CalculateExpireTime();


            // look at where we are moving towards (is assumed in Movers this is the case)
            transform.LookAt(aimingAt);

            

            isSetup = true;
        }

        public void SetProjectileID(int id)
        {
            ProjectileID = id;
        }

        private void CalculateExpireTime()
        {
            spawnTime = Time.time;
            var baseLifetime = CastBy.Spell.LifeTime;
            var inceasedBy = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("IncreasedSkillEffectDuration"), CastBy) / 100f;

            inceasedBy *= 1.5f;

            var final = baseLifetime * (1 + inceasedBy);

            expireTime = spawnTime + final;
        }

        public virtual void Move()
        {
            StartedMovingFrom = rigidBody.position;
            ProjectileMover.Move();
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
                    DestroySelfSoft();
                    return true;
                }
            }

            if (ShouldChain())
            {
                DoChain();
                return true;
            }

            return false;
        }

        public virtual void Awake()
        {
            PiercedFrom = new List<Enemy>();
            ChainedFrom = new List<Enemy>();
            ForkedFrom = new List<Enemy>();

            var trails = GetComponentsInChildren<TrailRenderer>();

            if (trails != null)
            {
                trailRenderers = trails.ToList();
            }

            maxTrailDur = trailRenderers.Max(t => t.time);

            spinner = GetComponent<SpinAround>();
        }


        internal virtual void OnTriggerExit(Collider other)
        {
            ForkingFrom = null;
            PiercingFrom = null;
        }


        internal virtual void Expire()
        {
            rigidBody.DOKill();
            DestroySelfSoft();
        }

        private bool ShouldExpire()
        {
            return Time.time > expireTime && isSetup;
        }


        private bool ShouldAimAssist()
        {
            return aimAssist && AimingAtEnemy == null && !collided && ForkedFrom.Count == 0;
        }

        private void DoAimAssist()
        {
            var closest = EnemyManager.Instance.EnemiesInRange(transform.position, 2, true).FirstOrDefault();

            if (closest != null)
            {
                var dir = closest.transform.position - transform.position;

                rigidBody.DOKill();
                rigidBody.isKinematic = false;

                rigidBody.velocity = dir.normalized * StartingSpeed;
            }
        }

        public virtual void FixedUpdate()
        {
            if (isDummy)
            {
                return;
            }

            if (ShouldExpire())
            {
                Expire();
            }

            if (ShouldAimAssist())
            {
                DoAimAssist();
            }
        }

        public virtual void OnTriggerEnter(Collider colliderObj)
        {
            Enemy collidedEnemy = colliderObj.gameObject.GetComponent<Enemy>();

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

                collided = true;
                collidingWith = collidedEnemy;

                rigidBody.DOKill();
                HandleEnemyCollision();
            }
        }

        public bool ShouldPierce()
        {
            bool always = ModifierHelper.ModifierExists(GameManager.Instance.GetModifierSO("AlwaysPierce"), CastBy);

            if (always)
            {
                return true;
            }

            int count = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("PierceAdditionalTimes"), CastBy);

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
            int shouldChainCount = ModifierHelper.GetTotal("ChainAdditionalTimes", CastBy);

            if (ChainedFrom.Count >= shouldChainCount)
            {
                return false;
            }

            return true;
        }

        public void DoChain()
        {
            ChainedFrom.Add(collidingWith);

            Enemy enemyToChainTo = EnemyManager.Instance.PickEnemyToChainTo(collidingWith);

            if (enemyToChainTo == null)
            {
                print("enemyToChainTo == null");
                // If a projectile has no valid chain targets,
                // it will target a nearby enemy but pass through without hitting them.
                return;
            }

            SetVelocityTowards(enemyToChainTo.transform.position, StartingSpeed);
        }

        public bool ShouldFork()
        {
            int count = ModifierHelper.GetTotal("ForkAdditionalTimes", CastBy);

            if (count == 0)
            {
                return false;
            }

            if (ForkedFrom.Count >= count)
            {
                return false;
            }

            return true;
        }

        public bool DoFork()
        {
            /* When a projectile forks, it splits into two identical projectiles
             * that continue travelling at 60 and -60 degree angles
             * from the projectile's original trajectory. */
            ForkedFrom.Add(collidingWith);
            ForkingFrom = collidingWith;

            Enemy enemyToChainTo = EnemyManager.Instance.PickEnemyToChainTo(collidingWith);
            Enemy enemyToChainTo2 = EnemyManager.Instance.PickEnemyToChainTo(collidingWith, enemyToChainTo);

            if (enemyToChainTo == null)
            {
                return false;
            }

            if (enemyToChainTo2 == null)
            {
                enemyToChainTo2 = enemyToChainTo;
            }

            ProjectileBase clone = Instantiate(this, transform.position, Quaternion.identity);
            ProjectileBase clone2 = Instantiate(this, transform.position, Quaternion.identity);

            clone.CopyFrom(this);
            clone2.CopyFrom(this);

            clone.SetVelocityTowards(enemyToChainTo.transform.position, StartingSpeed);
            clone2.SetVelocityTowards(enemyToChainTo2.transform.position, StartingSpeed);

            return true;
        }

        private void SetVelocityTowards(Vector3 position, float speed)
        {
            rigidBody.isKinematic = false;
            transform.LookAt(position);

            var dir = (position - transform.position).normalized;
            rigidBody.velocity = Vector3.zero;
            rigidBody.AddForce(dir * speed, ForceMode.VelocityChange);
        }

        private void CopyFrom(ProjectileBase p)
        {
            this.StartingSpeed = p.StartingSpeed;
            this.ForkingFrom = p.ForkingFrom;
            this.ForkedFrom = new List<Enemy>(p.ForkedFrom);
            this.ChainedFrom = new List<Enemy>(p.ChainedFrom);
            this.PiercedFrom = new List<Enemy>(p.PiercedFrom);
        }

        protected void DisableCollider()
        {
            projectileCollider.enabled = false;
        }

        protected void EnableCollider()
        {
            projectileCollider.enabled = true;
        }

        protected void HideBody()
        {
            bodyGroup.SetActive(false);
        }

        protected void DestroySelfSoft()
        {
            HideBody();
            DisableCollider();

            Destroy(gameObject, maxTrailDur);
        }

        public void EnableSpinner()
        {
            spinner.enabled = true;
        }

        public void MakeDummy()
        {
            DisableCollider();
            Rigidbody.isKinematic = true;
            isDummy = true;

            foreach (var tr in trailRenderers)
            {
                tr.enabled = false;
            }
        }

        public void MakeNormal()
        {
            EnableCollider();
            Rigidbody.isKinematic = false;
            isDummy = false;
            spinner.enabled = false;

            foreach (var tr in trailRenderers)
            {
                tr.enabled = true;
            }
        }
    }
}
