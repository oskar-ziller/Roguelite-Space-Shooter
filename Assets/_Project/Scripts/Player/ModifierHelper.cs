using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorGame
{
    public static class ModifierHelper
    {

        //public static int GetTotal(string s, SpellSlot slot)
        //{
        //    return GetTotal(GameManager.Instance.GetModifierSO(s), slot);
        //}


        //public static int GetTotal(Modifier modifier, SpellSlot slot)
        //{
        //    var total = 0;
        //    total += slot.Spell.GetModifierValueForCurrentLevel(modifier);

        //    var linked = slot.Linked;

        //    foreach (GemItem gem in linked)
        //    {
        //        total += gem.GetModifierValueForCurrentLevel(modifier);
        //    }

        //    return total;
        //}



        //public static bool ModifierExists(Modifier modifier, SpellSlot slot)
        //{
        //    bool activeSpell = slot.Spell.ModifierExists(modifier);
        //    var linked = slot.Linked.Any(g => g.ModifierExists(modifier));

        //    return activeSpell && linked;
        //}







    }
}
