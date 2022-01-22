using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class AilmentManager
    {
        private Enemy owner;
        private List<Ailment> igniteStacks = new List<Ailment>();
        private List<Ailment> chillStacks = new List<Ailment>();
        private List<Ailment> freezeStacks = new List<Ailment>();
        private List<Ailment> shockStacks = new List<Ailment>();

        private float igniteTick;

        public Ailment strongestIgnite;
        public Ailment strongestChill;
        public Ailment strongestFreeze;
        public Ailment strongestShock;


        private const float minChillEffect = 0.05f; // 5%
        private const float maxChillEffect = 0.30f; // 30%
        private const float baseChillDurationSeconds = 2;


        private const float baseFreezeDuration = 0.06f; // 0.06 seconds
        private const float minFreezeEffect = 0.06f; // 0.06 seconds
        private const float maxFreezeEffect = 3f; // 3 seconds%

        private const float minShockEffect = 0.05f; // 5%
        private const float maxShockEffect = 0.50f; // 50%
        private const float baseShockDurationSeconds = 2;

        const float baseIgniteDPSMultip = 0.5f; // 50% of the base damage of the hit 
        const float baseIgniteDurationSeconds = 4;
        const float igniteTickIntervalSeconds = 0.5f;

        public AilmentManager(Enemy enemy)
        {
            owner = enemy;
            igniteStacks = new List<Ailment>();
        }

        private float Formula(float damage, float hp, float increasedBy)
        {
            return 0.5f * (float)Math.Pow(damage / hp, 0.4) * ((increasedBy/100) + 1);
        }

        private void AddAilment(Ailment a)
        {
            Debug.Log($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
            Debug.Log($"igniteStacks: {igniteStacks.Count}");
            Debug.Log($"chillStacks: {chillStacks.Count}");
            Debug.Log($"freezeStacks: {freezeStacks.Count}");
            Debug.Log($"shockStacks: {shockStacks.Count}");

            a.startTime = Time.time;

            if (a.type == Ailments.Ignite)
            {
                igniteStacks.Add(a);
            }

            if (a.type == Ailments.Chill)
            {
                chillStacks.Add(a);
            }

            if (a.type == Ailments.Freeze)
            {
                freezeStacks.Add(a);
            }

            if (a.type == Ailments.Freeze)
            {
                shockStacks.Add(a);
            }
        }

        // apply chill effect at 10% for 0.25 seconds
        internal void ApplyChillingGround()
        {
            Ailment a = new Ailment();
            a.type = Ailments.Chill;
            a.duration = 0.25f;
            a.magnitude = 0.1f;

            AddAilment(a);
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
            bool frozen = strongestFreeze != null;

            if (frozen) // if frozen no need to chill
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

            var shockChance = from.GetTotal("ChanceToFreeze") / 100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToFreeze") / 100f;

            var totalChance = shockChance * (1 + chanceIncreasedBy);

            var cointoss = UnityEngine.Random.value;

            if (cointoss > totalChance)
            {
                return;
            }

            float effectIncreasedBy = from.GetTotal("IncreasedEffectOfFreeze") / 100f;
            effectIncreasedBy += from.GetTotal("IncreasedEffectOfColdAilments") / 100f;

            var effect = Formula(damageAmount, owner.totalHealth, effectIncreasedBy);

            if (effect < minFreezeEffect)
            {
                // do nothing
                return;
            }

            if (effect > maxFreezeEffect)
            {
                effect = maxFreezeEffect;
            }

            var duration = baseFreezeDuration;
            float durationIncreasedBy = from.GetTotal("IncreasedDurationOfColdAilments");

            duration *= 1 + durationIncreasedBy;

            Ailment a = new Ailment();
            a.type = Ailments.Freeze;
            a.duration = duration;
            a.magnitude = effect;

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
        }

        private void CheckForIgnite(SpellSlot from, float damageAmount)
        {
            // chance based

            if (damageAmount <= 0)
            {
                return;
            }

            var shockChance = from.GetTotal("ChanceToIgnite") / 100f;
            float chanceIncreasedBy = from.GetTotal("IncreasedChanceToIgnite") / 100f;

            var totalChance = shockChance * (1 + chanceIncreasedBy);

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

            int totalDamage = (int)(damageAmount * damageMultip * baseIgniteDPSMultip);


            Ailment a = new Ailment();
            a.type = Ailments.Ignite;
            a.duration = duration;
            a.magnitude = totalDamage;

            AddAilment(a);
        }

        public void UpdateAilments()
        {
            RemoveExpiredStakcs();

            strongestIgnite = FindStrongest(igniteStacks);
            strongestChill = FindStrongest(chillStacks);
            strongestFreeze = FindStrongest(freezeStacks);
            strongestShock = FindStrongest(shockStacks);

            CheckIgniteTick();
        }

        private void CheckIgniteTick()
        {
            if (strongestIgnite != null)
            {
                if (Time.time - igniteTick > igniteTickIntervalSeconds)
                {
                    Debug.Log($"Taking damage from {igniteStacks.Count} ignite stacks");

                    foreach (var item in igniteStacks) // all ignites
                    {
                        owner.IgniteTick((int)item.magnitude);
                    }

                    igniteTick = Time.time;
                }
            }
        }

        private void RemoveStack(List<Ailment> stacks, Ailment a)
        {
            stacks.Remove(a);
        }

        private void RemoveExpiredStakcs()
        {
            RemoveExpiredFrom(igniteStacks);
            RemoveExpiredFrom(chillStacks);
            RemoveExpiredFrom(freezeStacks);
            RemoveExpiredFrom(shockStacks);
        }

        private void RemoveExpiredFrom(List<Ailment> stacks)
        {
            var now = Time.time;

            // remove expired stacks
            for (int i = stacks.Count - 1; i >= 0; i--)
            {
                Ailment a = stacks[i];

                if (now - a.startTime > a.duration)
                {
                    RemoveStack(stacks, a);
                    continue;
                }
            }
        }



        private Ailment FindStrongest(List<Ailment> stacks)
        {
            if (stacks.Count == 0)
            {
                return null;
            }

            return stacks.OrderBy(a => a.magnitude).First();
        }

    }
}
