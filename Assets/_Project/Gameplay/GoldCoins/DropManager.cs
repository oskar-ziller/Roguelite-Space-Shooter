using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorGame
{
    public enum CoinSize
    {
        NONE,
        Small,
        Medium,
        Big
    }

    public class DropManager
    {

        private class WeightedRandomGenerator
        {
            /// <summary>
            /// Class used to store entries. WeightedRandom class picks 
            /// an entry at random based on weight.
            /// 
            /// We use WeightedRandomEntry class for either "val" or "coinsize"
            /// 
            /// "val" is used to get a random number (how many to spawn)
            /// "coinsize" is used to get a random size
            /// </summary>
            private class WeightedRandomEntry
            {
                public int val = int.MinValue;
                public CoinSize coinSize = CoinSize.NONE;
                public float weight;

                public WeightedRandomEntry(int val, float weight)
                {
                    this.val = val;
                    this.weight = weight;
                }

                public WeightedRandomEntry(CoinSize coinSize, float weight)
                {
                    this.coinSize = coinSize;
                    this.weight = weight;
                }
            }


            private List<WeightedRandomEntry> entries = new List<WeightedRandomEntry>();
            private float totalWeight;


            // Constructer for when we want a random number picked
            public WeightedRandomGenerator(Dictionary<int, float> entryDict)
            {
                foreach (var kvp in entryDict)
                {
                    entries.Add(new WeightedRandomEntry(kvp.Key, kvp.Value));
                }

                totalWeight = entries.Sum(e => e.weight);
            }

            // Constructer for when we want a random size picked
            public WeightedRandomGenerator(Dictionary<CoinSize, float> entryDict)
            {
                foreach (var kvp in entryDict)
                {
                    entries.Add(new WeightedRandomEntry(kvp.Key, kvp.Value));
                }

                totalWeight = entries.Sum(e => e.weight);
            }

            public int PickRandomNumber()
            {
                float randomWeight = UnityEngine.Random.Range(0, totalWeight);
                float currentWeight = 0f;

                foreach (var e in entries)
                {
                    if (e.val == int.MinValue)
                    {
                        throw new Exception("Wrong entries. Val need to be set");
                    }

                    currentWeight += e.weight;

                    if (randomWeight <= currentWeight)
                    {
                        return e.val;
                    }
                }

                return -1;
            }

            public CoinSize PickRandomSize()
            {
                float randomWeight = UnityEngine.Random.Range(0, totalWeight);
                float currentWeight = 0f;

                foreach (var e in entries)
                {
                    if (e.coinSize == CoinSize.NONE)
                    {
                        throw new Exception("Wrong entries. coinSize need to be set");
                    }

                    currentWeight += e.weight;

                    if (randomWeight <= currentWeight)
                    {
                        return e.coinSize;
                    }
                }

                return CoinSize.Small;
            }
        }


        /// <summary>
        /// Create a weighted table to figure out how many coins to spawn for each enemy rarity.
        /// Then for each one, spawn a small, med or big coin - again based on weighted random
        /// values.
        /// </summary>


        ////////////////////////////////////////////
        /// HOW MANY COINS TO SPAWN DICTIONARIES ///
        ////////////////////////////////////////////

        Dictionary<int, float> countDictNormal = new Dictionary<int, float>()
        {
            { 0, 85/100f },
            { 1, 10/100f },
            { 2, 3/100f },
            { 3, 2/100f },
            { 4, 1/100f },
        };

        Dictionary<int, float> countDictMagic = new Dictionary<int, float>()
        {
            { 0, 80/100f },
            { 1, 10/100f },
            { 2, 5/100f },
            { 3, 3/100f },
            { 4, 2/100f },
        };

        Dictionary<int, float> countDictRare = new Dictionary<int, float>()
        {
            { 2, 50/100f },
            { 3, 30/100f },
            { 4, 10/100f },
            { 5, 5/100f },
            { 6, 4/100f },
            { 10, 1/100f },
        };

        Dictionary<int, float> countDictUnique = new Dictionary<int, float>()
        {
            { 4, 40/100f },
            { 5, 30/100f },
            { 6, 20/100f },
            { 7, 5/100f },
            { 8, 4/100f },
            { 15, 1/100f },
        };

        //////////////////////////////
        /// COIN SIZE DICTIONARIES ///
        //////////////////////////////


        Dictionary<CoinSize, float> coinSizeNormal = new Dictionary<CoinSize, float>()
        {
            { CoinSize.Small, 90/100f },
            { CoinSize.Medium, 9/100f },
            { CoinSize.Big, 1/100f },
        };


        Dictionary<CoinSize, float> coinSizeMagic = new Dictionary<CoinSize, float>()
        {
            { CoinSize.Small, 80/100f },
            { CoinSize.Medium, 15/100f },
            { CoinSize.Big, 5/100f },
        };

        Dictionary<CoinSize, float> coinSizeRare = new Dictionary<CoinSize, float>()
        {
            { CoinSize.Small, 70/100f },
            { CoinSize.Medium, 25/100f },
            { CoinSize.Big, 5/100f },
        };

        Dictionary<CoinSize, float> coinSizeUnique = new Dictionary<CoinSize, float>()
        {
            { CoinSize.Small, 50/100f },
            { CoinSize.Medium, 35/100f },
            { CoinSize.Big, 15/100f },
        };


        /// <summary>
        /// Create WeightedRandom objects for each dictionary (each rarity)
        /// One for count and one for coin size.
        /// </summary>

        private WeightedRandomGenerator wrCountNormal, wrSizeNormal;
        private WeightedRandomGenerator wrCountMagic, wrSizeMagic;
        private WeightedRandomGenerator wrCountRare, wrSizeRare;
        private WeightedRandomGenerator wrCountUnique, wrSizeUnique;

        public DropManager()
        {
            // Populate objects
            wrCountNormal = new WeightedRandomGenerator(countDictNormal);
            wrSizeNormal = new WeightedRandomGenerator(coinSizeNormal);

            wrCountMagic = new WeightedRandomGenerator(countDictMagic);
            wrSizeMagic = new WeightedRandomGenerator(coinSizeMagic);

            wrCountRare = new WeightedRandomGenerator(countDictRare);
            wrSizeRare = new WeightedRandomGenerator(coinSizeRare);

            wrCountUnique = new WeightedRandomGenerator(countDictUnique);
            wrSizeUnique = new WeightedRandomGenerator(coinSizeUnique);
        }


        /// <summary>
        /// Picks a random number based on weights
        /// Higher rarities have higher chance to return bigger numbers
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public int PickRandomNumber(Enemy e)
        {
            WeightedRandomGenerator generatorToUse;

            generatorToUse = wrCountNormal;


            //if (e.rarity == EnemyRarity.Normal)
            //{
            //    generatorToUse = wrCountNormal;
            //}
            //else if (e.rarity == EnemyRarity.Magic)
            //{
            //    generatorToUse = wrCountMagic;
            //}
            //else if (e.rarity == EnemyRarity.Rare)
            //{
            //    generatorToUse = wrCountRare;
            //}
            //else if (e.rarity == EnemyRarity.Unique)
            //{
            //    generatorToUse = wrCountUnique;
            //}
            //else
            //{
            //    throw new Exception("Enemy rarity is undefined for generators. Rarity: " + e.rarity);
            //}

            return generatorToUse.PickRandomNumber();
        }



        /// <summary>
        /// Picks a random coin size based on weights.
        /// Higher rarities have higher chance to return bigger coin sizes.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public CoinSize PickRandomCoinSize(Enemy e)
        {
            WeightedRandomGenerator generatorToUse;
            generatorToUse = wrSizeNormal;

            //if (e.rarity == EnemyRarity.Normal)
            //{
            //    generatorToUse = wrSizeNormal;
            //}
            //else if (e.rarity == EnemyRarity.Magic)
            //{
            //    generatorToUse = wrSizeMagic;
            //}
            //else if (e.rarity == EnemyRarity.Rare)
            //{
            //    generatorToUse = wrSizeRare;
            //}
            //else if (e.rarity == EnemyRarity.Unique)
            //{
            //    generatorToUse = wrSizeUnique;
            //}
            //else
            //{
            //    throw new Exception("Enemy rarity is undefined for generators. Rarity: " + e.rarity);
            //}

            return generatorToUse.PickRandomSize();
        }


    }
}
