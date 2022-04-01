using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Enemies
{
    public class SpawnAreaVisual : MonoBehaviour
    {

        #region Variables

        private Material mat;
        private Color endColor = new(0, 0, 0, 0);

        #endregion

        #region Unity Methods

        private void Awake()
        {
            mat = GetComponentInChildren<MeshRenderer>().material;
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }



        #endregion

        #region Methods

        internal void FadeOut()
        {
            // 1 tane child var ama loop kurdum
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }

            mat.DOColor(endColor, .5f); 
        }

        #endregion

    }
}
