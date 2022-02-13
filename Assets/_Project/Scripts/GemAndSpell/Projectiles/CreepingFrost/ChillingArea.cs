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

        private SphereCollider collider;

        public Action<ChillingArea> AreaExpired;


        private SpellSlot castBy;

        public SpellSlot CastBy => castBy;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            collider = GetComponent<SphereCollider>();
        }


        internal void Init(SpellSlot castBy)
        {
            this.castBy = castBy;
            transform.parent = null;
            gameObject.SetActive(true);
            SpellCaster.AddCreepingFrostChillingArea(this);
            transform.DOScale(castBy.Modifiers.ExplosionRadi * 2, expandDur).SetEase(easingExpand);

            // setup destruction
            
            //StartCoroutine(DestroyWithDelay(castBy.Modifiers.ProjectileLifetime));

            // if have too many, destroy oldest
            var countLimit = castBy.GetTotal("ChillingAreaLimit");

            if (SpellCaster.CreepingFrostChillingAreaCount() > countLimit)
            {
                print("we have too many ChillingArea");
                var oldest = SpellCaster.GetOldestCreepingFrostChillingArea();
                oldest.DestroySelf();
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
                AreaExpired?.Invoke(this);
                Destroy(transform.gameObject);
            });
        }

        #endregion

    }
}
