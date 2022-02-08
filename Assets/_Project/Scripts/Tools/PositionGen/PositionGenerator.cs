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


        private const int iterationsBeforeRegionExpand = 50;


        PackShape regionShape = PackShape.Sphere;


        public PositionGenerator(List<EnemySO> spawnList, PackShape packShape, float spacing)
        {
            regionExtends = 0;
            spawnPositions = new List<FindSpawnPosResult>();

            this.spacing = spacing;
            this.spawnList = new List<EnemySO>(spawnList);
            regionShape = packShape;
        }





        public IEnumerator Generate()
        {
            Stopwatch sw = Stopwatch.StartNew();
            yield return DoGenerate();
            UnityEngine.Debug.Log("Generate took " + sw.ElapsedMilliseconds + "ms");
        }

        private IEnumerator DoGenerate()
        {
            foreach (EnemySO e in spawnList)
            {
                UnityEngine.Debug.Log("finding spot for enemy");

                FindSpawnPosResult pos = null;

                while (pos == null)
                {
                    pos = FindSpawnPos(e);

                    if (pos == null)
                    {
                        regionExtends++;
                        UnityEngine.Debug.Log("failed - region++: " + regionExtends);
                        yield return new WaitForSeconds(0.02f);
                    }
                }

                UnityEngine.Debug.Log($"Found spot. center: {pos.SpawnPos}");

                spawnPositions.Add(pos);
                yield return null;
            }
        }

        private FindSpawnPosResult FindSpawnPos(EnemySO toSpawn)
        {
            if (toSpawn.ShapeRadi > regionExtends)
            {
                UnityEngine.Debug.Log($"toSpawn.shapeRadi {toSpawn.ShapeRadi} > regionExtends {regionExtends}");
                return null;
            }

            for (int i = 0; i < iterationsBeforeRegionExpand; i++)
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

                if (!OverlapsWithExisting(randomPos, toSpawn.ShapeRadi))
                {
                    return new FindSpawnPosResult(randomPos, toSpawn);
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
