using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Enemies
{
    public class ExplosionHandler : MonoBehaviour
    {

        #region Variables

        //[SerializeField] private List<ParticleSystem> particleSystems;

        [Tooltip("Maximum duration of child particle systems. Object gets destroyed after.")]
        [SerializeField] private float maxDur = 0f;

        #endregion

        #region Unity Methods

        private void Start()
        {
            Destroy(gameObject, maxDur);
        }


        #endregion

        #region Methods


        #endregion

    }
}
