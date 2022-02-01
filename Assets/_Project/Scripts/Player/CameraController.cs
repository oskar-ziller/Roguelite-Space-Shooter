using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace MeteorGame
{
    public class CameraController : MonoBehaviour
    {
        #region Variables

        [SerializeField, Range(50f, 400f)]
        private float sens = 100f;

        [SerializeField, Range(0f, 90f)]
        private float verticalLookLimit = 90f;

        private PlayerController player;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            player = FindObjectOfType<PlayerController>();
        }

        void Start()
        {
        }

        void Update()
        {
            
        }

        private void FixedUpdate()
        {
            float xAxis = Input.GetAxisRaw("Mouse X");
            float yAxis = Input.GetAxisRaw("Mouse Y");

            float mouseX = xAxis * sens * Time.deltaTime;
            float mouseY = yAxis * sens * Time.deltaTime;

            transform.Rotate(Vector3.right, -mouseY);
            player.TurnHorizontal(mouseX); // rotate player body
        }

        #endregion

        #region Methods

        #endregion
    }
}


