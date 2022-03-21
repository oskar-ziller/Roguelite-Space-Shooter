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

        // only one spawner has this true
        // 1st wave always spawns on this
        public bool InitialSpawner = false;

        private readonly int animEndTrigger = Animator.StringToHash("EndSpawnAnim");
        private readonly int animStartTrigger = Animator.StringToHash("StartSpawnAnim");

        // duration to wait after spawn to let enemies spawn from this spawner
        // so packs don't spawn inside each other
        private readonly float waitAfterSpawnDuration = 15f;


        private List<LineRenderer> lineRenderers;
        private Animator animator;
        private bool isSpawning = false;

        public Vector3 PackPos => transform.position;
        public bool CanSpawn => !isSpawning;
        
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

        public void StartSpawnAnim()
        {
            if (isSpawning)
            {
                return;
            }

            isSpawning = true;
            animator.SetTrigger(animStartTrigger);
        }

        public void EndSpawnAnim()
        {
            animator.SetTrigger(animEndTrigger);
            StartCoroutine(SetIsSpawningWithDelay());
        }


        private IEnumerator SetIsSpawningWithDelay()
        {
            yield return new WaitForSeconds(waitAfterSpawnDuration);
            isSpawning = false;
        }

        #endregion

    }
}
