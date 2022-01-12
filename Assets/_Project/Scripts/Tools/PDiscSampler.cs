using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class PDiscSampler
{
    //float rMin, rMax;

    List<float> spawnSizeList;
    int totalToSpawn;

    int k;
    Vector3 region;
    float cellSize;
    Obj[,,] grid;

    List<Obj> activeSamples = new List<Obj>();
    List<Obj> legalPoints = new List<Obj>();

    int totalTries = 0;
    private float padding; // min dist between shells of two enemies

    public PDiscSampler(int k, List<float> sizes, Vector3 region, float padding)
    {
        //this.rMin = rmin;
        //this.rMax = rmax;
        spawnSizeList = sizes;
        this.k = k;
        this.region = region;
        this.padding = padding;
        cellSize = 1 / Mathf.Sqrt(3);

        totalToSpawn = spawnSizeList.Count;

        grid = new Obj[Mathf.CeilToInt(region.x / cellSize), Mathf.CeilToInt(region.y / cellSize), Mathf.CeilToInt(region.z / cellSize)];
    }


    // new PDiscSampler(1, 30, 


    /// Adds the sample to the active samples queue and the grid before returning it
    private void AddSample(Obj sample)
    {
        activeSamples.Add(sample);
        GridPos pos = new GridPos(sample.position, cellSize);

        pos.x = (int)Mathf.Max(pos.x, 0);
        pos.y = (int)Mathf.Max(pos.y, 0);
        pos.z = (int)Mathf.Max(pos.z, 0);

        pos.x = (int)Mathf.Min(pos.x, grid.GetLength(0) - 1);
        pos.y = (int)Mathf.Min(pos.y, grid.GetLength(0) - 1);
        pos.z = (int)Mathf.Min(pos.z, grid.GetLength(0) - 1);

        grid[pos.x, pos.y, pos.z] = sample;
    }


    /// Helper struct to calculate the x and y indices of a sample in the grid
    private struct GridPos
    {
        public int x;
        public int y;
        public int z;

        public GridPos(Vector3 sample, float cellSize)
        {
            x = (int)Mathf.CeilToInt(sample.x / cellSize);
            y = (int)Mathf.CeilToInt(sample.y / cellSize);
            z = (int)Mathf.CeilToInt(sample.z / cellSize);
        }
    }


    public class Obj
    {
        public Vector3 position;
        public float radius;

        public Obj(Vector3 pos, float r)
        {
            position = pos;
            radius = r;
        }
    }



    public List<Obj> FindPoints()
    {
        /*
         * The algorithm takes as input
         * the extent of the sample domain in Rn
         * the minimum distance r between samples
         * and a constant k.
         * */

        /*
         * Step 0. Initialize an n-dimensional background grid for storing samples and accelerating spatial searches.
         * We pick the cell size to be bounded by r/√n, so that each grid cell will contain at most one sample,
         * and thus the grid can be implemented as a simple n-dimensional array of integers:
         * the default −1 indicates no sample, a non-negative integer gives the index of the sample located in a cell.
         * */



        /*
         * Step 1. Select the initial sample, x0, randomly chosen uniformly from the domain.
         * Insert it into the background grid
         * and initialize the “active list” (an array of sample indices) with this index(zero).
         * */

        float randomSizeFromDict = spawnSizeList[0];
        Vector3 x0 = Vector3.one * randomSizeFromDict / 2f;

        AddSample(new Obj(x0, randomSizeFromDict));

        /*
         * Step 2. While the active list is not empty, choose a random index from it (say i).
         * Generate up to k points chosen uniformly from the spherical annulus between radius r and 2r around xi.
         * 
         * 
         * For each point in turn, check if it is within distance r of existing samples
         * (using the background grid to only test nearby samples).
         * 
         * If a point is adequately far from existing samples, emit it as the next sample
         * and add it to the active list.
         * 
         * If after k attempts no such point is found, instead remove i from the active list.
         * 
         * */


        while (activeSamples.Count > 0)
        {
            // Pick a random active sample
            int i = Random.Range(0, activeSamples.Count);

            Obj sample = activeSamples[i];

            // Try `k` random candidates between [radius, 2 * radius] from that sample.
            bool found = false;

            for (int j = 0; j < k; ++j)
            {
                totalTries++;

                ////float angle = 2 * Mathf.PI * Random.value;
                ////float r = Mathf.Sqrt(Random.value * 3 * radius2 + radius2); // See: http://stackoverflow.com/questions/9048095/create-random-number-within-an-annulus/9048443#9048443
                ////Vector2 candidate = sample + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float randomR = spawnSizeList[0];
                //float randomR = spawnSizeList[Mathf.Min(spawnSizeList.Count - 1, i + 1)];

                float minRadi = (sample.radius + randomR + padding + 0.1f);
                Vector3 randomPos = sample.position + Random.onUnitSphere * Random.Range(minRadi, minRadi*2);

                Obj candidate = new Obj(randomPos, randomR);

                // Accept candidates if it's inside the rect and farther than 2 * radius to any existing sample.
                if (IsInDomain(candidate) && IsFarEnough(candidate))
                {
                    found = true;
                    AddSample(candidate);
                    legalPoints.Add(candidate);
                    spawnSizeList.Remove(randomR);
                    break;
                }

            }


            // If we couldn't find a valid candidate after k attempts, remove this sample from the active samples queue
            if (!found)
            {
                activeSamples.Remove(sample);
                //activeSamples[i] = activeSamples[activeSamples.Count - 1];
                //activeSamples.RemoveAt(activeSamples.Count - 1);
            }


            if (spawnSizeList.Count == 0)
            {
                break;
            }

            if (legalPoints.Count > totalToSpawn)
            {
                Debug.Log("legalPoints.Count > totalToSpawn");
                break;
            }
        }

        Debug.Log($"PDisc took {totalTries} tries. - regionsize: {region}");
        return legalPoints;
    }




    private bool IsInDomain(Obj o)
    {
        Vector3 pos = o.position;

        return pos.x >= o.radius / 2f && pos.x < region.x - o.radius / 2f
            && pos.y >= o.radius / 2f && pos.y < region.y - o.radius / 2f
            && pos.z >= o.radius / 2f && pos.z < region.z - o.radius / 2f;
    }




    private bool IsFarEnough(Obj candidate)
    {
        float candidateRadius = candidate.radius;
        GridPos pos = new GridPos(candidate.position, cellSize);

        float ratio = candidateRadius / 1; // can have smaller than 1 as rMin
        int searchSize = Mathf.CeilToInt(2f * ratio);

        int xmin = (int)Mathf.Max(Mathf.Floor(pos.x - searchSize), 0);
        int ymin = (int)Mathf.Max(Mathf.Floor(pos.y - searchSize), 0);
        int zmin = (int)Mathf.Max(Mathf.Floor(pos.z - searchSize), 0);
        int xmax = (int)Mathf.Min(Mathf.Ceil(pos.x + searchSize), grid.GetLength(0) - 1);
        int ymax = (int)Mathf.Min(Mathf.Ceil(pos.y + searchSize), grid.GetLength(1) - 1);
        int zmax = (int)Mathf.Min(Mathf.Ceil(pos.z + searchSize), grid.GetLength(2) - 1);

        for (int z = zmin; z <= zmax; z++)
        {
            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    Obj existing = grid[x, y, z];

                    if (existing != null)
                    {
                        float d = Vector3.Distance(candidate.position, existing.position);

                        if (d < candidate.radius + existing.radius + padding)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;

        // Note: we use the zero vector to denote an unfilled cell in the grid. This means that if we were
        // to randomly pick (0, 0) as a sample, it would be ignored for the purposes of proximity-testing
        // and we might end up with another sample too close from (0, 0). This is a very minor issue.
    }













}
