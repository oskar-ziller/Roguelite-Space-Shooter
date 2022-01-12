using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MeteorGame
{
    [CreateAssetMenu(fileName = "New Gem", menuName = "ScriptableObjects/Gem")]
    public class GemSO : ScriptableObject
    {
        //public List<GemTag> tags;
        public List<ModifierWithValue> modifiers;
        public string name;

        [ColorUsage(false, false)]
        public Color gemColor;

        public SpellSO spellSO;

        [TextArea]
        public string description;
    }

}