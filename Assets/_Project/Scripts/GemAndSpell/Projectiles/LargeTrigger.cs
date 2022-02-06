using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class LargeTrigger : MonoBehaviour
    {


        #region Variables

        public Action<Collider> TriggerEnter;
        public Action<Collider> TriggerExit;

        #endregion

        #region Unity Methods

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter?.Invoke(other);

            //Enemy collidedEnemy = other.gameObject.GetComponent<Enemy>();

            //if (collidedEnemy != null)
            //{
            //    return;
            //}
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit?.Invoke(other);

            //Enemy collidedEnemy = other.gameObject.GetComponent<Enemy>();

            //if (collidedEnemy != null)
            //{
            //    return;
            //}
        }

        #endregion

        #region Methods

        #endregion

    }
}
