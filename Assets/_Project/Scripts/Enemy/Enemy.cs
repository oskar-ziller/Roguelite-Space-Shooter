using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{

    public enum EnemyRarity
    {
        Normal,
        Magic,
        Rare,
        Unique
    }

    public class Enemy : MonoBehaviour
    {
        //[Tooltip("Starting speed"), Range(0f, 5f)]
        //[SerializeField] private float startingSpeed = 1f;

        [Tooltip("Rigidbody of whole enemy")]
        [SerializeField] private Rigidbody rigidBody;

        [Tooltip("After this many seconds of no damage, hide healthbar on enemies. 0 = always on")]
        public float hideLifebarAfterSeconds = 0f;


        [Tooltip("Enemy level")]
        [SerializeField] public int level;

        [Tooltip("Enemy max level")]
        [SerializeField] private int maxLevel;

        [Tooltip("Rarity of this enemy")]
        public EnemyRarity rarity;



        [Tooltip("Audio to play on spawn")]
        [SerializeField] private AudioClip spawnSound;


        [Tooltip("Looping hover/engine sound")]
        [SerializeField] private AudioClip engineLoop;

        [Tooltip("Audio source that plays hover sounds")]
        [SerializeField] private AudioSource hoverAudioSource;


        public Action<Enemy> DamageTaken;

        private AudioSource spawnAudioSource;


        public AnimationCurve spawnAudioCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });



        internal void ApplyChillingGround()
        {
            ailmentManager.ApplyChillingGround();
        }


        // totalMonsterHealth = baseLife * monsterTypeHealthModifier
        //                                  * monsterRarityModifier
        //                                  * mapMonsterHealthModifier
        //                                  * monsterAffixHealthModifier



        // monsterTypeHealthModifier %50-%200 arasi


        /*
         * 
         
         Dominus level 33 -> 7043 hp

         
         
         */


        /* Normal 'monsterRarityModifier' = 100%.
         * magic monsters = 187% more
         * rares = 463% more
         * uniques = 625% more. 

         'mapMonsterHealthModifier' is just the "More Monster Life" affix on maps


        monsterAffixHealthModifier --> ?? unknown


         */





        private Vector3 startingVel;
        private Vector3 spawnPos;


        private float rotationDir;
        private Vector3 randomRotationVec;



        public event Action<Enemy> OnDeath;

        private AilmentManager ailmentManager;


        public Dictionary<int, float> zapDict = new Dictionary<int, float>();

        private int baseLife;
        public int totalHealth;
        public int currentHealth;




        public void Init(Vector3 pos)
        {
            spawnPos = pos;

            RepositionToSpawn();
            StartMoving();
            SetLevel(Mathf.RoundToInt(GameManager.Instance.GameLevel));
            //SetRandomRotation();

            baseLife = (int)(baseLifeArr[maxLevel - level - 1] * 1.2f);

            var monsterTypeHealthModifier = 1f; // %50-%200 arasi

            var monsterRarityModifier = 1.3f;

            if (rarity == EnemyRarity.Magic)
            {
                monsterRarityModifier = 2.87f;
                //monsterRarityModifier = 1.87f;
            }

            if (rarity == EnemyRarity.Rare)
            {
                monsterRarityModifier = 12.63f;
                //monsterRarityModifier = 6.63f;
            }

            if (rarity == EnemyRarity.Unique)
            {
                monsterRarityModifier = 18.25f;
                //monsterRarityModifier = 8.25f;
            }

            // totalMonsterHealth = baseLife * monsterTypeHealthModifier
            //                                  * monsterRarityModifier
            //                                  * mapMonsterHealthModifier
            //                                  * monsterAffixHealthModifier

            /* Normal 'monsterRarityModifier' = 100%.
            * magic monsters = 187% more
            * rares = 463% more
            * uniques = 625% more. */


            totalHealth = (int)(baseLife * monsterTypeHealthModifier * monsterRarityModifier);
            currentHealth = totalHealth;

            //print($"{level} level {rarity} enemy with {totalHealth} health - pos: {pos}");

            PlaySpawnSound();
            PlayHoverSound();
        }


        public bool IsFrozenOrChilled()
        {
            return ailmentManager.strongestChill != null || ailmentManager.strongestFreeze != null;
        }

        public bool IsIgnited()
        {
            return ailmentManager.strongestIgnite != null;
        }

        public bool IsShocked()
        {
            return ailmentManager.strongestShock != null;
        }

        private AudioSource EditSourceSettings(AudioSource s)
        {
            s.playOnAwake = false;
            s.spatialBlend = 1;
            s.volume = 0.5f;
            s.rolloffMode = AudioRolloffMode.Custom;

            s.minDistance = 0f;
            s.maxDistance = 90f;

            s.SetCustomCurve(AudioSourceCurveType.CustomRolloff, spawnAudioCurve);

            return s;
        }


        private void PlaySpawnSound()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s = EditSourceSettings(s);
            s.clip = spawnSound;
            s.Play();
        }

        private void PlayHoverSound()
        {
            hoverAudioSource.Play();
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        private void Awake()
        {
            ailmentManager = new AilmentManager(this);
            spawnAudioSource = gameObject.GetComponent<AudioSource>();
        }


        int[] baseLifeArr = { 44831, 42093, 39519, 37098, 34823, 32684, 30673, 28784, 27007, 25338, 23770, 22296, 20911, 19610, 18388, 17240, 16161, 15149, 14198, 13304, 12466, 11679, 10940, 10246, 9595, 8984, 8410, 7872, 7367, 6894, 6449, 6033, 5642, 5276, 4932, 4610, 4308, 4025, 3760, 3512, 3279, 3061, 2857, 2665, 2486, 2319, 2162, 2015, 1878, 1749, 1629, 1516, 1411, 1313, 1221, 1135, 1055, 980, 910, 844, 783, 726, 673, 624, 577, 534, 494, 456, 422, 389, 359, 331, 304, 280, 257, 236, 217, 199, 182, 166, 152, 138, 126, 114, 104, 94, 85, 76, 68, 61, 55, 49, 43, 38, 33, 29, 25, 21, 18, 15 };


        void Start()
        {

        }


        internal void SetRarity(EnemyRarity rarity)
        {
            this.rarity = rarity;
        }


        internal void ChangeSize(float radius)
        {
            transform.localScale = Vector3.one * radius;
        }

        private void StopIfFrozen()
        {
            if (ailmentManager.strongestFreeze != null)
            {
                rigidBody.velocity = Vector3.zero;
            }
            else
            {
                rigidBody.velocity = startingVel;
            }
        }


        private void SlowDownIfChilled()
        {
            var currentVel = rigidBody.velocity;

            if (ailmentManager.strongestChill != null)
            {
                var shouldBe = startingVel * ailmentManager.strongestChill.magnitude;

                if (currentVel.sqrMagnitude > shouldBe.sqrMagnitude)
                {
                    rigidBody.velocity = shouldBe;
                }
            }
            else
            {
                if (currentVel.sqrMagnitude < startingVel.sqrMagnitude)
                {
                    rigidBody.velocity = startingVel;
                }
            }
        }


        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    PlaySpawnSound();
            //}
        }

        private void FixedUpdate()
        {
            ailmentManager.UpdateAilments();
            StopIfFrozen();
            SlowDownIfChilled();
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        public void IgniteTick(int amount)
        {
            TakeDamage(amount);
        }

        private void TakeDamage(int amount)
        {
            var shock = ailmentManager.strongestShock;

            if (shock != null)
            {
                //print("taking damage with shock. before: " + amount);
                amount = (int)(amount * (1 + shock.magnitude));
                //print("taking damage with shock. after: " + amount);
            }

            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                Die();
            }

            DamageTaken?.Invoke(this);
        }


        public void TakeDoT(SpellSlot from, float scale, bool applyAilment = true)
        {
            float more = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("IncreasedProjectileDamage"), from) / 100f;
            float less = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("ReducedProjectileDamage"), from) / 100f;

            int fire = (int)(ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("FireDamagePerSecond"), from) * (1 + more) * (1 - less));
            int lightning = (int)(ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("LightningDamagePerSecond"), from) * (1 + more) * (1 - less));
            int cold = (int)(ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("ColdDamagePerSecond"), from) * (1 + more) * (1 - less));

            int combined = fire + lightning + cold;

            TakeDamage((int)(combined * scale));

            if (applyAilment)
            {
                ailmentManager.CheckIfDamageAppliesAilment(from, fire, cold, lightning);
            }
        }



        public void TakeHit(SpellSlot from, bool applyAilment = true)
        {
            float more = ModifierHelper.GetTotal("IncreasedProjectileDamage", from) / 100f;
            float less = ModifierHelper.GetTotal("ReducedProjectileDamage", from) / 100f;
            float final = (1 + more) * (1 - less);

            int fire = (int)(ModifierHelper.GetTotal("DealFireDamage", from) * final);
            int lightning = (int)(ModifierHelper.GetTotal("DealLightningDamage", from) * final);
            int cold = (int)(ModifierHelper.GetTotal("DealColdDamage", from) * final);

            int combined = fire + lightning + cold;

            TakeDamage(combined);

            if (applyAilment)
            {
                ailmentManager.CheckIfDamageAppliesAilment(from, fire, cold, lightning);
            }
        }





        public void ForceDie()
        {
            Die();
        }

        private void Die()
        {
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }

        private void StartMoving()
        {
            var startingSpeed = EnemyManager.Instance.enemySpeed;


            if (rarity == EnemyRarity.Magic)
            {
                startingSpeed *= 0.8f;
            }

            if (rarity == EnemyRarity.Rare)
            {
                startingSpeed *= 0.4f;
            }

            if (rarity == EnemyRarity.Unique)
            {
                startingSpeed *= 0.3f;
            }



            Vector3 movingTowards = new Vector3(transform.position.x, 0, transform.position.z); // directly below
            Vector3 dir = (movingTowards - transform.position).normalized;
            startingVel = dir * startingSpeed;

            rigidBody.isKinematic = false;
            rigidBody.velocity = startingVel;
        }

        public void SetTarget(Transform target)
        {

        }


        private void RepositionToSpawn()
        {
            transform.position = spawnPos;
        }

    }

}





