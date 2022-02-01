using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class LifeBar : MonoBehaviour
    {

        #region Variables

        private Enemy owner;
        private Coroutine hideLifeBar_Co;
        private MeshRenderer mesh;
        private Material mat;


        #endregion

        #region Unity Methods

        private void Awake()
        {
            owner = GetComponentInParent<Enemy>();
            mesh = GetComponent<MeshRenderer>();
            mat = mesh.material;
        }

        private void Start()
        {
            owner.DamageTaken += UpdateHealthBar;

            mat.SetFloat("_percentage", 1);

            // if not always on, start off
            if (owner.hideLifebarAfterSeconds != 0)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods

        private void SetBarPercent()
        {
            var percentage = (float)owner.currentHealth / owner.totalHealth;
            mat.SetFloat("_percentage", percentage);
        }

        private void UpdateHealthBar(Enemy _)
        {
            gameObject.SetActive(true);
            SetBarPercent();

            if (hideLifeBar_Co != null)
            {
                StopCoroutine(hideLifeBar_Co);
            }

            if (owner.hideLifebarAfterSeconds > 0)
            {
                hideLifeBar_Co = StartCoroutine(HideHealthbar());
            }
        }


        private IEnumerator HideHealthbar()
        {
            yield return new WaitForSeconds(owner.hideLifebarAfterSeconds);
            gameObject.SetActive(false);
        }


        #endregion

    }
}
