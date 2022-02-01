using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UnityEngine.UI;

namespace MeteorGame
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        public RectTransform infoRT;
        public RectTransform gemInfoRT;

        public TextMeshProUGUI infoTMP, statsTMP, nameTMP, descTMP, levelTMP;

        [Tooltip("Offset of the tooltip from mouse position")]
        public Vector2 infoPadding;
        public Vector2 gemInfoPadding;

        [Tooltip("Fadein duration of tooltip")]
        public float fadeDur;
        
        public Canvas tooltipCanvas;
        private CanvasScaler canvasScaler;

        Tween infoFadeTween;
        Tween gemInfoFadeTween;

        public Image gemInfoLine;


        private CanvasGroup infoCanvasGrp;
        private CanvasGroup gemInfoCanvasGrp;

        #region Unity Methods

        private void Awake()
        {
            infoCanvasGrp = infoRT.gameObject.GetComponent<CanvasGroup>();
            gemInfoCanvasGrp = gemInfoRT.gameObject.GetComponent<CanvasGroup>();
            canvasScaler = GetComponent<CanvasScaler>();

            Instance = this;
        }

        private void Update()
        {
            Vector2 mousePos = Input.mousePosition;

            if (Instance.gemInfoRT.gameObject.activeSelf)
            {
                Vector2 gemInfoTooltipPos = mousePos;

                if (mousePos.x > Screen.width / 2)
                {
                    // if cursor is on the right half of screen, show up on left side of cursor
                    gemInfoTooltipPos.x -= Instance.gemInfoRT.sizeDelta.x * tooltipCanvas.scaleFactor;

                    // add X padding
                    gemInfoTooltipPos.x += gemInfoPadding.x * tooltipCanvas.scaleFactor; 
                }
                else
                {
                    // remove X padding if on the other half of screen
                    gemInfoTooltipPos.x -= gemInfoPadding.x * tooltipCanvas.scaleFactor;
                }

                // add Y padding
                gemInfoTooltipPos.y += gemInfoPadding.y * tooltipCanvas.scaleFactor;


                // make sure tooltip wont get out of the screen on the bottom
                if (gemInfoTooltipPos.y < Instance.gemInfoRT.sizeDelta.y * tooltipCanvas.scaleFactor)
                {
                    gemInfoTooltipPos.y = Instance.gemInfoRT.sizeDelta.y * tooltipCanvas.scaleFactor;
                }

                Instance.gemInfoRT.position = gemInfoTooltipPos;
            }

            if (Instance.infoRT.gameObject.activeSelf)
            {
                Vector2 tooltipPos = mousePos;

                if (mousePos.x < Screen.width / 2)
                {
                    // if cursor is on the left half of screen, show up on left side of cursor
                    tooltipPos.x -= Instance.infoRT.sizeDelta.x * tooltipCanvas.scaleFactor;

                    // remove X padding
                    tooltipPos.x -= infoPadding.x * tooltipCanvas.scaleFactor;
                }
                else
                {
                    // add X padding if on the other half of screen
                    tooltipPos.x += infoPadding.x * tooltipCanvas.scaleFactor;
                }

                // add Y padding
                tooltipPos.y += infoPadding.y * tooltipCanvas.scaleFactor;


                // make sure tooltip wont get out of the screen on the bottom
                if (tooltipPos.y < Instance.infoRT.sizeDelta.y * tooltipCanvas.scaleFactor)
                {
                    tooltipPos.y = Instance.infoRT.sizeDelta.y * tooltipCanvas.scaleFactor;
                }

                Instance.infoRT.transform.position = tooltipPos;
            }

        }





        #endregion

        #region Methods

        public static void ShowInfo(string tooltipText)
        {
            Instance.infoTMP.text = tooltipText;
            Instance.infoRT.gameObject.SetActive(true);
            Instance.infoCanvasGrp.DOFade(1, Instance.fadeDur).SetUpdate(true);
        }

        public static void HideInfo()
        {
            if (Instance.infoCanvasGrp == null)
            {
                return;
            }

            Instance.infoFadeTween.Kill();
            Instance.infoCanvasGrp.alpha = 0;
            Instance.infoRT.gameObject.SetActive(false);
        }

        public static void ShowGemInfo(GemItem gem)
        {
            Instance.gemInfoRT.gameObject.SetActive(true);
            Instance.gemInfoCanvasGrp.DOFade(1, Instance.fadeDur).SetUpdate(true);

            //Instance.gemInfoLine.color = gem.Color;
            Instance.nameTMP.text = gem.Name;
            Instance.statsTMP.text = gem.GetStatsStringForUI();
            Instance.descTMP.text = "    " + gem.Description;
            Instance.levelTMP.text = "Level: " + gem.Level;
        }

        public static void HideGemInfo()
        {
            if (Instance.gemInfoCanvasGrp == null)
            {
                return;
            }

            Instance.gemInfoFadeTween.Kill();
            Instance.gemInfoCanvasGrp.alpha = 0;
            Instance.gemInfoRT.gameObject.SetActive(false);
        }

        #endregion

    }
}
