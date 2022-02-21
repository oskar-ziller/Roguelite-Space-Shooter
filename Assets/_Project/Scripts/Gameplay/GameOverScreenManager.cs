using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class GameOverScreenManager : MonoBehaviour
    {

        #region Variables

        [SerializeField] private Canvas gameOverCanvas;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            GameManager.Instance.GameOver += OnGameOver;
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods

        private void OnGameOver(Enemy _)
        {
            gameOverCanvas.gameObject.SetActive(true);
            GameManager.Instance.SetCursorMode(CursorLockMode.Confined);
        }

        public void OnRetryClicked()
        {
            gameOverCanvas.gameObject.SetActive(false);
            GameManager.Instance.SetCursorMode(CursorLockMode.Locked);
            GameManager.Instance.RestartGame();
        }

        #endregion

    }
}
