using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class GoldCoinDrop : MonoBehaviour
    {

        #region Variables

        private AudioSource audioSource;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        private void OnCollisionEnter(Collision collision)
        {
            var p = collision.gameObject.GetComponent<Player>();

            if (p != null)
            {
                audioSource.Play();
                GetComponent<Renderer>().enabled = false;
                Destroy(gameObject, 1f);
            }
        }


        #endregion

        #region Methods

        #endregion

    }
}
