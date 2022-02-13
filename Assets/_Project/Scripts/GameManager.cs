using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class GameManager : MonoBehaviour
    {

        public static GameManager Instance { get; private set; }

        #region Variables

        [Header("Difficulty Settings")]
        
        [Tooltip("How many minutes should gamelevel take to reac max")]
        public float minutesToHitMaxGameLevel = 10f;

        [Tooltip("How many levels to reach a level-checkpoint")]
        public float checkpointLevelInterval = 5;

        [Tooltip("Curve of Min-Max level in Min-Max minutes")]
        [SerializeField] private AnimationCurve difficultyCurve;

        [SerializeField] private int maxGameLevel = 100;



        [Header("Misc Settings")]

        [Tooltip("Max chain range for projectiles")]
        public float ChainAndForkRange = 45f;


        [Tooltip("Max aim assist range for projectiles")]
        public float AimAssistRange = 45f;


        [Tooltip("Max links allowed per spell slot")]
        public int MaxLinksAllowed = 7;


        [Header("References")]
        [SerializeField] private TabMenuManager tabMenuManager;

        public float MaxGameLevel => maxGameLevel;
        public AnimationCurve DifficultyCurve => difficultyCurve;
        public int GameLevel => gameLevel;
        public TabMenuManager TabMenuManager => tabMenuManager;
        public bool IsGamePaused { get; private set; }

        public ScriptableObjectManager ScriptableObjects => scriptableObjects;


        //private Stopwatch gameStartSW = Stopwatch.StartNew();

        private float gameLaunchTime;
        private float gamePlayTime;
        private float debugElapsedSeconds;
        private float debugGameLevel = 0;
        private int gameLevel = 1; // derived from minutes since start and difficultyCurve

        //public Stopwatch gamePlaySW = Stopwatch.StartNew();


        private ScriptableObjectManager scriptableObjects = new ScriptableObjectManager();




        #endregion

        #region Unity Methods

        private void KeepGameTime()
        {
            gameLaunchTime += Time.deltaTime;

            if (!IsGamePaused)
            {
                gamePlayTime += Time.deltaTime;
            }
        }




        public void PauseGame()
        {
            Time.timeScale = 0;
            IsGamePaused = true;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            IsGamePaused = false;
        }


        private void Awake()
        {
            Instance = this;
            gameLaunchTime = Time.time;
        }

        private void Start()
        {

            // load scriptable objects
            scriptableObjects.Load();
            gamePlayTime = Time.time;

            Cursor.lockState = CursorLockMode.Locked;
            //TabMenuManager.RebuildTabMenu();

            EnemyManager.Instance.Setup();

            EnemyManager.Instance.BeginSpawning();

            tabMenuManager.Setup();


            Player.Instance.Setup();

            SpellCaster.Instance.Setup();

            Player.Instance.DebugAddStuff();
        }


        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShowHideTabMenu();
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                //debugElapsed += TimeSpan.FromSeconds(15);
                debugGameLevel += 10;
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                EnemyManager.Instance.SpawnPack();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                EnemyManager.Instance.DestroyAllEnemies();
            }

            var secondsToMax = minutesToHitMaxGameLevel * 60;
            var val = HowFarIntoDifficulty() / secondsToMax;
            var rounded = Math.Round(val, 6);
            var secondsPercentage = (float)rounded;
            var eval = difficultyCurve.Evaluate(secondsPercentage);
            var res = Mathf.Floor(eval * maxGameLevel) + 1;

            gameLevel = (int)res + (int)debugGameLevel;

            if (gameLevel > maxGameLevel)
            {
                gameLevel = maxGameLevel;
            }






            KeepGameTime();
        }

        public float HowFarIntoDifficulty()
        {
            return gamePlayTime + debugElapsedSeconds;
        }

        #endregion

        #region Methods


        private void ShowHideTabMenu()
        {
            if (tabMenuManager.IsShowing)
            {
                tabMenuManager.Hide();
                ResumeGame();
            }
            else
            {
                tabMenuManager.Show();
                PauseGame();
            }
        }


       



        #endregion

    }
}
