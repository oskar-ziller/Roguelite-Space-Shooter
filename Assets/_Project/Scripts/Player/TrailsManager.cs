using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class TrailsManager : MonoBehaviour
    {

        #region Variables

        [SerializeField] private float minSpeedForTrail = 2f;


        private List<TrailRenderer> trails;
        private bool trailsDisabled = false;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            trails = GetComponentsInChildren<TrailRenderer>().ToList();
        }

        private void FixedUpdate()
        {
            if (Player.Instance.Speed < minSpeedForTrail)
            {
                DisableTrails();
            }
            else
            {
                EnableTrails();
            }
        }

        #endregion

        #region Methods

        private void DisableTrails()
        {
            if (!trailsDisabled)
            {
                foreach (var trail in trails)
                {
                    trail.emitting = false;
                }

                trailsDisabled = true;
            }
        }


        private void EnableTrails()
        {
            if (trailsDisabled)
            {
                foreach (var trail in trails)
                {
                    trail.emitting = true;
                }

                trailsDisabled = false;
            }
        }

        #endregion

    }
}
