using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class AilmentVisuals : MonoBehaviour
    {

        #region Variables

        [SerializeField]
        private GameObject burningVisuals;

        [SerializeField]
        private GameObject chillAndFreezeVisuals;

        [SerializeField]
        private GameObject weakenVisuals;

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

            if (ailmentManager.BurnStacks.Count > 0)
            {
                if (!burningVisuals.activeInHierarchy)
                {
                    burningVisuals.SetActive(true);
                }
            }
            else
            {
                if (burningVisuals.activeInHierarchy)
                {
                    burningVisuals.SetActive(false);
                }
            }


            if (ailmentManager.Weaken != null)
            {
                if (!weakenVisuals.activeInHierarchy)
                {
                    weakenVisuals.SetActive(true);
                }
            }
            else
            {
                if (weakenVisuals.activeInHierarchy)
                {
                    weakenVisuals.SetActive(false);
                }
            }

        }


        #endregion

    }
}
