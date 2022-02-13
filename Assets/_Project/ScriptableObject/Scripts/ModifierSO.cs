using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MeteorGame
{
    [CreateAssetMenu(fileName = "New Modifier", menuName = "ScriptableObjects/Modifier")]
    public class ModifierSO : ScriptableObject
    {
        public string internalName;

        public bool hasNumericalValue = true;

        public bool multiplicative = false;

        public bool isReduction;

        [TextArea]
        public string description;


        [Tooltip("Uncheck to disable from loading")]
        public bool isEnabled = true;
    }
}
