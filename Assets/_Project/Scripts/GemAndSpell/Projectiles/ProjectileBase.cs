using DG.Tweening;
using System;
using System.Collections;
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

        [Tooltip("Does projectile need aim assist (snap to enemies if close enough)")]
        [SerializeField] private bool aimAssist = false;

        [Tooltip("Trigger collider for projectile")]
        [SerializeField] private Collider mainProjCollider;

        public Vector3 Position { get { return rigidbody.position; } set { rigidbody.position = value; } }

        public Rigidbody Rigidbody { get { return rigidbody; } }


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



        //protected HashSet<GameObject> InExplosionRange => inExplosionRange;
        //protected HashSet<GameObject> InChainRange => inChainRange;
        //protected HashSet<GameObject> InAimAssistRange => inAimAssistRange;

        private List<TrailRenderer> trailRenderers;
        private float maxTrailDur = 0f;

        private SpinAround spinner;

        private Rigidbody rigidbody;

        //private HashSet<GameObject> inExplosionRange = new HashSet<GameObject>();
        //private HashSet<GameObject> inChainRange = new HashSet<GameObject>();
        //private HashSet<GameObject> inAimAssistRange = new HashSet<GameObject>();

        private bool aimAssisted = false;
        private bool expiring;

        public void SetCastBy(SpellSlot spellSlot)
        {
            CastBy = spellSlot;
        }

        public void Setup(SpellSlot castBySlot, Vector3 aimingAt, Enemy hitEnemy, int castID, int projectileID)
        {
            StartingSpeed = castBySlot.ProjectileSpeed;
            TotalProjectiles = castBySlot.ProjectileCount;
            CastBy = castBySlot;
            MovingTowards = aimingAt;
            AimingAtEnemy = hitEnemy;
            CastID = castID;
            ProjectileID = projectileID;  // when single cast has multiple projectiles

            CalculateExpireTime();


            // look at where we are moving towards (is assumed in Movers this is the case)
            transform.LookAt(aimingAt);
            //SetTriggers();

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
            var inceasedBy = CastBy.GetTotal("IncreasedSkillEffectDuration") / 100f;
            var final = baseLifetime * (1 + inceasedBy);

            expireTime = spawnTime + final;
        }

        public virtual void Move()
        {
            StartedMovingFrom = Rigidbody.position;
            ProjectileMover.Move();
        }



        private void ResetScale()
        {
            transform.localScale = Vector3.one;
        }

        private void ScaleToDummyScale()
        {
            transform.localScale = Vector3.one / CastBy.ProjectileCount;
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

            rigidbody = GetComponent<Rigidbody>();
            //projectileCollider = GetComponent<SphereCollider>();

            //explTrigger.TriggerEnter += OnExpTriggerEnter;
            //explTrigger.TriggerExit += OnExpTriggerExit;

            //chainTrigger.TriggerEnter += OnChainTriggerEnter;
            //chainTrigger.TriggerExit += OnChainTriggerExit;

            //aimAssistTrigger.TriggerEnter += OnAimAssistTriggerEnter;
            //aimAssistTrigger.TriggerExit += OnAimAssistTriggerExit;
        }

        //private void OnAimAssistTriggerExit(Collider obj)
        //{
        //    InAimAssistRange.Remove(obj.gameObject);
        //}

        //private void OnAimAssistTriggerEnter(Collider obj)
        //{
        //    InAimAssistRange.Add(obj.gameObject);
        //}

        //private void OnChainTriggerExit(Collider obj)
        //{
        //    InChainRange.Remove(obj.gameObject);
        //}

        //private void OnChainTriggerEnter(Collider obj)
        //{
        //    InChainRange.Add(obj.gameObject);
        //}


        //public virtual void OnExpTriggerEnter(Collider obj)
        //{
        //    InExplosionRange.Add(obj.gameObject);
        //}


        //public virtual void OnExpTriggerExit(Collider obj)
        //{
        //    InExplosionRange.Remove(obj.gameObject);
        //}



        internal virtual void Expire()
        {
            expiring = true;
            Rigidbody.DOKill();
            DestroySelfSoft();
        }

        private bool ShouldExpire()
        {
            return !expiring && Time.time > expireTime && isSetup;
        }




        private void DoAimAssist()
        {
            //var dir = InAimAssistRange.First().transform.position - transform.position;

            //Rigidbody.DOKill();
            //Rigidbody.isKinematic = false;

            //Rigidbody.velocity = dir.normalized * StartingSpeed;
            //aimAssisted = true;
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

            //bool shouldAimAssist = !aimAssisted && aimAssist && AimingAtEnemy == null
            //    && !collided && ForkedFrom.Count == 0 && ChainedFrom.Count == 0
            //    && InAimAssistRange.Count > 0;

            if (false)
            {
                DoAimAssist();
            }
        }






        public virtual void OnTriggerEnter(Collider collider)
        {

            if (isDummy)
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

                collided = true;
                collidingWith = collidedEnemy;

                Rigidbody.DOKill();
                HandleEnemyCollision();
            }
        }



        public bool ShouldPierce()
        {
            bool always = CastBy.GetTotal("AlwaysPierce") > 0;

            if (always)
            {
                return true;
            }

            int count = (int)CastBy.GetTotal("PierceAdditionalTimes");

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
            if (ChainedFrom.Count >= CastBy.ChainAdditionalTimes)
            {
                return false;
            }

            return true;
        }

        public void DoChain()
        {
            var chainRangeSqr = GameManager.Instance.ChainAndForkRange * GameManager.Instance.ChainAndForkRange;

            var potentials = EnemyManager.Instance.aliveEnemies.Where(e => e != null
            && e.gameObject != collidingWith.gameObject
            && (e.transform.position - transform.position).sqrMagnitude < chainRangeSqr);
            
            Enemy e = potentials.Count() > 0 ?
                potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;

            if (e != null)
            {
                SetVelocityTowards(e.transform.position, StartingSpeed);
                ChainedFrom.Add(collidingWith);
                return;
            }

            print("enemyToChainTo == null");
        }

        public bool ShouldFork()
        {
            if (ForkedFrom.Count >= CastBy.ForkAdditionalTimes)
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

            var chainRangeSqr = GameManager.Instance.ChainAndForkRange * GameManager.Instance.ChainAndForkRange;

            var potentials = EnemyManager.Instance.aliveEnemies.Where(e => e != null
            && e.gameObject != collidingWith.gameObject
            && (e.transform.position - transform.position).sqrMagnitude < chainRangeSqr);

            Enemy e = potentials.Count() > 0 ?
                potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;


            if (e == null)
            {
                print("can't fork");
                return false;
            }

            ForkedFrom.Add(collidingWith);
            ForkingFrom = collidingWith;


            potentials = potentials.Where(p => p != e);

            Enemy e2 = potentials.Count() > 0 ?
                potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;

            if (e2 == null)
            {
                print("use same enemy for fork");
                e2 = e;
            }

            
            ProjectileBase clone = Instantiate(this, transform.position, Quaternion.identity);
            ProjectileBase clone2 = Instantiate(this, transform.position, Quaternion.identity);

            clone.CopyFrom(this);
            clone2.CopyFrom(this);

            clone.SetVelocityTowards(e.transform.position, StartingSpeed);
            clone2.SetVelocityTowards(e2.transform.position, StartingSpeed);

            return true;
        }

        private void SetVelocityTowards(Vector3 position, float speed)
        {
            Rigidbody.isKinematic = false;
            transform.LookAt(position);

            var dir = (position - transform.position).normalized;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.AddForce(dir * speed, ForceMode.VelocityChange);
        }

        private void CopyFrom(ProjectileBase p)
        {
            this.StartingSpeed = p.StartingSpeed;
            this.ForkingFrom = p.ForkingFrom;
            this.ForkedFrom = new List<Enemy>(p.ForkedFrom);
            this.ChainedFrom = new List<Enemy>(p.ChainedFrom);
            this.PiercedFrom = new List<Enemy>(p.PiercedFrom);
            this.CastBy = p.CastBy;
        }

        protected void DisableCollider()
        {
            mainProjCollider.gameObject.SetActive(false);
            //explTrigger.gameObject.SetActive(false);
        }

        protected void EnableCollider()
        {
            mainProjCollider.gameObject.SetActive(true);
            //explTrigger.gameObject.SetActive(true);
        }

        protected void HideBody()
        {
            print("hiding body");
            bodyGroup.SetActive(false);
        }

        protected void DisableRigidBody()
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
        }

        protected void DestroySelfSoft()
        {
            HideBody();
            DisableCollider();
            DisableRigidBody();

            Destroy(gameObject, maxTrailDur);
        }

        public void EnableSpinner()
        {
            spinner.enabled = true;
        }

        private void DisableTrails()
        {
            foreach (var tr in trailRenderers)
            {
                tr.enabled = false;
            }
        }

        private void EnableTrails()
        {
            foreach (var tr in trailRenderers)
            {
                tr.enabled = true;
            }
        }

        public void MakeDummy()
        {
            DisableCollider();
            DisableTrails();
            ScaleToDummyScale();



            Rigidbody.isKinematic = true;
            isDummy = true;
        }

        public void MakeNormal()
        {
            EnableCollider();
            EnableTrails();
            ResetScale();

            Rigidbody.isKinematic = false;
            isDummy = false;
            spinner.enabled = false;
        }
    }
}
