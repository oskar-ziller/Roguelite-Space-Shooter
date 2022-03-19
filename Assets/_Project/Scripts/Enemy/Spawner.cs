using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace MeteorGame.Enemies
{
    public class Spawner : MonoBehaviour
    {

        #region Variables

        List<LineRenderer> lineRenderers;
        Animator animator;
        readonly int animStartTrigger = Animator.StringToHash("StartSpawnAnim");
        readonly int animEndTrigger = Animator.StringToHash("EndSpawnAnim");

        public Vector3 PackPos => transform.position;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            animator = GetComponent<Animator>();
            lineRenderers = GetComponentsInChildren<LineRenderer>().ToList();
        }

        private void Start()
        {
            EnemyManager.Instance.EnemySpawner.AddSpawner(this);
        }

        private void Update()
        {

        }

        #endregion

        #region Methods

        private void DisableLines()
        {
            foreach (var lr in lineRenderers)
            {
                lr.enabled = false;
            }
        }

        private void EnableLines()
        {
            foreach (var lr in lineRenderers)
            {
                lr.enabled = true;
            }
        }

        [ContextMenu("test")]
        public void StartSpawnAnim()
        {
            animator.SetTrigger(animStartTrigger);
        }

        [ContextMenu("test2")]
        public void EndSpawnAnim()
        {
            animator.SetTrigger(animEndTrigger);
        }

        #endregion

    }
}
