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
        public float maxRegionSize;
        public List<SpawnPos> spawnPositions;
        List<EnemyRarity> spawnList;

        float spacing = 0f;


        private const int iterationsBeforeRegionExpand = 50;


        PackShape regionShape = PackShape.Sphere;

        public PositionGenerator()
        {
            regionExtends = 0;
            spawnPositions = new List<SpawnPos>();
        }

        public PositionGenerator(PackShape shape, float spacing, List<EnemyRarity> spawnList, float maxExtends)
        {
            regionExtends = 0;
            spawnPositions = new List<SpawnPos>();

            this.spacing = spacing;
            this.spawnList = new List<EnemyRarity>(spawnList);
            regionShape = shape;
            maxRegionSize = maxExtends;
        }





        public IEnumerator Generate()
        {
            Stopwatch sw = Stopwatch.StartNew();
            yield return DoGenerate();
            UnityEngine.Debug.Log("Generate took " + sw.ElapsedMilliseconds + "ms");
        }

        private IEnumerator DoGenerate()
        {
            var uniques = spawnList.Where(o => o == EnemyRarity.Unique).ToList();
            var rares = spawnList.Where(o => o == EnemyRarity.Rare).ToList();
            var magics = spawnList.Where(o => o == EnemyRarity.Magic).ToList();
            var normals = spawnList.Where(o => o == EnemyRarity.Normal).ToList();

            foreach (EnemyRarity e in uniques)
            {
                SpawnPos pos = null;

                while (pos == null)
                {
                    pos = FindSpotFor(e);

                    if (pos == null)
                    {
                        regionExtends++;
                        UnityEngine.Debug.Log("regionExtends++");
                        yield return new WaitForSeconds(0.02f);

                        if (regionExtends > maxRegionSize)
                        {
                            throw new System.Exception("regionExtends > maxRegionSize");
                        }
                    }
                }

                spawnPositions.Add(pos);
                yield return null;
            }

            foreach (EnemyRarity e in rares)
            {
                SpawnPos pos = null;

                while (pos == null)
                {
                    pos = FindSpotFor(e);

                    if (pos == null)
                    {
                        regionExtends++;
                        UnityEngine.Debug.Log("regionExtends++");
                        yield return new WaitForSeconds(0.02f);

                        if (regionExtends > maxRegionSize)
                        {
                            throw new System.Exception("regionExtends > maxRegionSize");
                        }
                    }
                }

                spawnPositions.Add(pos);
                yield return null;
            }

            foreach (EnemyRarity e in magics)
            {
                SpawnPos pos = null;

                while (pos == null)
                {
                    pos = FindSpotFor(e);

                    if (pos == null)
                    {
                        regionExtends++;
                        UnityEngine.Debug.Log("regionExtends++");
                        yield return new WaitForSeconds(0.02f);

                        if (regionExtends > maxRegionSize)
                        {
                            throw new System.Exception("regionExtends > maxRegionSize");
                        }
                    }
                }

                spawnPositions.Add(pos);
                yield return null;
            }

            foreach (EnemyRarity e in normals)
            {
                SpawnPos pos = null;

                while (pos == null)
                {
                    pos = FindSpotFor(e);

                    if (pos == null)
                    {
                        regionExtends++;
                        yield return new WaitForSeconds(0.02f);

                        if (regionExtends > maxRegionSize)
                        {
                            throw new System.Exception("regionExtends > maxRegionSize");
                        }
                    }
                }

                spawnPositions.Add(pos);
                yield return null;
            }
        }

        private SpawnPos FindSpotFor(SpawnInfo toSpawn, EnemyRarity rarity)
        {
            if (toSpawn.extends.magnitude > regionExtends || toSpawn.r > regionExtends)
            {
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

                if (IsInRegion(randomPos, toSpawn) && !OverlapsWithExisting(randomPos, toSpawn))
                {
                    return new SpawnPos(randomPos, toSpawn, rarity);
                }
            }

            return null;
        }

        private bool IsInRegion(Vector3 randomPos, SpawnInfo toSpawn)
        {
            if (toSpawn.shape == ColliderShape.Cube)
            {
                if (regionShape == PackShape.Cube)
                {
                    Vector3 max = randomPos + toSpawn.extends;
                    Vector3 min = randomPos - toSpawn.extends;

                    bool check = max.x < regionExtends && max.y < regionExtends && max.z < regionExtends
                        && min.x > -regionExtends && min.y > -regionExtends && min.z > -regionExtends;

                    return check;
                }

                if (regionShape == PackShape.Sphere)
                {
                    bool check = DoesCubeIntersectSphere(randomPos + toSpawn.extends,
                                randomPos + toSpawn.extends + Vector3.one,
                                Vector3.zero,
                                regionExtends);

                    return check;
                }
            }


            if (toSpawn.shape == ColliderShape.Sphere)
            {
                if (regionShape == PackShape.Cube)
                {
                    bool check = DoesCubeIntersectSphere(Vector3.zero - (Vector3.one * regionExtends),
                                Vector3.zero + (Vector3.one * regionExtends),
                                randomPos,
                                toSpawn.r);

                    return check;
                }

                if (regionShape == PackShape.Sphere)
                {
                    return randomPos.sqrMagnitude + toSpawn.r * toSpawn.r < regionExtends * regionExtends;
                }


                //bool check =
                //       randomPos.x < regionExtends - toSpawn.r
                //    && randomPos.y < regionExtends - toSpawn.r
                //    && randomPos.z < regionExtends - toSpawn.r

                //    && randomPos.x > -regionExtends + toSpawn.r
                //    && randomPos.y > -regionExtends + toSpawn.r
                //    && randomPos.z > -regionExtends + toSpawn.r;

                //return check;
            }

            return false;
        }

        bool DoesCubeIntersectSphere(Vector3 p1, Vector3 p2, Vector3 c, float R)
        {
            float Squared(float x)
            {
                return x * x;
            }

            float dist_squared = R * R;
            /* assume C1 and C2 are element-wise sorted, if not, do that now */
            if (c.x < p1.x) dist_squared -= Squared(c.x - p1.x);
            else if (c.x > p2.x) dist_squared -= Squared(c.x - p2.x);
            if (c.y < p1.y) dist_squared -= Squared(c.y - p1.y);
            else if (c.y > p2.y) dist_squared -= Squared(c.y - p2.y);
            if (c.z < p1.z) dist_squared -= Squared(c.z - p1.z);
            else if (c.z > p2.z) dist_squared -= Squared(c.z - p2.z);
            return dist_squared > 0;
        }

        private bool OverlapsWithExisting(Vector3 randomPos, SpawnInfo toCheck)
        {
            foreach (SpawnPos existing in spawnPositions)
            {
                // Cube vs Sphere
                if (existing.spawnInfo.shape == ColliderShape.Cube)
                {
                    if (toCheck.shape == ColliderShape.Sphere)
                    {
                        bool check = DoesCubeIntersectSphere(existing.center - existing.spawnInfo.extends,
                            existing.center + existing.spawnInfo.extends,
                            randomPos,
                            toCheck.r);

                        if (check)
                        {
                            return true;
                        }
                    }
                }

                // Sphere vs Cube
                if (existing.spawnInfo.shape == ColliderShape.Sphere)
                {
                    if (toCheck.shape == ColliderShape.Cube)
                    {
                        bool check = DoesCubeIntersectSphere(randomPos - toCheck.extends,
                            randomPos + toCheck.extends,
                            existing.center,
                            existing.spawnInfo.r);

                        if (check)
                        {
                            return true;
                        }
                    }
                }

                // Sphere vs Sphere
                if (existing.spawnInfo.shape == ColliderShape.Sphere)
                {
                    if (toCheck.shape == ColliderShape.Sphere)
                    {
                        bool check = (randomPos - existing.center).sqrMagnitude < (existing.spawnInfo.r * existing.spawnInfo.r) * 2;

                        if (check)
                        {
                            return true;
                        }
                    }
                }

                // Cube vs Cube
                if (existing.spawnInfo.shape == ColliderShape.Cube)
                {
                    if (toCheck.shape == ColliderShape.Cube)
                    {
                        var aStart = existing.center - existing.spawnInfo.extends;
                        var aEnd = existing.center + existing.spawnInfo.extends;

                        var bStart = randomPos - toCheck.extends;
                        var bEnd = randomPos + toCheck.extends;

                        bool check = aStart.x <= bEnd.x && bStart.x <= aEnd.x
                            && aStart.y <= bEnd.y && bStart.y <= aEnd.y
                            && aStart.z <= bEnd.z && bStart.z <= aEnd.z;

                        if (check)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }



        private SpawnPos FindSpotFor(EnemyRarity e)
        {
            return FindSpotFor(EnemyManager.Instance.GetSpawnInfo(e), e);
        }

        

    }
}
