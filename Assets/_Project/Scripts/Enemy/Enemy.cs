using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public Action<Enemy> DamageTaken;

        private int id;

        private float currentSpeed;



        private List<ChillingArea> collidingChillingAreas = new List<ChillingArea>(); // keep track of which chilling areas we are colliding

        private void OnChillingAreaExpired(ChillingArea a)
        {
            if (collidingChillingAreas.Contains(a))
            {
                collidingChillingAreas.Remove(a);

                if (collidingChillingAreas.Count == 0)
                {
                    ailmentManager.RemoveChillingAreaAilment();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //var chillingArea = other.GetComponent<ChillingArea>();

            //if (chillingArea != null)
            //{
            //    collidingChillingAreas.Add(chillingArea);
            //    chillingArea.AreaExpired += OnChillingAreaExpired;
            //}
        }

        private void OnTriggerExit(Collider other)
        {
            var chillingArea = other.GetComponent<ChillingArea>();

            if (chillingArea != null)
            {
                collidingChillingAreas.Remove(chillingArea);

                if (collidingChillingAreas.Count == 0)
                {
                    ailmentManager.RemoveChillingAreaAilment();
                }

            }
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




        private float startingSpeed;
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



        [ContextMenu("Debug enemy")]
        void DebugEnemy()
        {
            print("Debug enemy");


            Init(transform.position, UnityEngine.Random.Range(-99999, 999999), 99);

            EnemyManager.Instance.AddEnemy(this);
        }



        public void Init(Vector3 pos, int id, int level = 0)
        {
            this.id = id;

            baseLifeList = baseLifeArr.ToList();
            baseLifeList.Reverse();

            spawnPos = pos;

            RepositionToSpawn();

            var levelToSet = level == 0 ? Mathf.RoundToInt(GameManager.Instance.GameLevel) : level;
            SetLevel(levelToSet);
            //SetRandomRotation();


            int index = levelToSet < baseLifeList.Count ?  levelToSet : baseLifeList.Count - 1;

            baseLife = baseLifeList[index];

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

            StartCoroutine(CheckFrozenOrChilledCoroutine());
            StartCoroutine(CheckIgniteTickCoroutine());
            StartCoroutine(CheckChillingAreaCoroutine());
        }

        private IEnumerator CheckChillingAreaCoroutine()
        {
            while (true)
            {
                if (collidingChillingAreas.Count > 0)
                {
                    foreach (ChillingArea chillingArea in collidingChillingAreas)
                    {
                        TakeDoT(chillingArea.CastBy, 0.25f, applyAilment: false);
                    }

                    if (!ailmentManager.InChillingArea)
                    {
                        ailmentManager.AddChillingAreaAilment();
                    }

                    yield return new WaitForSeconds(0.25f);
                }

                yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.3f));
            }
        }



        public void SetLevel(int level)
        {
            this.level = level;
        }

        private void Awake()
        {
            ailmentManager = GetComponent<AilmentManager>();
        }
        /*
        int[] baseLifeArr = { 44831, 42093, 39519, 37098, 34823,
                              32684, 30673, 28784, 27007, 25338,
                              23770, 22296, 20911, 19610, 18388,
                              17240, 16161, 15149, 14198, 13304,
                              12466, 11679, 10940, 10246, 9595,
                              8984, 8410, 7872, 7367, 6894,
                              6449, 6033, 5642, 5276, 4932,
                              4610, 4308, 4025, 3760, 3512,
                              3279, 3061, 2857, 2665, 2486,
                              2319, 2162, 2015, 1878, 1749,
                              1629, 1516, 1411, 1313, 1221,
                              1135, 1055, 980, 910, 844,
                              783, 726, 673, 624, 577,
                              534, 494, 456, 422, 389,
                              359, 331, 304, 280, 257,
                              236, 217, 199, 182, 166,
                              152, 138, 126, 114, 104,
                              94, 85, 76, 68, 61,
                              55, 49, 43, 38, 33,
                              29, 25, 21, 18, 15 };

        */

        int[] baseLifeArr = { 44831, 42093, 39519, 37098, 34823,
                              32684, 30673, 28784, 27007, 25338,
                              23770, 22296, 20911, 19610, 18388,
                              17240, 16161, 15149, 14198, 13304,
                              12466, 11679, 10940, 10246, 9595,
                              8984, 8410, 7872, 7367, 6894,
                              6449, 6033, 5642, 5276, 4932,
                              4610, 4308, 4025, 3760, 3512,
                              3279, 3061, 2857, 2665, 2486,
                              2319, 2162, 2015, 1878, 1749,
                              1629, 1516, 1411, 1313, 1221,
                              1135, 1055, 980, 910, 844,
                              783, 726, 673, 624, 577,
                              534, 494, 456, 422, 389,
                              359, 331, 304, 280, 257,
                              236, 217, 199, 182, 166,
                              152, 138, 126, 114, 104};


        List<int> baseLifeList;

        internal void SetRarity(EnemyRarity rarity)
        {
            this.rarity = rarity;
        }


        internal void ChangeSize(float radius)
        {
            transform.localScale = Vector3.one * radius;
        }



        private void SlowDownIfChilled()
        {
            if (ailmentManager.Chill != null)
            {
                var shouldBe = startingSpeed * (1 - ailmentManager.Chill.magnitude);

                if (currentSpeed > shouldBe)
                {
                    //print($"Ailment chill detected. - Currentvel: {currentVel} - {currentVel.magnitude} - {currentVel.sqrMagnitude} " +
                    //    $"Shouldbe: {shouldBe} - {shouldBe.magnitude} - {shouldBe.sqrMagnitude}");

                    var newVel = startingVel * (1 - ailmentManager.Chill.magnitude);
                    rigidBody.velocity = newVel;
                    currentSpeed = newVel.magnitude;
                }
            }

            if (ailmentManager.InChillingArea)
            {
                var shouldBe = startingSpeed * (1f - AilmentManager.ChillingAreaEffect);

                if (currentSpeed > shouldBe)
                {
                    var newVel = startingVel * (1f - AilmentManager.ChillingAreaEffect);
                    rigidBody.velocity = newVel;
                    currentSpeed = newVel.magnitude;
                }
            }
        }

        private void StopIfFrozen()
        {
            if (ailmentManager.Freeze != null)
            {
                if (currentSpeed != 0)
                {
                    rigidBody.velocity = Vector3.zero;
                    currentSpeed = 0;
                }
            }
        }

        private void RecoverVelocityIfNoAilment()
        {
            bool shouldBeSlower = ailmentManager.Freeze != null 
                || ailmentManager.Chill != null 
                || ailmentManager.InChillingArea;

            if (!shouldBeSlower)
            {
                if (currentSpeed != startingSpeed)
                {
                    rigidBody.velocity = startingVel;
                    currentSpeed = startingSpeed;
                }
            }
        }

        private IEnumerator CheckFrozenOrChilledCoroutine()
        {
            while (true)
            {
                StopIfFrozen();
                SlowDownIfChilled();
                RecoverVelocityIfNoAilment();
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.3f));
            }
        }

        private IEnumerator CheckIgniteTickCoroutine()
        {
            while (true)
            {
                if (ailmentManager.IgniteStacks.Count > 0)
                {
                    var stacks = ailmentManager.IgniteStacks;
                    //Debug.Log($"Taking ailment damage from {stacks.Count} ignite stacks");

                    foreach (var i in stacks) 
                    {
                        IgniteTick((int)i.magnitude);
                    }

                    yield return new WaitForSeconds(AilmentManager.IgniteTickInterval);
                }
                else
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.25f));
                }
            }
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        public void IgniteTick(int amount)
        {
            //print("Ailment - IgniteTick " + amount);
            TakeDamage(amount);
        }

        private void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var shock = ailmentManager.Shock;

            if (shock != null)
            {
                //print("taking damage with shock. before: " + amount);
                amount = (int)(amount * (1 + shock.magnitude));
                //print("taking damage with shock. after: " + amount);
            }

            currentHealth -= amount;


            //print($"-ENEMY{id}- Took {amount} damage." +
            //    $" currentHealth: {currentHealth} -" +
            //    $" totalHealth: {totalHealth} -" +
            //    $" baseLife: {baseLife}");


            if (currentHealth <= 0)
            {
                Die();
            }

            DamageTaken?.Invoke(this);
        }


        public void TakeDoT(SpellSlot from, float scale, bool applyAilment = true)
        {
            //print($"Taking {from.DamageOverTime} damage per second." +
            //    $" Current hit: {(int)(from.DamageOverTime * scale)}");

            TakeDamage((int)(from.DamageOverTime * scale));

            if (applyAilment)
            {
                ailmentManager.CheckIfDamageAppliesAilment(from, from.FireDoT, from.ColdDoT, from.LightDoT);
            }
        }





        public void TakeHit(SpellSlot from, bool applyAilment = true)
        {
            var inc = 0f;
            var red = 0f;

            if (ailmentManager.IgniteStacks.Count > 0)
            {
                inc += from.GetTotal("IncreasedDamageAgainstIgnited") / 100f;
            }

            if (ailmentManager.Chill != null || ailmentManager.Freeze != null)
            {
                inc += from.GetTotal("IncreasedDamageAgainstChilledOrFrozen") / 100f;
            }

            if (ailmentManager.Shock != null)
            {
                inc += from.GetTotal("IncreasedDamageAgainstShocked") / 100f;
            }

            float fireFinal = from.FireEffectiveDamage * (1 + inc) * (1 - red);
            float coldFinal = from.ColdEffectiveDamage * (1 + inc) * (1 - red);
            float lightFinal = from.LightningEffectiveDamage * (1 + inc) * (1 - red);

            int final = (int)(fireFinal + coldFinal + lightFinal);

            //print("Taking damage of: " + final);

            TakeDamage(final);

            if (applyAilment)
            {
                ailmentManager.CheckIfDamageAppliesAilment(from, (int)fireFinal, (int)coldFinal, (int)lightFinal);
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

        public void StartMoving(Vector3 dir)
        {
            startingSpeed = EnemyManager.Instance.EnemySpeed;


            if (rarity == EnemyRarity.Magic)
            {
                startingSpeed *= 0.9f;
            }

            if (rarity == EnemyRarity.Rare)
            {
                startingSpeed *= 0.8f;
            }

            if (rarity == EnemyRarity.Unique)
            {
                startingSpeed *= 0.7f;
            }

            //Vector3 movingTowards = Vector3.zero; 
            //Vector3 dir = (movingTowards - transform.position).normalized;
            startingVel = dir * startingSpeed;

            rigidBody.isKinematic = false;
            rigidBody.velocity = startingVel;
            currentSpeed = startingVel.magnitude;
        }

   

        private void RepositionToSpawn()
        {
            transform.position = spawnPos;
        }

    }

}





