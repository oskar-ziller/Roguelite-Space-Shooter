using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{


    [Serializable]
    public class GemItem
    {

        #region Variables

        [SerializeField] private SpellItem spellItem = null;
        [SerializeField] private string name;
        [SerializeField] private int level = 1;
        [SerializeField] private bool isEquipped = false;
        [SerializeField] private List<ModifierWithValue> modifiers;


        public List<ModifierWithValue> Modifiers => modifiers;
        public bool IsEquipped => isEquipped;
        public string Name => name;
        public Color Color => gemColor;

        public bool HasSpell => spellItem != null && spellItem.Name != null;
        public SpellItem Spell => spellItem;
        public string Description => description;
        public int Level => level;
        public int LevelUpCost => (level + 1) * 1000;

        public const int MaxGemLevel = 20;

        private string description;
        private Color gemColor;

        private string statColorBright = "#edf508";
        private string statColorDark = "#969494";
        private string statColorCold = "#58c4f4";
        private string statColorFire = "#ff4d07";
        private string statColorRadiation = "#85fa46";
        private string statColorDoT = "#00ff9c";

        private int smallStatSize = 80;

        private string uiString = "";


        #endregion


        #region Methods

        public GemItem(GemSO gemSO, int level = 0)
        {
            modifiers = gemSO.modifiers;
            name = gemSO.name;
            description = gemSO.description;
            gemColor = gemSO.gemColor;

            this.level = level;

            if (gemSO.spellSO != null)
            {
                spellItem = new SpellItem(gemSO.spellSO, this);
            }
            else
            {
                spellItem = null;
            }
        }

        public int GetModifierValueForCurrentLevel(ModifierSO m)
        {
            var gemMod = modifiers.FirstOrDefault(mod => mod.modifierSO == m);

            if (gemMod == null) // mod doesnt exist
            {
                return 0;
            }

            return gemMod.ValueAtLevel(level);
        }


        public void Equip()
        {
            isEquipped = true;
        }

        public void UnEquip()
        {
            isEquipped = false;
        }

        public void LevelUp()
        {
            level++;
            uiString = "";
        }

        public void SetLevel(int level)
        {
            this.level = level;
            uiString = "";
        }



        public string ColorizeDamageTypes(string input)
        {
            input = input.Replace("cold", $"<b><color={statColorCold}>cold</color></b>");
            input = input.Replace("freeze", $"<b><color={statColorCold}>freeze</color></b>");
            input = input.Replace("chill", $"<b><color={statColorCold}>chill</color></b>");

            input = input.Replace("fire", $"<b><color={statColorFire}>fire</color></b>");
            input = input.Replace("burn", $"<b><color={statColorFire}>burn</color></b>");

            input = input.Replace("radiation", $"<b><color={statColorRadiation}>radiation</color></b>");
            input = input.Replace("weaken", $"<b><color={statColorRadiation}>weaken</color></b>");

            input = input.Replace("damage per second", $"<b><color={statColorDoT}>damage per second</color></b>");


            return input;
        }


        /// <summary>
        /// Using the description gets a colorized string for tooltip UI
        /// </summary>
        /// <returns></returns>
        public string GetStatsStringForUI()
        {
            if (uiString == "")
            {
                var mods = modifiers;

                foreach (ModifierWithValue m in mods)
                {
                    var curr = GetModifierValueForCurrentLevel(m.modifierSO);
                    string modifierDesc = ColorizeDamageTypes(m.modifierSO.description);
                    var percentage = "";

                    if (modifierDesc.Contains("%"))
                    {
                        percentage = "%";
                    }


                    // Example input:
                    // Skills have XXX% increased area of effect

                    int indexOf = modifierDesc.IndexOf("XXX");

                    uiString += "   <line-height=90%>";

                    if (indexOf == -1)
                    {
                        uiString += "- ";
                        uiString += modifierDesc;
                    }
                    else
                    {
                        uiString += "- ";
                        uiString += modifierDesc.Substring(0, indexOf); // Skills have 

                        uiString += $"<b><color={statColorBright}>";
                        uiString += curr;
                        uiString += percentage; // percentage is empty if no '%' character exists in this description
                        uiString += @"</color></b>"; // Skills have 28% 

                        uiString += modifierDesc.Substring(indexOf, modifierDesc.Length - indexOf);
                        // Skills have 28% XXX% increased area of effect

                        if (m.max == m.min)
                        {
                            uiString = uiString.Replace($"XXX%", "");
                            uiString = uiString.Replace($"XXX", "");
                        }
                        else
                        {
                            var replace = $" <color={statColorDark}><size={smallStatSize}%>({m.min}-{m.max})<size=100%></color>";
                            uiString = uiString.Replace("XXX%", replace);
                            uiString = uiString.Replace("XXX", replace);
                            // Skills have 28% (5-52) increased area of effect
                        }
                    }

                    uiString += Environment.NewLine;
                    uiString += Environment.NewLine;
                }
            }

            return uiString;
        }

        #endregion

    }
}
