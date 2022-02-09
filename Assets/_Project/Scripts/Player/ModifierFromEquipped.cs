using System;

namespace MeteorGame
{
    /// <summary>
    /// Used to store all modifiers that come from gems and spells
    /// </summary>
    [Serializable]
    public class ModifierFromEquipped
    {
        public ModifierSO m;
        public float val = float.MinValue;

        public ModifierFromEquipped(ModifierSO m, float val)
        {
            this.m = m;
            this.val = val;
        }

    }
}
