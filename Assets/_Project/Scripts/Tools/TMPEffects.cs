using DG.Tweening;
using ntw.CurvedTextMeshPro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame
{
    public class TMPEffects : MonoBehaviour
    {

        #region Variables
        private TextProOnACircle textProOnACircle;
        private TextMeshPro tmpObj;
        public float rotateSpeed = 0.1f;
        public float fadeTime = 3f;
        public float fadeDelay = 3f;


        Tween fadeTween;
        private float alphaTweening = 1f;

        private Color startingColor;
        private Color tweeningColor;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            textProOnACircle = GetComponent<TextProOnACircle>();
            tmpObj = GetComponent<TextMeshPro>();
            startingColor = tmpObj.color;

            GameManager.Instance.GameStart += OnGameStart;
        }

        private void Start()
        {

        }

        private void FixedUpdate()
        {
            if (fadeTween != null && fadeTween.IsActive() && fadeTween.IsPlaying())
            {
                tweeningColor.a = alphaTweening;
                tmpObj.color = tweeningColor;
            }

            if (tweeningColor.a > 0)
            {
                textProOnACircle.SetAngularOffset(textProOnACircle.AngularOffset + rotateSpeed);
            }
        }

        #endregion

        #region Methods

        private void OnGameStart()
        {
            fadeTween.Kill();

            tmpObj.color = startingColor;
            tweeningColor = startingColor;

            alphaTweening = 1f;
            fadeTween = DOTween.To(() => alphaTweening, x => alphaTweening = x, 0, fadeTime).SetDelay(fadeDelay);
        }

        #endregion

    }
}
