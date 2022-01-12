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

        [Header("Spell Data")]
        public AllModifiers AllModifiers;
        public AllGems AllGems;
        public AllSpells AllSpells;


        [Header("Difficulty Settings")]
        
        [Tooltip("How many minutes should gamelevel take to reac max")]
        public float minutesToHitMaxGameLevel = 10f;

        [Tooltip("How many levels to reach a level-checkpoint")]
        public float checkpointLevelInterval = 5;

        [Tooltip("Curve of Min-Max level in Min-Max minutes")]
        [SerializeField] private AnimationCurve difficultyCurve;

        [SerializeField] private float maxGameLevel = 100;



        [Header("Misc Settings")]

        [Tooltip("Max chain range for projectiles")]
        public float chainRange = 10f;

        [Tooltip("Max links allowed per spell slot")]
        public int MaxLinks = 7;




        [Header("References")]

        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private TabMenuManager tabMenuManager;
        [SerializeField] private Canvas tabMenuCanvas;

        public float MaxGameLevel => maxGameLevel;
        public AnimationCurve DifficultyCurve => difficultyCurve;
        public float GameLevel => gameLevel;
        public TabMenuManager TabMenuManager => tabMenuManager;
        public bool IsGamePaused { get; private set; }
        public bool waitingForChallenge { get; private set; }

        private Stopwatch gameStartSW = Stopwatch.StartNew();
        private float lastChallengeLevelCompleted = -1;
        private TimeSpan debugElapsed;
        private float debugGameLevel = 0;
        private float gameLevel = 0; // derived from minutes since start and difficultyCurve
        private bool tabMenuShowing = false;

        #endregion

        #region Unity Methods


        public void PauseGame()
        {
            gameStartSW.Stop();
            Time.timeScale = 0;
            IsGamePaused = true;
        }

        public void ResumeGame()
        {
            gameStartSW.Start();
            Time.timeScale = 1;
            IsGamePaused = false;
        }

        public void StartChallenge()
        {
            return;

            if (!waitingForChallenge)
            {
                gameStartSW.Stop();
                waitingForChallenge = true;
            }
        }

        public void StopChallenge()
        {
            if (waitingForChallenge)
            {
                gameStartSW.Start();
                waitingForChallenge = false;
                lastChallengeLevelCompleted = gameLevel;
            }
        }

        public Modifier GetModifierSO(string s)
        {
            return AllModifiers.Get(s);
        }

        public SpellSO GetSpellSO(string s)
        {
            return AllSpells.Get(s);
        }


        /*



        timeFactor = 0.0506 * difficultyValue * 1^0.2
        stageFactor = 1.15^{stagesCompleted}}
        coeff = (1 + timeInMinutes * timeFactor) * stageFactor

        difficultyValue is equal to 1 for Drizzle, 2 for Rainstorm, and 3 for Monsoon.


        enemyLevel = 1 + (coeff-1) / 0.33

        moneyCost = baseCost * coeff^1.25




        Whenever a monster spawns, its reward is directly multiplied by coeff :

        enemyXPReward = coeff * monsterValue * rewardMultiplier

        enemyGoldReward = 2 * coeff * monsterValue * rewardMultiplier


        Credits per second = 0.75 * (1 + 0.4 * coeff) * 1 / 2

        */


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            gameStartSW.Restart();

            enemySpawner.BeginSpawning();
            Cursor.lockState = CursorLockMode.Locked;
            TabMenuManager.RebuildInventoryUI();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SwapPauseResume();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShowHideTabMenu();
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                //debugElapsed += TimeSpan.FromSeconds(15);
                debugGameLevel += 5;
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                debugElapsed += TimeSpan.FromSeconds(120);
            }

            var secondsToMax = minutesToHitMaxGameLevel * 60;
            var val = HowFarIntoDifficulty().TotalSeconds / secondsToMax;
            var rounded = Math.Round(val, 6);
            var secondsPercentage = (float)rounded;
            var eval = difficultyCurve.Evaluate(secondsPercentage);
            var res = Mathf.Floor(eval * maxGameLevel);

            //var minutePercentage = (float)(HowFarIntoDifficulty().TotalSeconds / (minutesToHitMaxGameLevel * 60));
            //var eval = difficultyCurve.Evaluate(minutePercentage);
            gameLevel = res + debugGameLevel;


            bool laterThanLastChallenge = gameLevel > lastChallengeLevelCompleted;
            bool atChallengeLevel = ((gameLevel - 1) % checkpointLevelInterval == 0);

            if (!waitingForChallenge && laterThanLastChallenge && atChallengeLevel && gameLevel > 1)
            {
                StartChallenge();
            }

            if (gameLevel > maxGameLevel)
            {
                gameLevel = maxGameLevel;
            }
        }

        public TimeSpan HowFarIntoDifficulty()
        {
            return gameStartSW.Elapsed + debugElapsed;
        }

        #endregion

        #region Methods


        private void ShowHideTabMenu()
        {
            SwapPauseResume();

            if (tabMenuShowing)
            {
                HideTabMenu();
            }
            else
            {
                ShowTabMenu();
            }
        }


        private void SwapPauseResume()
        {
            if (IsGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }


        private void ShowTabMenu()
        {
            if (tabMenuShowing)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Confined;
            tabMenuCanvas.gameObject.SetActive(true);
            tabMenuShowing = true;
        }

        private void HideTabMenu()
        {
            if (!tabMenuShowing)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            tabMenuCanvas.gameObject.SetActive(false);
            tabMenuShowing = false;
        }



        #endregion

    }
}
