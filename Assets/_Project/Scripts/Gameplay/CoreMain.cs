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

        private void OnCollisionEnter(Collision collision)
        {
            Enemy e = collision.collider.gameObject.GetComponent<Enemy>();

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
