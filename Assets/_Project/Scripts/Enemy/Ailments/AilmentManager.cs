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
        private const int baseShockDurationSeconds = 2;

        const float baseIgniteDPS = 0.5f; // 50% of the base damage of the hit 
        const int baseIgniteDurationSeconds = 4;
        const float igniteTickIntervalSeconds = 0.5f;

        public AilmentManager(Enemy enemy)
        {
            owner = enemy;
            igniteStacks = new List<Ailment>();
        }

        private float Formula(float damage, float hp, float increasedBy)
        {
            return 0.5f * (float)Math.Pow(damage / hp, 0.4) * (increasedBy + 1);
        }

        private void AddAilment(Ailment a, List<Ailment> target)
        {
            a.startTime = Time.time;

            if (target == igniteStacks)
            {
                a.type = Ailments.Ignite;
                //Debug.Log($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                igniteStacks.Add(a);
            }

            if (target == chillStacks)
            {
                a.type = Ailments.Chill;
                //Debug.Log($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                chillStacks.Add(a);
            }

            if (target == freezeStacks)
            {
                a.type = Ailments.Freeze;
                //Debug.Log($"Adding ailment {a.type} - duration: {a.duration}");
                freezeStacks.Add(a);
            }

            if (target == shockStacks)
            {
                a.type = Ailments.Shock;
                //Debug.Log($"Adding ailment {a.type} - magnitude: {a.magnitude} - duration: {a.duration}");
                shockStacks.Add(a);
            }
        }

        // apply chill effect at 10% for 0.25 seconds
        internal void ApplyChillingGround()
        {
            Ailment a = new Ailment();
            a.duration = 0.25f;
            a.magnitude = 0.1f;

            AddAilment(a, chillStacks);
        }

        public void CheckIfDamageAppliesAilment(SpellSlot from, int fireDamage, int coldDamage, int lightningDamage)
        {
            CheckForIgnite(from, fireDamage);
            CheckForFeezeAndChill(from, coldDamage);
            CheckForShock(from, lightningDamage);
        }

        private void CheckForShock(SpellSlot from, float damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            var shockChance = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("ChanceToShock"), from) / 100f;

            if (UnityEngine.Random.value < shockChance)
            {
                float increasedBy = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("IncreasedEffectOfShock"), from);
                var effect = Formula(damageAmount, owner.totalHealth, increasedBy);

                if (effect < minShockEffect)
                {
                    // do nothing
                    return;
                }

                if (effect > maxShockEffect)
                {
                    effect = maxShockEffect;
                }

                Ailment a = new Ailment();
                a.duration = baseShockDurationSeconds;
                a.magnitude = effect;

                AddAilment(a, shockStacks);
            }

            
        }

        private void CheckForFeezeAndChill(SpellSlot from, int damageAmount)
        {
            bool frozen = CheckForFreeze(from, damageAmount);

            if (frozen) // if frozen no need to chill
            {
                return;
            }

            CheckForChill(from, damageAmount);
        }

        private bool CheckForFreeze(SpellSlot from, int damageAmount)
        {
            if (damageAmount <= 0)
            {
                return false;
            }

            var freezeChance = ModifierHelper.GetTotal("ChanceToFreeze", from) / 100f;

            // When a target is frozen, it is also considered chilled.
            // As the target becomes unfrozen, chill will remain on the target for
            // an additional duration of 0.3 seconds
            if (UnityEngine.Random.value < freezeChance)
            {
                float increasedBy = ModifierHelper.GetTotal("ChanceToFreeze", from);
                increasedBy += ModifierHelper.GetTotal("ChanceToFreeze", from);
                increasedBy += ModifierHelper.GetTotal("ChanceToFreeze", from);

                // Base freeze duration is 60 milliseconds (0.06 seconds) for every 1% of the target's maximum life
                var damagePercentage = damageAmount / owner.totalHealth;
                var effect = baseFreezeDuration * (1 + increasedBy) * damagePercentage * 100f;

                //Debug.Log($"CheckForFreeze with damage: {damageAmount} - effect: {effect}");

                if (effect < minFreezeEffect)
                {
                    //Debug.Log($"{effect} < minFreezeEffect");
                    return false;
                }

                if (effect > maxFreezeEffect)
                {
                    effect = maxFreezeEffect;
                }

                Ailment a = new Ailment();
                a.duration = effect;
                a.magnitude = effect;

                AddAilment(a, freezeStacks);

                Ailment b = new Ailment();
                b.duration = effect + 0.3f;
                b.magnitude = 0.1f;

                AddAilment(b, chillStacks);

                return true;
            }

            return false;
        }

        private void CheckForChill(SpellSlot from, float damageAmount)
        {
            if (damageAmount == 0)
            {
                return;
            }

            float increasedBy = ModifierHelper.GetTotal("IncreasedEffectOfChill", from);
            increasedBy += ModifierHelper.GetTotal("IncreasedEffectOfColdAilments", from);

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
            float durationIncreasedBy = ModifierHelper.GetTotal("IncreasedDurationOfColdAilments", from);

            duration *= 1 + durationIncreasedBy;

            Ailment a = new Ailment();
            a.duration = duration;
            a.magnitude = effect;

            AddAilment(a, chillStacks);
        }

        private void CheckForIgnite(SpellSlot from, float damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            var igniteChance = ModifierHelper.GetTotal("ChanceToIgnite", from) / 100f;
            var increasedDamage = ModifierHelper.GetTotal("IncreasedIgniteDamage", from) / 100f;
            int totalDamage = (int)(damageAmount * baseIgniteDPS * (1 + increasedDamage));

            if (UnityEngine.Random.value < igniteChance)
            {
                Ailment a = new Ailment();
                a.duration = baseIgniteDurationSeconds;
                a.magnitude = totalDamage;

                AddAilment(a, igniteStacks);
            }
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

                    // strongest ignite
                    //owner.IgniteTick((int)strongestIgnite.magnitude);
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

            Ailment strongest = stacks.First();

            for (int i = 0; i < stacks.Count; i++)
            {
                Ailment a = stacks[i];

                if (a.magnitude > strongest.magnitude)
                {
                    strongest = a;
                }
            }

            return strongest;
        }

    }
}
