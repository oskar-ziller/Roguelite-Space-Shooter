using MeteorGame.Enemies;
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


        private const int iterationsBeforeGiveUp = 10;
        private const int iterationsBeforeRegionIncrease = 5;

        private float delayAfterGiveUp = 0.01f;
        private float delayAfterRegionIncrease = 0f;
        private float delayAfterSuccess = 0f;

        private int findPosIterations = 0;
        private int gaveUpCount = 0;
        private int increasedRegionCount = 0;

        private WaitForSeconds delayAfterGiveUpCached;
        private WaitForSeconds delayAfterRegionIncreaseCached;
        private WaitForSeconds delayAfterSuccessCached;

        public PositionGenerator(List<EnemySO> spawnList, PackShape packShape, float spacing)
        {
            regionExtends = 0;
            spawnPositions = new List<FindSpawnPosResult>();

            this.spacing = spacing;
            this.spawnList = new List<EnemySO>(spawnList);
            //regionShape = packShape;


            // order the list so we spawn biggest first
            this.spawnList = this.spawnList.OrderByDescending(e => e.ShapeRadi).ToList();
            // set region size to biggest enemy size to save some time
            regionExtends = Mathf.CeilToInt(this.spawnList.First().ShapeRadi + 2);

            delayAfterGiveUpCached = new WaitForSeconds(delayAfterGiveUp);
            delayAfterRegionIncreaseCached = new WaitForSeconds(delayAfterRegionIncrease);
            delayAfterSuccessCached = new WaitForSeconds(delayAfterSuccess);
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
                if (spawnPositions.Count == 0)
                {
                    spawnPositions.Add(new FindSpawnPosResult(Vector3.zero, e));
                    continue;
                }

                Vector3 pos = Vector3.zero;

                //UnityEngine.Debug.Log($"Trying to find spot for {e.Name} - size: {e.ShapeRadi}");

                while (pos == Vector3.zero)
                {
                    for (int i = 0; i < iterationsBeforeRegionIncrease; i++)
                    {
                        pos = FindSpawnPos(e);

                        if (pos == Vector3.zero)
                        {
                            gaveUpCount++;
                            //UnityEngine.Debug.Log("Gave up on point. Sleeping for: " + delayAfterGiveUp);

                            if (delayAfterGiveUp > 0)
                            {
                                yield return delayAfterGiveUpCached;
                            }
                            else
                            {
                                yield return null;
                            }
                        }
                    }

                    if (pos == Vector3.zero)
                    {
                        regionExtends++;
                        //UnityEngine.Debug.Log($"Still can't fit. Increase region to {regionExtends} and sleep for {delayAfterRegionIncrease} ");
                        increasedRegionCount++;

                        if (delayAfterRegionIncrease > 0)
                        {
                            yield return delayAfterRegionIncreaseCached;
                        }
                        else
                        {
                            yield return null;
                        }

                    }
                }

                spawnPositions.Add(new FindSpawnPosResult(pos, e));


                if (delayAfterSuccess > 0)
                {
                    yield return delayAfterSuccessCached;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private Vector3 GetRandomPos()
        {
            //if (regionShape == PackShape.Sphere || true)
            //{
            //    randomPos = Random.insideUnitSphere * regionExtends;
            //}
            //else if (regionShape == PackShape.Cube)
            //{
            //    var extend = regionExtends;

            //    randomPos = new Vector3(Random.Range(-extend, extend)
            //        , Random.Range(-extend, extend)
            //        , Random.Range(-extend, extend));
            //}

            return Random.insideUnitSphere * regionExtends;
        }


        private Vector3 FindSpawnPos(EnemySO toSpawn)
        {
            for (int i = 0; i < iterationsBeforeGiveUp; i++)
            {
                findPosIterations++;

                var randPos = GetRandomPos();

                if (!OverlapsWithExisting(randPos, toSpawn.ShapeRadi))
                {
                    return randPos;
                }
            }

            return Vector3.zero;
        }

        private bool OverlapsWithExisting(Vector3 randomPos, float toCheckRadi)
        {
            foreach (FindSpawnPosResult existing in spawnPositions)
            {
                var dist = Vector3.Distance(existing.SpawnPos, randomPos);
                bool check = dist < ((existing.EnemySO.ShapeRadi + toCheckRadi)) + spacing;

                if (check)
                {
                    return true; // overlaps
                }
            }

            return false;
        }
    }
}
