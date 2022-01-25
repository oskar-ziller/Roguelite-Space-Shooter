using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class GoldCoinDrop : MonoBehaviour
    {

        #region Variables

        [Tooltip("Non-kinematic body we use to move the whole object")]
        [SerializeField] private Rigidbody rb;

        public GameObject parent;
        public float gravity = 3f;
        public float explosionSpeedMin = 3f;
        public float explosionSpeedMax = 16f;
        public int amount = 30;

        #endregion

        #region Unity Methods

        private void Start()
        {
            var rand = Random.Range(-20, 20);
            Vector3 customAxis = Quaternion.AngleAxis(rand, transform.forward) * transform.up;

            rand = Random.Range(-20, 20);
            Vector3 customAxis2 = Quaternion.AngleAxis(rand, transform.right) * customAxis;
            
            var randSpeed = Random.Range(explosionSpeedMin, explosionSpeedMax);
            rb.AddRelativeForce(customAxis2 * randSpeed, ForceMode.Impulse);
        }

        private void Update()
        {

        }

        private void FixedUpdate()
        {
            rb.velocity += Vector3.down * Time.deltaTime * gravity;
        }

        private void OnTriggerEnter(Collider other)
        {
            var p = other.gameObject.GetComponentInParent<Player>();

            if (p != null)
            {
                CollidedWithPlayer();
            }

        }

        private void CollidedWithPlayer()
        {
            Player.Instance.CollidedWithDroppedGold(this);
            Destroy(parent);
        }


        #endregion

        #region Methods

        #endregion

    }
}
