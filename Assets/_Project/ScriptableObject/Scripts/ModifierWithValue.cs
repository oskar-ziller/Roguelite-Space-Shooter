namespace MeteorGame
{
    [System.Serializable]
    public class ModifierWithValue
    {
        public Modifier modifier;
        public int min;
        public int max;

        public string GetDescription()
        {
            if (min == max)
            {
                return modifier.description.Replace("XXX", $"{min}");
            }

            return modifier.description.Replace("XXX", $"({min} - {max})");
        }
    }
}