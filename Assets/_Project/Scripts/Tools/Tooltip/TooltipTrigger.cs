using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace MeteorGame
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string infoText;
        public float infoDelay;
        public float gemInfoDelay;
        Coroutine showUpRoutine;
        Coroutine showUpRoutineGem;

        public bool showInfo, showGemInfo;

        private GemItem gem;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (showInfo)
            {
                showUpRoutine = StartCoroutine(ShowInfoWithDelay());
            }

            if (showGemInfo)
            {
                showUpRoutineGem = StartCoroutine(ShowGemInfoWithDelay());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showUpRoutine != null)
            {
                StopCoroutine(showUpRoutine);
            }

            if (showUpRoutineGem != null)
            {
                StopCoroutine(showUpRoutineGem);
            }

            TooltipSystem.HideInfo();
            TooltipSystem.HideGemInfo();
        }

        private IEnumerator ShowInfoWithDelay()
        {
            yield return new WaitForSecondsRealtime(infoDelay);
            TooltipSystem.ShowInfo(infoText);
        }

        private IEnumerator ShowGemInfoWithDelay()
        {
            yield return new WaitForSecondsRealtime(gemInfoDelay);
            TooltipSystem.ShowGemInfo(gem);
        }


        internal void SetupGemInfoTooltip(GemItem gem)
        {
            showGemInfo = true;
            this.gem = gem;
        }

        private void OnDisable()
        {
            TooltipSystem.HideGemInfo();
            TooltipSystem.HideInfo();
        }
    }
}
