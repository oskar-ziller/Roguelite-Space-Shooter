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

        private Collider collider;

        //private SphereCollider collider;


        //protected HashSet<GameObject> InExplosionRange => inExplosionRange;
        //protected HashSet<GameObject> InChainRange => inChainRange;
        //protected HashSet<GameObject> InAimAssistRange => inAimAssistRange;

        //private List<TrailRenderer> trailRenderers;

        //private SpinAround spinner;


        //private HashSet<GameObject> inExplosionRange = new HashSet<GameObject>();
        //private HashSet<GameObject> inChainRange = new HashSet<GameObject>();
        //private HashSet<GameObject> inAimAssistRange = new HashSet<GameObject>();


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

            //var trails = GetComponentsInChildren<TrailRenderer>();

            //if (trails != null)
            //{
            //    trailRenderers = trails.ToList();

            //    if (trailRenderers.Count > 0)
            //    {
            //        maxTrailDur = trailRenderers.Max(t => t.time);
            //    }
            //}




            //spinner = GetComponent<SpinAround>();
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<SphereCollider>();


            //collider.radius = meshScaleMain / 2f;
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
            Die();
        }

        private bool ShouldExpire()
        {
            return Time.time > expireTime && isSetup;
        }


        protected void Die()
        {
            transform.DOKill();
            Destroy(gameObject);
        }



        private void DoAimAssist()
        {
            //var dir = InAimAssistRange.First().transform.position - transform.position;

            //Rigidbody.DOKill();
            //Rigidbody.isKinematic = false;

            //Rigidbody.velocity = dir.normalized * StartingSpeed;
            //aimAssisted = true;
        }


        public virtual void Update()
        {
            if (isDummy)
            {
                return;
            }

            // scales back to original size as it moves away from player
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
            if (isDummy || !collider.gameObject.activeInHierarchy)
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

                collided = true;
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

        public void DoChain()
        {
            //var chainRangeSqr = GameManager.Instance.ChainAndForkRange * GameManager.Instance.ChainAndForkRange;

            //var potentials = EnemyManager.Instance.AliveEnemies.Where(e => e != null
            //&& e.gameObject != collidingWith.gameObject
            //&& (e.transform.position - transform.position).sqrMagnitude < chainRangeSqr);
            
            //Enemy e = potentials.Count() > 0 ?
            //    potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;

            //if (e != null)
            //{
            //    SetVelocityTowards(e.transform.position, spawnInfo.CastBy.Modifiers.ProjectileSpeedCalcd);
            //    ChainedFrom.Add(collidingWith);
            //    return;
            //}

            //print("enemyToChainTo == null");
        }

        public bool ShouldFork()
        {
            if (ForkedFrom.Count >= spawnInfo.CastBy.GetTotal("ForkAdditionalTimes"))
            {
                return false;
            }

            return true;
        }

        public bool DoFork()
        {
            ///* When a projectile forks, it splits into two identical projectiles
            // * that continue travelling at 60 and -60 degree angles
            // * from the projectile's original trajectory. */

            //var chainRangeSqr = GameManager.Instance.ChainAndForkRange * GameManager.Instance.ChainAndForkRange;

            //var potentials = EnemyManager.Instance.AliveEnemies.Where(e => e != null
            //&& e.gameObject != collidingWith.gameObject
            //&& (e.transform.position - transform.position).sqrMagnitude < chainRangeSqr);

            //Enemy e = potentials.Count() > 0 ?
            //    potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;


            //if (e == null)
            //{
            //    print("can't fork");
            //    return false;
            //}

            //ForkedFrom.Add(collidingWith);
            //ForkingFrom = collidingWith;


            //potentials = potentials.Where(p => p != e);

            //Enemy e2 = potentials.Count() > 0 ?
            //    potentials.ElementAt(UnityEngine.Random.Range(0, potentials.Count())) : null;

            //if (e2 == null)
            //{
            //    print("use same enemy for fork");
            //    e2 = e;
            //}


            //ProjectileBase clone = Instantiate(this, transform.position, Quaternion.identity);
            //ProjectileBase clone2 = Instantiate(this, transform.position, Quaternion.identity);

            //clone.CopyFrom(this);
            //clone2.CopyFrom(this);

            //clone.SetVelocityTowards(e.transform.position, spawnInfo.CastBy.Modifiers.ProjectileSpeedCalcd);
            //clone2.SetVelocityTowards(e2.transform.position, spawnInfo.CastBy.Modifiers.ProjectileSpeedCalcd);

            //return true;

            return false;
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
        }



        protected void DisableRigidBody()
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
        }


        //public float scaleTime = 0.5f;

        private  void ScaleProjectileWhileMove()
        {
            bodyMesh.localScale = Vector3.zero;
            bodyMesh.DOScale(MeshScaleMain, ScaleDur);
            transform.DOScale(1f, ScaleDur);
        }

        //protected void DestroySelfSoft()
        //{
        //    mainMesh.gameObject.SetActive(false);
        //    DisableRigidBody();
        //    collider.enabled = false;
        //    transform.DOKill();

        //    Destroy(gameObject);
        //}

        //public void EnableSpinner()
        //{
        //    spinner.enabled = true;
        //}



        //private void EnableTrails()
        //{
        //    foreach (var tr in trailRenderers)
        //    {
        //        tr.enabled = true;
        //    }
        //}



        //public void MakeDummy(float dur)
        //{
        //    CachedDummyScale = (dummyScale / CastBy.ProjectileCount);
        //    DisableRigidBody();

        //    isDummy = true;
        //}

        //public void MakeNormal()
        //{
        //    Rigidbody.isKinematic = false;
        //    isDummy = false;
        //    //spinner.enabled = false;
        //}


    }
}
