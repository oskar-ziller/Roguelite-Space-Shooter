using MeteorGame.Enemies;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MeteorGame.UI
{
    public class PacksAliveUI : MonoBehaviour
    {

        #region Variables
        [SerializeField] private TextMeshProUGUI aliveEnemies;
        [SerializeField] private TextMeshProUGUI alivePacks;
        #endregion

        #region Unity Methods

        private void Start()
        {
            EnemyManager.Instance.OnEnemyCreatedOrDied += UpdateEnemyUI;
            EnemyManager.Instance.OnPackCreatedOrDied += UpdatePackUI;
        }

        #endregion

        #region Methods

        private void UpdateEnemyUI(Enemy e)
        {
            var enemyCount = EnemyManager.Instance.AliveEnemyCount;
            aliveEnemies.text = string.Format("({0:000})", enemyCount);
        }

        private void UpdatePackUI(EnemyPack ep)
        {
            var packCount = EnemyManager.Instance.AlivePacks.Count;
            alivePacks.text = string.Format("{0:00}", packCount);
        }

        #endregion

    }
}
