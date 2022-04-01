using MeteorGame.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MeteorGame
{
    public class ScriptableObjectManager
    {
        public Dictionary<string, ModifierSO> Modifiers = new();
        public List<SpellSO> Spells = new();
        public List<GemSO> Gems = new();
        public List<EnemySO> Enemies = new();


        public void Load()
        {
            List<ModifierSO> modifierSOs = Resources.LoadAll<ModifierSO>("ScriptableObjects/Shooting/Modifiers").ToList();
            List<SpellSO> spellSOs = Resources.LoadAll<SpellSO>("ScriptableObjects/Shooting/Spells").ToList();
            List<GemSO> gemSOs = Resources.LoadAll<GemSO>("ScriptableObjects/Shooting/Gems").ToList();
            List<EnemySO> enemySOs = Resources.LoadAll<EnemySO>("ScriptableObjects/Enemy").ToList();

            foreach (var item in modifierSOs)
            {
                if (item.isEnabled)
                {
                    Modifiers.Add(item.internalName, item);
                }
            }

            foreach (var item in spellSOs)
            {
                if (item.isEnabled)
                {
                    Spells.Add(item);
                }
            }

            foreach (var item in gemSOs)
            {
                if (item.isEnabled)
                {
                    Gems.Add(item);
                }
            }

            foreach (var item in enemySOs)
            {
                if (item.IsEnabled)
                {
                    Enemies.Add(item);
                }
            }
        }




    }
}
