using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class AilmentVisuals : MonoBehaviour
    {

        #region Variables

        [SerializeField]
        private GameObject igniteVisuals;

        [SerializeField]
        private GameObject chillAndFreezeVisuals;

        [SerializeField]
        private GameObject shockVisuals;

        private AilmentManager ailmentManager;


        #endregion

        #region Unity Methods

        private void Awake()
        {
        }

        private void Start()
        {
            ailmentManager = GetComponentInParent<AilmentManager>();
            StartCoroutine(CheckAilmentsCoroutine());
        }

        #endregion

        #region Methods

        IEnumerator CheckAilmentsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
                CheckAilmentsAndUpdateVisuals();
            }
        }
        private void CheckAilmentsAndUpdateVisuals()
        {
            if (ailmentManager.InChillingArea || ailmentManager.Chill != null || ailmentManager.Freeze != null)
            {
                if (!chillAndFreezeVisuals.activeInHierarchy)
                {
                    chillAndFreezeVisuals.SetActive(true);
                }
            }
            else
            {
                if (chillAndFreezeVisuals.activeInHierarchy)
                {
                    chillAndFreezeVisuals.SetActive(false);
                }
            }

            if (ailmentManager.IgniteStacks.Count > 0)
            {
                if (!igniteVisuals.activeInHierarchy)
                {
                    igniteVisuals.SetActive(true);
                }
            }
            else
            {
                if (igniteVisuals.activeInHierarchy)
                {
                    igniteVisuals.SetActive(false);
                }
            }


            if (ailmentManager.Shock != null)
            {
                if (!shockVisuals.activeInHierarchy)
                {
                    shockVisuals.SetActive(true);
                }
            }
            else
            {
                if (shockVisuals.activeInHierarchy)
                {
                    shockVisuals.SetActive(false);
                }
            }

        }


        #endregion

    }
}
