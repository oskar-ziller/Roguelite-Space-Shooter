using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace MeteorGame.Enemies
{
    public class PackBox : MonoBehaviour
    {

        #region Variables

        [SerializeField] private Transform sphere;
        [SerializeField] private TextMeshPro text;

        [Tooltip("When enemies die, how fast the centerPos changes and sphere scales")]
        [SerializeField] private float scaleSpeed = 1f;
        [SerializeField] private float moveSpeed = 1f;

        private Material sphereMat; // used for fading in
        private EnemyPack pack;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            var sphereRendererComponent = sphere.gameObject.GetComponent<MeshRenderer>();
            sphereMat = sphereRendererComponent.material;
        }

        private void Start()
        {
            MakeInvisAtStart();
        }

        private void FixedUpdate()
        {
            if (pack != null)
            {
                SetScale();
                SetPos();
            }
        }

        #endregion

        #region Methods

        private void SetScale()
        {
            sphere.localScale = Vector3.Lerp(sphere.localScale, Vector3.one * pack.PackSize, scaleSpeed * Time.fixedDeltaTime);
        }

        private void SetPos()
        {
            transform.position = Vector3.Lerp(transform.position, pack.Centroid, moveSpeed * Time.fixedDeltaTime);
        }

        internal void SetPack(EnemyPack p)
        {
            pack = p;
        }

        internal void FadeIn()
        {
            DoLookAt();

            var c = sphereMat.color;
            var fadeinColor = new Color(c.r, c.g, c.b, 1);

            sphereMat.DOColor(fadeinColor, 2f).SetDelay(2f);

            FadeInText();
        }


        private void MakeInvisAtStart()
        {
            var c = sphereMat.color;
            var fadeOutColor = new Color(c.r, c.g, c.b, 0);
            sphereMat.color = fadeOutColor;

            text.alpha = 0f;
        }

        private void FadeInText()
        {
            text.DOFade(1f, 2f).SetDelay(2f);
        }

        private void DoLookAt()
        {
            transform.LookAt(Vector3.zero);
        }

        #endregion

    }
}
