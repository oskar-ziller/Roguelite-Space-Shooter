using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public enum UIError
    {
        CantAfford,
        CantLink
    }

    public class ErrorDisplayer : MonoBehaviour
    {

        #region Variables

        public float fadeDelaySeconds;
        public float fadeDuration;

        public TextMeshProUGUI currencyTMP;
        public Image currencyIcon;

        private TextMeshProUGUI errorTMP;
        private CanvasGroup errorCanvasGrp;

        private Sequence mySequence;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            errorTMP = GetComponentInChildren<TextMeshProUGUI>();
            errorCanvasGrp = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods

        public void DisplayError(UIError error)
        {
            ShowErrorBar(error);
            FadeOutErrorBar();
        }


        private void ShowErrorBar(UIError e)
        {
            var errorText = "";
            errorCanvasGrp.alpha = 1;


            if (e == UIError.CantAfford)
            {
                errorText = "Can't afford.";
                StartCoroutine(BlinkCurrency());
            }

            if (e == UIError.CantLink)
            {
                errorText = "Can't link more. Buy more link slots or replace existing ones.";
            }

            errorTMP.text = errorText;
        }

        private void FadeOutErrorBar()
        {
            if (mySequence != null)
            {
                mySequence.Kill();
            }

            mySequence = DOTween.Sequence();
            mySequence.AppendInterval(fadeDelaySeconds);
            mySequence.Append(errorCanvasGrp.DOFade(0, fadeDuration));
            mySequence.Play();
        }

        private IEnumerator BlinkCurrency()
        {
            var currencyIconOrigColor = currencyIcon.color;
            var currencyOrigColor = currencyTMP.color;

            currencyIcon.color = Color.red;
            currencyTMP.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            currencyIcon.color = currencyIconOrigColor;
            currencyTMP.color = currencyOrigColor;

            yield return new WaitForSeconds(0.1f);

            currencyIcon.color = Color.red;
            currencyTMP.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            currencyIcon.color = currencyIconOrigColor;
            currencyTMP.color = currencyOrigColor;

            yield return new WaitForSeconds(0.1f);

            currencyIcon.color = Color.red;
            currencyTMP.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            currencyIcon.color = currencyIconOrigColor;
            currencyTMP.color = currencyOrigColor;
        }








        #endregion

    }
}
