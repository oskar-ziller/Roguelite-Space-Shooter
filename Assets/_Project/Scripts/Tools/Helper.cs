using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MeteorGame
{
    public static class Helper
    {
        public static float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return ((value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow)) + toLow;
        }

        public static Vector3 Map(float value, float fromLow, float fromHigh, Vector3 toLow, Vector3 toHigh)
        {
            var x = Map(value, fromLow, fromHigh, toLow.x, toHigh.x);
            var y = Map(value, fromLow, fromHigh, toLow.y, toHigh.z);
            var z = Map(value, fromLow, fromHigh, toLow.y, toHigh.z);

            return new Vector3(x, y, z);
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

        public static int ValueAtLevel(this ModifierWithValue m, int level)
        {
            return (int)Map(level, 0, 20, m.min, m.max);
        }
    }
}
