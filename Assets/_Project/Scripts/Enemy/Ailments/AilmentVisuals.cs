using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class AilmentVisuals : MonoBehaviour
    {

        #region Variables

        private Enemy owner;

        [SerializeField]
        private GameObject igniteVisuals;

        [SerializeField]
        private GameObject chillAndFreezeVisuals;

        [SerializeField]
        private GameObject shockVisuals;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            owner = GetComponentInParent<Enemy>();
        }

        private void Start()
        {
            StartCoroutine(CheckAilmentsCoroutine());
        }

        IEnumerator CheckAilmentsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.15f);
                CheckAilmentsAndUpdateVisuals();
            }
        }


        private void CheckAilmentsAndUpdateVisuals()
        {
            if (owner.IsFrozenOrChilled())
            {
                chillAndFreezeVisuals.SetActive(true);
            }
            else
            {
                chillAndFreezeVisuals.SetActive(false);
            }

            if (owner.IsIgnited())
            {
                igniteVisuals.SetActive(true);
            }
            else
            {
                igniteVisuals.SetActive(false);
            }

            if (owner.IsShocked())
            {
                shockVisuals.SetActive(true);
            }
            else
            {
                shockVisuals.SetActive(false);
            }
        }

        private void Update()
        {
            
        }

        #endregion

        #region Methods

        #endregion

    }
}
