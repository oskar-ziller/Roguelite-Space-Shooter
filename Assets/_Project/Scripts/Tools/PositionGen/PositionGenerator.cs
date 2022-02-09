using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{

    public class PositionGenerator
    {
        public int regionExtends;
        public List<FindSpawnPosResult> spawnPositions;
        List<EnemySO> spawnList;

        float spacing = 0f;


        private const int iterationsBeforeGiveUp = 5;
        private const int iterationsBeforeRegionIncrease = 5;

        private float delayAfterGiveUp = 0.01f;
        private float delayAfterRegionIncrease = 0.01f;
        private float delayAfterSuccess = 0.01f;


        PackShape regionShape = PackShape.Sphere;


        private int findPosIterations = 0;
        private int gaveUpCount = 0;
        private int increasedRegionCount = 0;

        public PositionGenerator(List<EnemySO> spawnList, PackShape packShape, float spacing)
        {
            regionExtends = 0;
            spawnPositions = new List<FindSpawnPosResult>();

            this.spacing = spacing;
            this.spawnList = new List<EnemySO>(spawnList);
            regionShape = packShape;


            // order the list so we spawn biggest first
            spawnList = spawnList.OrderByDescending(e => e.ShapeRadi).ToList();
            // set region size to biggest enemy size to save some time
            regionExtends = Mathf.CeilToInt(spawnList.First().ShapeRadi);
        }


        public IEnumerator Generate()
        {
            Stopwatch sw = Stopwatch.StartNew();
            yield return DoGenerate();
            UnityEngine.Debug.Log("Generate took " + sw.ElapsedMilliseconds + "ms");
            UnityEngine.Debug.Log(string.Format("findPosIterations: {0} - gaveUpCount {1} - increasedRegionCount {2}", findPosIterations, gaveUpCount, increasedRegionCount));
        }

        private IEnumerator DoGenerate()
        {
            foreach (EnemySO e in spawnList)
            {
                FindSpawnPosResult pos = null;

                UnityEngine.Debug.Log($"Trying to find spot for {e.name} - size: {e.ShapeRadi}");


                while (pos == null)
                {
                    for (int i = 0; i < iterationsBeforeRegionIncrease; i++)
                    {
                        pos = FindSpawnPos(e);

                        if (pos == null)
                        {
                            gaveUpCount++;
                            UnityEngine.Debug.Log("Gave up on point. Sleeping for: " + delayAfterGiveUp);
                            yield return new WaitForSeconds(delayAfterGiveUp);
                        }
                    }


                    if (pos == null)
                    {
                        regionExtends++;
                        UnityEngine.Debug.Log($"Still can't fit. Increase region to {regionExtends} and sleep for {delayAfterRegionIncrease} ");
                        increasedRegionCount++;
                        yield return new WaitForSeconds(delayAfterRegionIncrease);
                    }
                }

                UnityEngine.Debug.Log($"Found spot. center: {pos.SpawnPos}");

                spawnPositions.Add(pos);
                yield return new WaitForSeconds(delayAfterSuccess);
            }
        }

        private Vector3 GetRandomPos()
        {
            Vector3 randomPos = Vector3.zero;

            if (regionShape == PackShape.Sphere)
            {
                randomPos = Random.insideUnitSphere * regionExtends;
            }
            else if (regionShape == PackShape.Cube)
            {
                var extend = regionExtends;

                randomPos = new Vector3(Random.Range(-extend, extend)
                    , Random.Range(-extend, extend)
                    , Random.Range(-extend, extend));
            }

            return randomPos;
        }


        private FindSpawnPosResult FindSpawnPos(EnemySO toSpawn)
        {
            for (int i = 0; i < iterationsBeforeGiveUp; i++)
            {
                findPosIterations++;
                var randPos = GetRandomPos();

                if (!OverlapsWithExisting(GetRandomPos(), toSpawn.ShapeRadi))
                {
                    return new FindSpawnPosResult(randPos, toSpawn);
                }
            }

            return null;
        }

        private bool OverlapsWithExisting(Vector3 randomPos, float toCheckRadi)
        {
            foreach (FindSpawnPosResult existing in spawnPositions)
            {
                float existingR = existing.EnemySO.ShapeRadi;
                float withSpacing = existingR + toCheckRadi + spacing;
                bool check = (randomPos - existing.SpawnPos).sqrMagnitude < withSpacing * withSpacing;

                if (check)
                {
                    return true; // overlaps
                }
            }

            return false;
        }
    }
}
