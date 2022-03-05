using MeteorGame.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class CoreMain : MonoBehaviour
    {
        #region Variables

        #endregion

        #region Unity Methods

        private void OnTriggerEnter(Collider other)
        {
            Enemy e = other.gameObject.GetComponent<Enemy>();

            if (e != null)
            {
                GameManager.Instance.InitGameOver(e);
            }
        }

        #endregion

        #region Methods

        #endregion

    }
}
