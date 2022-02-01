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
        CantLink,
        SpellSlotNeedsUnlock,
        GemAlreadyMaxLevel,

    }

    public class ErrorDisplayer : MonoBehaviour
    {

        #region Variables

        public float fadeDelaySeconds;
        public float fadeDuration;

        public float bgFadeDelaySeconds;
        public float bgFadeDuration;


        public TextMeshProUGUI currencyTMP;
        public Image currencyIcon;
        public Image darkOverlay;

        private TextMeshProUGUI errorTMP;
        private CanvasGroup errorCanvasGrp;

        private Sequence bgSequence, fadeSequence;

        private Color darkBgColor = new Color(0, 0, 0, 0.5f);

        private Coroutine currencyBlinkCoroutine;


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

            ShowDarkBG();
            FadeOutBG();
        }

        private void ShowDarkBG()
        {
            darkOverlay.color = darkBgColor;
        }


        private void ShowErrorBar(UIError e)
        {
            var errorText = "";
            errorCanvasGrp.alpha = 1;


            if (e == UIError.CantAfford)
            {
                errorText = "Can't afford.";

                if (currencyBlinkCoroutine != null)
                {
                    StopCoroutine(currencyBlinkCoroutine);
                    currencyBlinkCoroutine = StartCoroutine(BlinkCurrency());
                }

            }

            if (e == UIError.CantLink)
            {
                errorText = "Can't link more. Buy more link slots or replace existing ones.";
            }

            if (e == UIError.SpellSlotNeedsUnlock)
            {
                errorText = "You need to unlock SLOT 2 to equip";
            }

            errorTMP.text = errorText;
        }

        private void FadeOutBG()
        {
            darkOverlay.DOKill();
            ;

            if (bgSequence != null)
            {
                bgSequence.Kill();
            }

            bgSequence = DOTween.Sequence();
            bgSequence.AppendInterval(bgFadeDelaySeconds);
            bgSequence.Append(darkOverlay.DOFade(0, bgFadeDuration));
            bgSequence.SetUpdate(true);
            bgSequence.Play();
        }

        private void FadeOutErrorBar()
        {
            if (fadeSequence != null)
            {
                fadeSequence.Kill();
            }

            fadeSequence = DOTween.Sequence();
            fadeSequence.AppendInterval(fadeDelaySeconds);
            fadeSequence.Append(errorCanvasGrp.DOFade(0, fadeDuration));
            fadeSequence.SetUpdate(true);
            fadeSequence.Play();
        }

        private IEnumerator BlinkCurrency()
        {
            var currencyIconOrigColor = currencyIcon.color;
            var currencyOrigColor = currencyTMP.color;

            currencyIcon.color = Color.red;
            currencyTMP.color = Color.red;

            yield return new WaitForSecondsRealtime(0.1f);

            currencyIcon.color = currencyIconOrigColor;
            currencyTMP.color = currencyOrigColor;

            yield return new WaitForSecondsRealtime(0.1f);

            currencyIcon.color = Color.red;
            currencyTMP.color = Color.red;

            yield return new WaitForSecondsRealtime(0.1f);

            currencyIcon.color = currencyIconOrigColor;
            currencyTMP.color = currencyOrigColor;

            yield return new WaitForSecondsRealtime(0.1f);

            currencyIcon.color = Color.red;
            currencyTMP.color = Color.red;

            yield return new WaitForSecondsRealtime(0.1f);

            currencyIcon.color = currencyIconOrigColor;
            currencyTMP.color = currencyOrigColor;
        }








        #endregion

    }
}
