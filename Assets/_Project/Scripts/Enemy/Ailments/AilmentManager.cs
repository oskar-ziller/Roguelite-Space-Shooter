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

        private const float minShockEffect = 0.05f; // 5%
        private const float maxShockEffect = 0.50f; // 50%
        private const float baseShockDurationSeconds = 2;

        const float baseIgniteDPSMultip = 0.5f; // 50% of the base damage of the hit 
        const float baseIgniteDurationSeconds = 4;
        public const float IgniteTickInterval = 0.5f;
        public bool InChillingArea { get; private set; } = false;

        public Ailment Chill { get; private set; } = null;
        public Ailment Shock { get; private set; } = null;
        public Ailment Freeze { get; private set; } = null;
        public List<Ailment> IgniteStacks { get; private set; } = new List<Ailment>();


        private void Awake()
        {
            owner = GetComponent<Enemy>();
        }

        private float Formula(float damage, float hp, float increasedBy)
        {
            return 0.5f * (float)Math.Pow(damage / hp, 0.4) * ((increasedBy/100) + 1);
        }


        private IEnumerator RemoveAilmentWhenExpired(Ailment a)
        {
            yield return new WaitForSeconds(a.duration);
            RemoveAilment(a);
        }

        private void RemoveAilment(Ailment a)
        {
            print("Removing ailment " + a.type);

            if (a.type == Ailments.Chill)
            {
                Chill = null;
            }

            if (a.type == Ailments.Freeze)
            {
                Freeze = null;
            }

            if (a.type == Ailments.Shock)
            {
                Shock = null;
            }

            if (a.type == Ailments.Ignite)
            {
                IgniteStacks.Remove(a);
            }
        }

        private void AddAilment(Ailment a)
        {

            if (a.type == Ailments.Ignite)
            {
                print($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                IgniteStacks.Add(a);
                StartCoroutine(RemoveAilmentWhenExpired(a));
                return;
            }

            if (a.type == Ailments.Chill)
            {
                if (Chill == null || a.magnitude > Chill.magnitude)
                {
                    print($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                    Chill = a;
                    StartCoroutine(RemoveAilmentWhenExpired(a));
                    return;
                }
            }

            if (a.type == Ailments.Freeze)
            {
                if (Freeze == null || a.duration > Freeze.duration)
                {
                    print($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                    Freeze = a;
                    StartCoroutine(RemoveAilmentWhenExpired(a));
                    return;
                }
            }

            if (a.type == Ailments.Shock)
            {
                if (Shock == null || a.magnitude > Shock.magnitude)
                {
                    print($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                    Shock = a;
                    StartCoroutine(RemoveAilmentWhenExpired(a));
                    return;
                }
            }
        }


        internal void CheckIfDamageAppliesAilment(SpellSlot from, int fireFinal, int coldFinal, int lightFinal)
        {
            CheckForIgnite(from, fireFinal);
            CheckForFeezeAndChill(from, coldFinal);
            CheckForShock(from, lightFinal);
        }

        private void CheckForShock(SpellSlot from, float damageAmount)
        {
            // chance based

            if (damageAmount <= 0)
            {
                return;
            }

            var shockChance = from.GetTotal("ChanceToShock") / 100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToShock");

            var totalChance = shockChance * (1 + chanceIncreasedBy);

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }

            float effectIncreasedBy = from.GetTotal("IncreasedEffectOfShock") / 100f;
            effectIncreasedBy += from.GetTotal("IncreasedEffectOfLightningAilments") / 100f;
            var effect = Formula(damageAmount, owner.totalHealth, effectIncreasedBy);

            if (effect < minShockEffect)
            {
                // do nothing
                return;
            }

            if (effect > maxShockEffect)
            {
                effect = maxShockEffect;
            }

            var duration = baseShockDurationSeconds;
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfLigtningAilments") / 100f;

            duration *= 1 + durationIncreasedBy;

            Ailment a = new Ailment();
            a.type = Ailments.Shock;
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

            var freezeChance = from.GetTotal("ChanceToFreeze") / 100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToFreeze") / 100f;

            var totalChance = freezeChance * (1 + chanceIncreasedBy);

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }


            // Freeze duration is 60 milliseconds (0.06 seconds) for every 1% of the target's maximum life

            float damagePercentage = damageAmount / owner.totalHealth;
            float dur = damagePercentage * 0.06f;


            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfFreeze") / 100f;
            durationIncreasedBy += from.GetTotal("IncreasedDurationOfColdAilments") / 100f;

            float durationReducedBy = from.GetTotal("ReducedDurationOfFreeze") / 100f;

            dur = dur * (1 + durationIncreasedBy) * (1 - durationReducedBy);

            if (dur < minFreezeDur)
            {
                // do nothing
                return;
            }

            if (dur > maxFreezeDur)
            {
                dur = maxFreezeDur;
            }

            Ailment a = new Ailment();
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

            float increasedBy = from.GetTotal("IncreasedEffectOfChill") / 100f;
            increasedBy += from.GetTotal("IncreasedEffectOfColdAilments") / 100f;

            var effect = Formula(damageAmount, owner.totalHealth, increasedBy);

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
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfColdAilments") / 100f;

            duration *= 1 + durationIncreasedBy;

            Ailment a = new Ailment();
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


        private void CheckForIgnite(SpellSlot from, float damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            var igniteChance = from.GetTotal("ChanceToIgnite") / 100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToIgnite") / 100f;

            var totalChance = igniteChance * (1 + chanceIncreasedBy);

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }

            var duration = baseIgniteDurationSeconds;
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfFireAilments") / 100f;

            duration *= 1 + durationIncreasedBy;


            var increasedDamage = from.GetTotal("IncreasedIgniteDamage") / 100f;
            var damageMultip = 1 + increasedDamage;

            // The burning damage over time is 50% of the base damage of the hit
            int totalDamage = (int)(damageAmount * damageMultip * baseIgniteDPSMultip);


            Ailment a = new Ailment();
            a.type = Ailments.Ignite;
            a.duration = duration;
            a.magnitude = totalDamage;

            AddAilment(a);
        }


    }
}
