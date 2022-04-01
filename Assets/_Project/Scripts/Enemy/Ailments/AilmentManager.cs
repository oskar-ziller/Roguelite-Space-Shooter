using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class AilmentManager : MonoBehaviour
    {
        private Enemy owner;

        private const float minChillEffect = 0.05f; // 5%
        private const float maxChillEffect = 0.30f; // 30%
        private const float baseChillDurationSeconds = 2;

        private const float baseFreezeDuration = 0.06f; // 0.06 seconds
        private const float minFreezeDur = 0.3f; // 0.3 seconds
        private const float maxFreezeDur = 3f; // 3 seconds

        private const float minWeakenEffect = 0.05f; // 5%
        private const float maxWeakenEffect = 0.50f; // 50%
        private const float baseWeakenDurationSeconds = 2;

        const float baseBurnDPSMultip = 0.5f; // 50% of the base damage of the hit 
        const float baseBurnDurationSeconds = 4;
        public const float BurnTickInterval = 0.5f;

        public const float ChillingAreaEffect = 0.1f;

        public bool InChillingArea { get; private set; } = false;

        public Ailment Chill { get; private set; } = null;
        public Ailment Weaken { get; private set; } = null;
        public Ailment Freeze { get; private set; } = null;

        public List<Ailment> BurnStacks { get; private set; } = new List<Ailment>();



        private void Awake()
        {
            owner = GetComponent<Enemy>();
        }

        private float Formula(float damage, float hp, float increasedBy)
        {
            return 0.5f * (float)Math.Pow(damage / hp, 0.4) * (increasedBy + 1);
        }



        internal void Reset()
        {
            Chill = null;
            Freeze = null;
            Weaken = null;
            BurnStacks.Clear();
            InChillingArea = false;
        }




        private IEnumerator RemoveAilmentWhenExpired(Ailment a)
        {
            yield return new WaitForSeconds(a.duration);
            RemoveAilment(a);
        }

        private void RemoveAilment(Ailment a)
        {
            //print("Removing ailment " + a.type);

            if (a.type == Ailments.Chill)
            {
                Chill = null;
            }

            if (a.type == Ailments.Freeze)
            {
                Freeze = null;
            }

            if (a.type == Ailments.Weaken)
            {
                Weaken = null;
            }

            if (a.type == Ailments.Burn)
            {
                BurnStacks.Remove(a);
            }
        }


        private void AddAilment(Ailment a)
        {

            if (a.type == Ailments.Burn)
            {
                BurnStacks.Add(a);
                StartCoroutine(RemoveAilmentWhenExpired(a));
                return;
            }

            if (a.type == Ailments.Chill)
            {
                if (Chill == null || a.magnitude > Chill.magnitude)
                {
                    Chill = a;
                    StartCoroutine(RemoveAilmentWhenExpired(a));
                    return;
                }
            }

            if (a.type == Ailments.Freeze)
            {
                if (Freeze == null || a.duration > Freeze.duration)
                {
                    Freeze = a;
                    StartCoroutine(RemoveAilmentWhenExpired(a));
                    return;
                }
            }

            if (a.type == Ailments.Weaken)
            {
                if (Weaken == null || a.magnitude > Weaken.magnitude)
                {
                    //print($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                    Weaken = a;
                    StartCoroutine(RemoveAilmentWhenExpired(a));
                    return;
                }
            }
        }


        internal void CheckIfDamageAppliesAilment(SpellSlot from, int fireFinal, int coldFinal, int lightFinal)
        {
            CheckForBurn(from, fireFinal);
            CheckForFeezeAndChill(from, coldFinal);
            CheckForWeaken(from, lightFinal);
        }

        private void CheckForWeaken(SpellSlot from, float damageAmount)
        {
            // chance based

            if (damageAmount <= 0)
            {
                return;
            }

            var weakenChance = from.GetTotal("ChanceToWeaken")/100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToWeaken");

            var totalChance = weakenChance * chanceIncreasedBy;

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }

            float effectIncreasedBy = from.GetTotal("IncreasedEffectOfWeaken");
            var effect = Formula(damageAmount, owner.TotalHealth, effectIncreasedBy);

            if (effect < minWeakenEffect)
            {
                // do nothing
                return;
            }

            if (effect > maxWeakenEffect)
            {
                effect = maxWeakenEffect;
            }

            var duration = baseWeakenDurationSeconds;
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfWeaken");

            duration *= durationIncreasedBy;

            Ailment a = new();
            a.type = Ailments.Weaken;
            a.duration = duration;
            a.magnitude = effect;

            AddAilment(a);
        }



        private void CheckForFeezeAndChill(SpellSlot from, int damageAmount)
        {
            CheckForFreeze(from, damageAmount);

            if (Freeze != null) // if frozen no need to chill
            {
                return;
            }

            CheckForChill(from, damageAmount);
        }


        private void CheckForFreeze(SpellSlot from, int damageAmount)
        {
            // When a target is frozen, it is also considered chilled.
            // As the target becomes unfrozen, chill will remain on the target for
            // an additional duration of 0.3 seconds

            // chance based

            if (damageAmount <= 0)
            {
                return;
            }

            var freezeChance = from.GetTotal("ChanceToFreeze")/100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToFreeze");

            var totalChance = freezeChance * chanceIncreasedBy;

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }


            // Freeze duration is 60 milliseconds (0.06 seconds) for every 1% of the target's maximum life

            float damagePercentage = damageAmount / owner.TotalHealth;
            float dur = damagePercentage * 0.06f;


            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfFreeze");
            float increasedDurationOfColdAilments = from.GetTotal("IncreasedDurationOfColdAilments");
            float durationReducedBy = from.GetTotal("ReducedDurationOfFreeze");

            dur = dur * durationIncreasedBy * increasedDurationOfColdAilments * durationReducedBy;

            if (dur < minFreezeDur)
            {
                // do nothing
                return;
            }

            if (dur > maxFreezeDur)
            {
                dur = maxFreezeDur;
            }

            Ailment a = new();
            a.type = Ailments.Freeze;
            a.duration = dur;
            a.magnitude = dur;

            AddAilment(a);
        }

        private void CheckForChill(SpellSlot from, float damageAmount)
        {
            if (damageAmount == 0)
            {
                return;
            }

            float increasedBy = from.GetTotal("IncreasedEffectOfChill");
            var increasedEffectOfColdAilments = from.GetTotal("IncreasedEffectOfColdAilments");

            var effect = Formula(damageAmount, owner.TotalHealth, increasedBy);

            if (effect < minChillEffect)
            {
                // do nothing
                return;
            }

            if (effect > maxChillEffect)
            {
                effect = maxChillEffect;
            }

            var duration = baseChillDurationSeconds;
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfColdAilments");

            duration *= durationIncreasedBy;

            Ailment a = new();
            a.type = Ailments.Chill;
            a.duration = duration;
            a.magnitude = effect;

            AddAilment(a);
        }

        internal void AddChillingAreaAilment()
        {
            if (InChillingArea == false)
            {
                InChillingArea = true;
                return;
            }
        }

        internal void RemoveChillingAreaAilment()
        {
            InChillingArea = false;
        }


        private void CheckForBurn(SpellSlot from, float damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            var burnChance = from.GetTotal("ChanceToBurn")/100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToBurn");

            var totalChance = burnChance * chanceIncreasedBy;

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }

            var duration = baseBurnDurationSeconds;
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfBurn");

            duration *= durationIncreasedBy;


            var increasedDamage = from.GetTotal("IncreasedDamageWithBurn");

            // The burning damage over time is 50% of the base damage of the hit
            int totalDamage = (int)(damageAmount * increasedDamage * baseBurnDPSMultip);


            Ailment a = new();
            a.type = Ailments.Burn;
            a.duration = duration;
            a.magnitude = totalDamage;

            AddAilment(a);
        }


    }
}
