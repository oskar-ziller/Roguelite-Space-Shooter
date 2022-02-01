using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class GoldCoinDrop : MonoBehaviour
    {

        #region Variables

        [Tooltip("Amount to give to player once picked up")]
        public int goldAmount = 30;

        [Header("Explosion")]
        [Tooltip("Objects get Random(-randomAngleRange, randomAngleRange) angle at start")]
        public float randomAngleRange = 20f;
        public float gravity = 30f;
        public float explosionSpeedMin = 3f;
        public float explosionSpeedMax = 16f;

        private SphereCollider triggerCollider;
        private AudioSource audioSource;
        private CapsuleCollider worldCollider;
        private MeshRenderer meshRenderer;
        private Light lightObj;
        private Rigidbody parentRB;
        private bool inMagnet = false;


        #endregion

        #region Unity Methods

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            triggerCollider = GetComponent<SphereCollider>();
            worldCollider = GetComponentInChildren<CapsuleCollider>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            lightObj = GetComponent<Light>();
            parentRB = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            var rand = Random.Range(-randomAngleRange, randomAngleRange);
            Vector3 customAxis = Quaternion.AngleAxis(rand, transform.forward) * transform.up;

            rand = Random.Range(-randomAngleRange, randomAngleRange);
            Vector3 customAxis2 = Quaternion.AngleAxis(rand, transform.right) * customAxis;
            
            var randSpeed = Random.Range(explosionSpeedMin, explosionSpeedMax);
            parentRB.AddRelativeForce(customAxis2 * randSpeed, ForceMode.Impulse);
        }


        private void FixedUpdate()
        {
            parentRB.velocity += Vector3.down * Time.deltaTime * gravity;

            if (inMagnet)
            {
                var dir = (Player.Instance.transform.position - parentRB.position).normalized;
                parentRB.velocity += dir * Time.deltaTime * 250f;
            }

        }


        private void OnTriggerEnter(Collider other)
        {
            var magnet = other.gameObject.GetComponent<GoldMagnet>();

            if (magnet == null)
            {
                var p = other.gameObject.GetComponentInParent<Player>();

                if (p != null)
                {
                    CollidedWithPlayer();
                }
            }
            else
            {
                inMagnet = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var magnet = other.gameObject.GetComponent<GoldMagnet>();

            if (magnet != null)
            {
                inMagnet = false;
            }
        }


        private void MoveTowardsPlayer()
        {

        }



        private void HideBody()
        {
            triggerCollider.enabled = false;
            worldCollider.enabled = false;
            meshRenderer.enabled = false;
            lightObj.enabled = false;
        }

        private void CollidedWithPlayer()
        {
            Player.Instance.CollidedWithDroppedGold(this);
            HideBody();
        }

        public void PlayAuidoWithPitch(float pitch)
        {
            audioSource.pitch = pitch;
            audioSource.Play();
            Destroy(gameObject, audioSource.clip.length);
        }


        #endregion

        #region Methods

        #endregion

    }
}
