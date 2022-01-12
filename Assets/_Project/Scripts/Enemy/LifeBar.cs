using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class LifeBar : MonoBehaviour
    {

        #region Variables

        [Tooltip("The lifebar belongs to")]
        [SerializeField] Enemy owner;

        //[Tooltip("Health Canvas")]
        //[SerializeField] private Canvas lifeBarCanvas;

        [Tooltip("Health Slider from UI Canvas")]
        [SerializeField] private Slider healthSlider;

        //[Tooltip("billboard script of canvas (to set offset)")]
        //[SerializeField] private Billboard billboard;

        private Coroutine hideLifeBar_Co;


        #endregion

        #region Unity Methods

        private void Awake()
        {
        }

        private void Start()
        {
            owner.DamageTaken += UpdateHealthBar;
            SetSliderValToOwnerHealth();

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

        private void SetSliderValToOwnerHealth()
        {
            healthSlider.value = (float)owner.currentHealth / owner.totalHealth;
        }

        private void UpdateHealthBar(Enemy _)
        {
            gameObject.SetActive(true);
            SetSliderValToOwnerHealth();

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
