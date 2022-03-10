using DG.Tweening;
using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace MeteorGame.Enemies
{
    public class Enemy : MonoBehaviour
    {
        [Tooltip("Rigidbody of whole enemy")]
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private int level;

        public event Action<Enemy> DamageTaken;
        public event Action<Enemy> HealthChanged;
        public event Action<Enemy, bool> Died;

        public int TotalHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public EnemyPack BelongsToPack { get; private set; }
        public bool IsDying { get; private set; }
        public Mesh Mesh => spawnInfo.SO.BodyMesh;
        public float Radi => spawnInfo.SO.ShapeRadi;
        public bool IsActive => isActive;
        public EnemySO SO => spawnInfo.SO;

        private float currentSpeed;
        private Vector3 startingVel;
        private AilmentManager ailmentManager;
        private MeshRenderer renderer;
        private MeshFilter meshFilter;
        private float normalSpeed;
        private ExplosionHandler explosionHandler;
        private EnemySpawnInfo spawnInfo;
        //private Tween alphaTween;
        //private float alphaTweening;
        private bool isActive = false;

        private int enemiesLayer = int.MinValue;
        private int stencilLayer = int.MinValue;

        private void Die(bool forced = false)
        {
            SetValuesToDefaults();
            spawnInfo.pack.OnPackEnemyDeath(this);
            Died?.Invoke(this, forced);
        }

        public void ForceDie()
        {
            Die(true);
        }

        private void Awake()
        {
            if (ailmentManager == null)
            {
                ailmentManager = GetComponent<AilmentManager>();
            }

            if (renderer == null)
            {
                renderer = GetComponent<MeshRenderer>();
            }

            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            enemiesLayer = LayerMask.NameToLayer("Enemies");
            stencilLayer = LayerMask.NameToLayer("Stencil1");
        }


        /// <summary>
        /// Activates and starts moving
        /// </summary>
        internal void Activate()
        {
            isActive = true;
            gameObject.layer = enemiesLayer;

            MoveToWorldOrigin(from: spawnInfo.pack.Position);
        }

        private void SetValuesToDefaults()
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.isKinematic = true;
            IsDying = false;
            isActive = false;
            ailmentManager.Reset();
        }

        /// <summary>
        /// Calculates HP depending on level, curve and kind
        /// </summary>
        private int CalculateHP(float hpMultip)
        {
            var startingHP = EnemyManager.Instance.BaseEnemyHP * hpMultip;
            var hpMultipForLevel = EnemyManager.Instance.EnemySpawner.CalculateEnemyHPMultip();
            return Mathf.CeilToInt(startingHP * hpMultipForLevel);
        }

        /// <summary>
        /// Sets transform pos and scale from EnemySpawnInfo
        /// </summary>
        private void SetPosAndScale(EnemySpawnInfo spawninfo)
        {
            //spawnPos = spawninfo.spawnPos;
            transform.position = spawnInfo.pack.Position + spawninfo.spawnPos;
            transform.localScale = spawninfo.SO.ShapeRadi * Vector3.one * 2f;
        }



        internal void Create(EnemySpawnInfo info)
        {
            level = GameManager.Instance.GameLevel;
            spawnInfo = info;
            explosionHandler = info.SO.ExplosionHandler;
            BelongsToPack = info.pack;
            meshFilter.mesh = info.SO.BodyMesh;
            renderer.material = info.SO.BodyMat;

            SetPosAndScale(info);
            gameObject.layer = stencilLayer;
            //SetAlpha(0f);

            //Spawned?.Invoke(this);

            //StartFade();


            TotalHealth = CalculateHP(info.SO.HealthMultiplier);
            SetCurrnetHealth(TotalHealth);
        }

        //internal void FadeIn(float dur)
        //{
        //    if (alphaTween != null && alphaTween.active)
        //    {
        //        alphaTween.Complete();
        //        alphaTween.Kill();
        //        alphaTweening = 0f;
        //        renderer.material.SetFloat("_alpha", alphaTweening);
        //    }

        //    alphaTween = DOTween.To(() => alphaTweening, x => alphaTweening = x, 1, dur);
        //    alphaTween.onUpdate += StepComplete;
        //}

        //private void SetAlpha(float newAlpha)
        //{
        //    renderer.material.SetFloat("_alpha", newAlpha);
        //}

        //private void StepComplete()
        //{
        //    SetAlpha(alphaTweening);
        //}

        public bool IsVisible()
        {
            return renderer.isVisible;
        }


        private void SpawnExplosion()
        {
            if (explosionHandler == null)
            {
                return;
            }

            ExplosionHandler explosion = Instantiate(explosionHandler);

            explosion.transform.position = transform.TransformPoint(Vector3.zero);
            explosion.transform.localScale = transform.localScale;
            explosion.transform.transform.SetParent(EnemyManager.Instance.EnemyExplosionHolder, true);
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

        //int[] baseLifeArr = { 44831, 42093, 39519, 37098, 34823,
        //                      32684, 30673, 28784, 27007, 25338,
        //                      23770, 22296, 20911, 19610, 18388,
        //                      17240, 16161, 15149, 14198, 13304,
        //                      12466, 11679, 10940, 10246, 9595,
        //                      8984, 8410, 7872, 7367, 6894,
        //                      6449, 6033, 5642, 5276, 4932,
        //                      4610, 4308, 4025, 3760, 3512,
        //                      3279, 3061, 2857, 2665, 2486,
        //                      2319, 2162, 2015, 1878, 1749,
        //                      1629, 1516, 1411, 1313, 1221,
        //                      1135, 1055, 980, 910, 844,
        //                      783, 726, 673, 624, 577,
        //                      534, 494};


        //List<int> baseLifeList;

        private void SlowDownIfChilled()
        {
            if (ailmentManager.Chill != null)
            {
                var shouldBe = normalSpeed * (1 - ailmentManager.Chill.magnitude);

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
                var shouldBe = normalSpeed * (1f - AilmentManager.ChillingAreaEffect);

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
                if (currentSpeed != normalSpeed)
                {
                    rigidBody.velocity = startingVel;
                    currentSpeed = normalSpeed;
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

        private IEnumerator CheckBurningTickCoroutine()
        {
            while (true)
            {
                if (ailmentManager.BurnStacks.Count > 0)
                {
                    var stacks = ailmentManager.BurnStacks;
                    //Debug.Log($"Taking ailment damage from {stacks.Count} burn stacks");

                    foreach (var i in stacks) 
                    {
                        BurningTick((int)i.magnitude);
                    }

                    yield return new WaitForSeconds(AilmentManager.BurnTickInterval);
                }
                else
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.25f));
                }
            }
        }

        public bool IsAlive()
        {
            return CurrentHealth > 0;
        }

        public void BurningTick(int amount)
        {
            //print("Ailment - BurningTick " + amount);
            TakeDamage(amount);
        }

        private void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var weaken = ailmentManager.Weaken;

            if (weaken != null)
            {
                //print("taking damage with weaken. before: " + amount);
                amount = (int)(amount * (1 + weaken.magnitude));
                //print("taking damage with weaken. after: " + amount);
            }


            SetCurrnetHealth(CurrentHealth - amount);

            //print($"-ENEMY{id}- Took {amount} damage." +
            //    $" currentHealth: {currentHealth} -" +
            //    $" totalHealth: {totalHealth} -" +
            //    $" baseLife: {baseLife}");


            if (CurrentHealth > 0)
            {
                DamageTaken?.Invoke(this);
            }
            else
            {
                DieWithEffect();
            }
        }

        private void DieWithEffect()
        {
            //if (!IsDying)
            //{
            //    IsDying = true;
            //    transform.DOScale(0, 0.5f).onComplete = Die;
            //}

            SpawnExplosion();
            Die();
        }

        private void SetCurrnetHealth(int newHealth)
        {
            CurrentHealth = newHealth;
            HealthChanged?.Invoke(this);
        }


        public void TakeDoT(SpellSlot from, float scale, bool applyAilment = true)
        {
            //print($"Taking {from.DamageOverTime} damage per second." +
            //    $" Current hit: {(int)(from.DamageOverTime * scale)}");

            TakeDamage((int)(from.Modifiers.CombinedDoT * scale));

            if (applyAilment)
            {
                ailmentManager.CheckIfDamageAppliesAilment(from, from.Modifiers.FireDoT, from.Modifiers.ColdDoT, from.Modifiers.LightDoT);
            }
        }



        public void TakeHit(SpellSlot from, bool applyAilment = true)
        {
            float increasedDamageAgainstBurning = 1f;
            float increasedDamageAgainstChilledOrFrozen = 1f;
            float increasedDamageAgainstWeakened = 1f;

            if (ailmentManager.BurnStacks.Count > 0)
            {
                increasedDamageAgainstBurning = from.GetTotal("IncreasedDamageAgainstBurning");
            }

            if (ailmentManager.Chill != null || ailmentManager.Freeze != null)
            {
                increasedDamageAgainstChilledOrFrozen  = from.GetTotal("IncreasedDamageAgainstChilledOrFrozen");
            }

            if (ailmentManager.Weaken != null)
            {
                increasedDamageAgainstWeakened = from.GetTotal("IncreasedDamageAgainstWeakened");
            }

            float fireFinal = from.Modifiers.FireEffectiveDamage * increasedDamageAgainstBurning;
            float coldFinal = from.Modifiers.ColdEffectiveDamage * increasedDamageAgainstChilledOrFrozen;
            float radiationFinal = from.Modifiers.RadiationEffectiveDamage * increasedDamageAgainstWeakened;

            int final = (int)(fireFinal + coldFinal + radiationFinal);

            TakeDamage(final);


            if (applyAilment && CurrentHealth > 0)
            {
                ailmentManager.CheckIfDamageAppliesAilment(from, (int)fireFinal, (int)coldFinal, (int)radiationFinal);
            }
        }





        public void MoveToWorldOrigin(Vector3 from)
        {
            MoveTo(from, Vector3.zero);
        }

        private void MoveTo(Vector3 from, Vector3 to)
        {
            var dir = (to - from).normalized;
            //transform.LookAt(to);
            normalSpeed = spawnInfo.pack.Speed * EnemyManager.Instance.BaseEnemySpeed;
            startingVel = dir * normalSpeed;

            rigidBody.isKinematic = false;
            rigidBody.velocity = startingVel;
            currentSpeed = startingVel.magnitude;
        }

    }

}





