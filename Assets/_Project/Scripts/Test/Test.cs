using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class Test : MonoBehaviour
    {

        #region Variables

        public Rigidbody rigidBody;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }


        private void Start()
        {
            DoTest();
        }

        private void OnEnable()
        {
            DoTest();
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods

        private void DoTest()
        {
            List<Vector3> path = new List<Vector3>();

            path.Add(new Vector3(1, 0, 1));
            path.Add(new Vector3(-2, 0, 3));

            rigidBody.DOPath(path.ToArray(), 2f, PathType.CatmullRom, PathMode.Ignore)
                .SetLoops(-1);
        }

        #endregion

    }
}
