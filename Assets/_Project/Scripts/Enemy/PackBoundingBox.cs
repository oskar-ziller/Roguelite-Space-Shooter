using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace MeteorGame
{
    public class PackBoundingBox : MonoBehaviour
    {

        #region Variables

        private Renderer rend;
        public float fadeDur = 2f;

        private float myAlpha;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {
            rend = GetComponent<Renderer>();
            myAlpha = rend.material.GetFloat("_alpha");
            DOTween.To(() => myAlpha, x => myAlpha = x, 0, fadeDur);
            Destroy(gameObject, fadeDur);
        }

        private void Update()
        {
            rend.material.SetFloat("_alpha", myAlpha);
        }

        #endregion

        #region Methods

        #endregion

    }
}
