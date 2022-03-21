using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace MeteorGame
{
    public class Test : MonoBehaviour
    {

        private void Start()
        {
            var rb = GetComponent<Rigidbody>();

            rb.velocity = new Vector3(0, -4, 0);
        }




    }
}
