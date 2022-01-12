using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class AllSpells : MonoBehaviour
    {
        public List<SpellSO> All;

        internal SpellSO Get(string s)
        {
            return All.First(sp => sp.internalName == s);
        }
    }
}
