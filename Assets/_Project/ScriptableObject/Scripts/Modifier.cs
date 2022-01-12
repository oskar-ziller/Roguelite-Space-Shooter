using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MeteorGame
{
    [CreateAssetMenu(fileName = "New Modifier", menuName = "ScriptableObjects/Modifier")]
    public class Modifier : ScriptableObject
    {
        public string internalName;

        [TextArea]
        public string description;
    }
}
