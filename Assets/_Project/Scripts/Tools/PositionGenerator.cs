using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{

    public class PositionGenerator
    {
        public int regionSize;
        public List<MySphere> spheres;
        //Vector3 validRandomPos;
        int insideWhile = 0;
        int totalFramesWaited = 0;

        List<float> spawnList;

        float maxRegionSize = 40;
        float spacing = 0f;
        int iterationsBeforeWait = 40;

        PackShape shape = PackShape.Sphere;

        public PositionGenerator()
        {
            regionSize = 0;
            spheres = new List<MySphere>();
        }

        public PositionGenerator(PackShape shape, float maxRegionSize, float spacing, List<float> spawnList, int iterationsBeforeWait)
        {
            regionSize = 0;
            spheres = new List<MySphere>();

            this.maxRegionSize = maxRegionSize;
            this.spacing = spacing;
            this.spawnList = new List<float>(spawnList);
            this.iterationsBeforeWait = iterationsBeforeWait;
            this.shape = shape;
        }


        Vector3 FindEmptyRadiusInRegion(float sphereScale, float region)
        {
            if (regionSize < sphereScale)
            {
                return Vector3.zero;
            }


            int limit = 200;

            for (int i = 0; i < limit; i++)
            {
                Vector3 validRandomPos = Vector3.zero;

                if (shape == PackShape.Sphere)
                {
                    validRandomPos = UnityEngine.Random.insideUnitSphere * (regionSize/2 - sphereScale/2);
                }
                else if (shape == PackShape.Cube)
                {
                    var a = -regionSize / 2 + sphereScale/2;
                    var b = regionSize / 2 - sphereScale/2;
                    validRandomPos = new Vector3(UnityEngine.Random.Range(a,b)
                        , UnityEngine.Random.Range(a, b)
                        , UnityEngine.Random.Range(a, b));
                }

                if (IsInRegion(validRandomPos) && !OverlapsWithExisting(validRandomPos, sphereScale))
                {
                    return validRandomPos;
                }
            }

            return Vector3.zero;
        }

        private bool IsInRegion(Vector3 validRandomPos)
        {
            return true;
        }

        bool OverlapsWithExisting(Vector3 pos, float radi)
        {
            foreach (var ex in spheres)
            {
                var dst = Vector3.Distance(pos, ex.center);
                var smaller = ex.radi < radi ? ex.radi : radi;
                var bigger = ex.radi > radi ? ex.radi : radi;

                if (dst < radi / 2 + ex.radi / 2 + (smaller) * spacing + 0.1f)
                {
                    return true;
                }
            }

            return false;
        }



        public void Generate()
        {
            FitSpheresInRegion();
        }


        void FitSpheresInRegion()
        {
            bool maxReached = false;

            for (int i = spawnList.Count - 1; i >= 0; i--)
            {
                float itemRadi = spawnList[i];
                Vector3 pos = Vector3.zero;

                if (spheres.Count == 0)
                {
                    pos = Vector3.zero;
                }
                else
                {
                    pos = FindEmptyRadiusInRegion(sphereScale: itemRadi , region: regionSize);
                }


                while (pos == Vector3.zero)
                {
                    regionSize++;
                    //Debug.Log($"couldnt add {itemRadi} - increasing regionsize to {regionSize}");

                    if (regionSize >= maxRegionSize)
                    {
                        Debug.Log("regionsize limit reached");
                        throw new System.Exception("region size limit");
                    }

                    pos = FindEmptyRadiusInRegion(sphereScale: itemRadi, region: regionSize);
                }

                var toadd = new MySphere(itemRadi, pos);
                spheres.Add(toadd);
                spawnList.RemoveAt(i);
                //Debug.Log($"added {toadd.radi}");

            }
        }


    }
}
