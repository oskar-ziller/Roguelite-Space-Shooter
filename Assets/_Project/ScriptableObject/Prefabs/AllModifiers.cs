using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class AllModifiers : MonoBehaviour
    {
        public List<Modifier> All;
       

        public Modifier Get(string s)
        {
            return All.First(m => m.internalName == s);
        }

    }
}