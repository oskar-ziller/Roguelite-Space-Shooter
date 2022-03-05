using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame.Enemies
{
    public class LifeBar : MonoBehaviour
    {

        #region Variables

        private Enemy owner;
        private MeshRenderer renderer;

        public float tweenDur = 0.1f;
        public float tweenVariation = 0.05f;

        private float percentageTweening = 1f; // used for tween

        private Tween barPercentTween;

        #endregion

        #region Unity Methods

        private void Awake()
        {

            if (owner == null)
            {
                owner = GetComponent<Enemy>();
                owner.Died += OnOwnerDied;
            }

            if (renderer == null)
            {
                renderer = GetComponent<MeshRenderer>();
            }

        }

        private void OnOwnerDied(Enemy _, bool __)
        {
            barPercentTween.Kill();
        }

        private void Start()
        {
            owner.HealthChanged += UpdateHealthBar;
            renderer.material.SetFloat("_percentage", 1);
        }

        #endregion

        #region Methods

        internal void StartBarPercentTween()
        {
            if (barPercentTween != null && barPercentTween.active)
            {
                barPercentTween.Complete();
                barPercentTween.Kill();
                //percentageTweening = 1f;
                renderer.material.SetFloat("_percentage", percentageTweening);
            }

            var current = renderer.material.GetFloat("_percentage");
            var target = (float)owner.CurrentHealth / owner.TotalHealth;


            var dur = tweenDur + Random.Range(-tweenVariation, tweenVariation);

            barPercentTween = DOTween.To(() => percentageTweening, x => percentageTweening = x, target, dur);
            barPercentTween.onUpdate += StepComplete;
        }


        private void StepComplete()
        {
            renderer.material.SetFloat("_percentage", percentageTweening);
        }

        private void UpdateHealthBar(Enemy _)
        {
            var current = renderer.material.GetFloat("_percentage");
            percentageTweening = current;
            StartBarPercentTween();
        }




        #endregion

    }
}
