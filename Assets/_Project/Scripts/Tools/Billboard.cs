using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{


    public class Billboard : MonoBehaviour
    {
        private Transform enemyTransform;

        private float startingY;


        #region Unity Methods

        private void Awake()
        {
            //gameObject.SetActive(false);
        }

        private void Start()
        {
            enemyTransform = GetComponentInParent<Enemy>().transform;
            startingY = transform.localPosition.y;
            StartCoroutine(LifebarLookatCoroutine());
        }


        private void LateUpdate()
        {
            
        }

        
        private IEnumerator LifebarLookatCoroutine()
        {
            while (true)
            {
                var isPlayerAboveEnemy = Player.Instance.transform.position.y > enemyTransform.position.y;

                var location = isPlayerAboveEnemy ? -startingY : startingY;
                transform.localPosition = location * Vector3.up;
                transform.LookAt(Player.Instance.transform.position);
                yield return new WaitForSeconds(0.1f);
            }
        }

        #endregion

    }
}
