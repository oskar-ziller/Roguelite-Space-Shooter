using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Enemies
{
    public class SpawnBox : MonoBehaviour
    {

        #region Variables

        [Tooltip("Holder that holds visuals, particles and Stencil material")]
        [SerializeField] private Transform holder;

        public event Action AnimCompleted;
        
        
        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods

        public void OnAnimCompleted()
        {
            AnimCompleted?.Invoke();
            Destroy(gameObject);
        }

        internal void SetSize(float packSize)
        {
            holder.localScale = packSize * Vector3.one;
        }

        #endregion

    }
}
