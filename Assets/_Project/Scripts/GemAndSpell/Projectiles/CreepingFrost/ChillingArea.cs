using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class ChillingArea : MonoBehaviour
    {

        #region Variables
        [Tooltip("Duration of transform scale tween")]
        [SerializeField] private float expandDur;

        [Tooltip("Duration of transform scale tween")]
        [SerializeField] private float shrinkDur;

        [Tooltip("Easing of transform scale tween")]
        [SerializeField] private Ease easingExpand;

        [Tooltip("Easing of transform scale tween")]
        [SerializeField] private Ease easingShrink;


        private SpellSlot castBy;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        private void OnEnable()
        {

        }

        internal void Init(SpellSlot castBy)
        {
            this.castBy = castBy;
            transform.parent = null;
            gameObject.SetActive(true);
            SpellCaster.AddCreepingFrostChillingArea(this);
            transform.DOScale(castBy.Spell.ExplosionRadius * 2, expandDur).SetEase(easingExpand);

            // setup destruction
            float baseDur = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("BaseDuration"), castBy);
            float increasedBy = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("IncreasedSkillEffectDuration"), castBy) / 100f;
            float reducedBy = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("ReducedSkillEffectDuration"), castBy) / 100f;
            float totalDur = baseDur * (1 + increasedBy) * (1 - reducedBy);

            StartCoroutine(DestroyWithDelay(totalDur));

            // if have too many, destroy oldest
            var countLimit = ModifierHelper.GetTotal(GameManager.Instance.GetModifierSO("ChillingAreaLimit"), castBy);

            if (SpellCaster.CreepingFrostChillingAreaCount() > countLimit)
            {
                print("we have too many ChillingArea");
                var oldest = SpellCaster.GetOldestCreepingFrostChillingArea();
                oldest.DestroySelf();
            }

            StartCoroutine(DealDPSLoop());
        }


        private void DealDamage()
        {
            float totalRadi = transform.localScale.x / 2f;

            foreach (Enemy e in EnemyManager.Instance.EnemiesInRange(transform.position, totalRadi, fromShell: true))
            {
                e.TakeDoT(castBy, 0.25f, applyAilment: false);
                e.ApplyChillingGround(); // apply chill effect at 10% for 0.25 seconds
            }
        }

        private IEnumerator DealDPSLoop()
        {
            while (true)
            {
                DealDamage();
                yield return new WaitForSeconds(0.25f);
            }
        }

        #endregion

        #region Methods

        private IEnumerator DestroyWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            DestroySelf();
            yield return null;
        }

        private void DestroySelf()
        {
            transform.DOKill();
            SpellCaster.RemoveCreepingFrostChillingArea(this);

            transform.DOScale(Vector3.zero, shrinkDur).SetEase(easingShrink).OnComplete(() => {
                Destroy(transform.gameObject);
            });
        }

        #endregion

    }
}
