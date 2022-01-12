using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{


    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Transform parentObj;

        private Transform cam;
        private RectTransform canvas;


        [SerializeField] private Vector3 above;
        [SerializeField] private Vector3 below;

        private float height;

        private bool isSetup = false;

        #region Unity Methods

        private void Awake()
        {
            //gameObject.SetActive(false);
        }

        private void Start()
        {
            cam = Camera.main.transform;
            canvas = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            var playerPos = Player.Instance.transform.position;

            var location = below;

            if (playerPos.y > parentObj.position.y)
            {
                location = above;
            }

            //rect.localPosition = location;
            //canvas.rotation = Quaternion.identity;
            //canvas.Translate(location);

            canvas.localPosition = location;
            canvas.LookAt(playerPos);


            //transform.rotation = Quaternion.identity;
            //transform.rotation = Quaternion.FromToRotation(transform.forward, cam.forward);
            //transform.forward = -cam.forward;
            //rect.Translate(new Vector3(0, 0.5f, 0));
        }

        #endregion

    }
}
