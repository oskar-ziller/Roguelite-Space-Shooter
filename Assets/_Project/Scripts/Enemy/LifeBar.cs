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
        //private Coroutine hideLifeBar_Co;
        private MeshRenderer mesh;
        private Material mat;

        public float tweenDur = 0.1f;
        public float tweenVariation = 0.05f;


        private float percentageTweening = 1f; // used for tween

        #endregion

        #region Unity Methods

        private void Awake()
        {
            owner = GetComponentInParent<Enemy>();
            mesh = GetComponent<MeshRenderer>();
            mat = mesh.material;
        }

        private void Start()
        {
            owner.DamageTaken += UpdateHealthBar;

            mat.SetFloat("_percentage", 1);
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods



        private Tween barPercentTween;

        internal void StartBarPercentTween()
        {
            if (barPercentTween != null && barPercentTween.active)
            {
                barPercentTween.Complete();
                barPercentTween.Kill();
                //percentageTweening = 1f;
                mat.SetFloat("_percentage", percentageTweening);
            }

            var current = mat.GetFloat("_percentage");
            var target = (float)owner.currentHealth / owner.totalHealth;


            var dur = tweenDur + Random.Range(-tweenVariation, tweenVariation);

            barPercentTween = DOTween.To(() => percentageTweening, x => percentageTweening = x, target, dur).SetUpdate(true);
            //barPercentTween.onStepComplete += StepComplete;
            barPercentTween.onUpdate += StepComplete;
        }


        private void StepComplete()
        {
            print("step");
            mat.SetFloat("_percentage", percentageTweening);
        }


 

        private void UpdateHealthBar(Enemy _)
        {
            //gameObject.SetActive(true);
            var current = mat.GetFloat("_percentage");
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
