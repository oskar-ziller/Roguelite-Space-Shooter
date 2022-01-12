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
        
        }

        private void Update()
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

        #endregion

        #region Methods

        #endregion

    }
}
