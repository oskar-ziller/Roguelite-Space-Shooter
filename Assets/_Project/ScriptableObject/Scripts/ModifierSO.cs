using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MeteorGame
{
    [CreateAssetMenu(fileName = "New Modifier", menuName = "ScriptableObjects/Modifier")]
    public class ModifierSO : ScriptableObject
    {
        public string internalName;
        public bool hasValues = true;

        [TextArea]
        public string description;


        [Tooltip("Uncheck to disable from loading")]
        public bool isEnabled = true;
    }
}
