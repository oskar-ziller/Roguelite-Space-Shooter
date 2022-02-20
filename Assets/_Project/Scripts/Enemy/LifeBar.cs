using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class LifeBar : MonoBehaviour
    {

        #region Variables

        private Enemy owner;
        private MeshRenderer mesh;

        public float tweenDur = 0.1f;
        public float tweenVariation = 0.05f;

        private float percentageTweening = 1f; // used for tween

        private Tween barPercentTween;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            owner = GetComponent<Enemy>();
            mesh = GetComponent<MeshRenderer>();

            owner.Died += OnOwnerDied;
        }

        private void OnOwnerDied(Enemy obj)
        {
            barPercentTween.Kill();
        }

        private void Start()
        {
            owner.HealthChanged += UpdateHealthBar;
            mesh.material.SetFloat("_percentage", 1);
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
                mesh.material.SetFloat("_percentage", percentageTweening);
            }

            var current = mesh.material.GetFloat("_percentage");
            var target = (float)owner.CurrentHealth / owner.TotalHealth;


            var dur = tweenDur + Random.Range(-tweenVariation, tweenVariation);

            barPercentTween = DOTween.To(() => percentageTweening, x => percentageTweening = x, target, dur).SetUpdate(true);
            //barPercentTween.onStepComplete += StepComplete;
            barPercentTween.onUpdate += StepComplete;
        }


        private void StepComplete()
        {
            //print("step");
            mesh.material.SetFloat("_percentage", percentageTweening);
        }

        private void UpdateHealthBar(Enemy _)
        {
            //gameObject.SetActive(true);
            var current = mesh.material.GetFloat("_percentage");
            percentageTweening = current;
            StartBarPercentTween();

            //if (hideLifeBar_Co != null)
            //{
            //    StopCoroutine(hideLifeBar_Co);
            //}

            //if (owner.hideLifebarAfterSeconds > 0)
            //{
            //    hideLifeBar_Co = StartCoroutine(HideHealthbar());
            //}
        }


        //private IEnumerator HideHealthbar()
        //{
        //    yield return new WaitForSeconds(owner.hideLifebarAfterSeconds);
        //    gameObject.SetActive(false);
        //}


        #endregion

    }
}
