using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class ProjectileDummy : MonoBehaviour
    {

        #region Variables

        private SpinAround spinner;
        public Transform bodyMesh;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            spinner = GetComponent<SpinAround>();
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        internal void SetSpinnerVals(int id, int projectileCount, float startDeg)
        {
            spinner.projID = id;
            spinner.totalCount = projectileCount;
            spinner.startDeg = startDeg;
            spinner.ResetSelf();
        }

        #endregion

        #region Methods

        #endregion

    }
}
