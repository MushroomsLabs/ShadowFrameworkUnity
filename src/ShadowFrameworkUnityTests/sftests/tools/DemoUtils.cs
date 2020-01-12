using UnityEngine;

namespace MLab.ShadowFramework.Tests
{
    public class DemoUtils
    {
        public const float ONE_THIRD = 1.0f / 3.0f;

        public static string IntsToString(int[] ints)
        {
            string msg = "[";
            for (int i = 0; i < ints.Length; i++)
            {
                if (i > 0)
                    msg = msg + ",";
                msg = msg + ints[i];
            }
            return msg;
        }

        public static string Vector3sToString(Vector3[] ints)
        {
            string msg = "[";
            for (int i = 0; i < ints.Length; i++)
            {
                if (i > 0)
                    msg = msg + ",";
                msg = msg + ints[i];
            }
            return msg;
        }

        public static string Vector2ToString(Vector2[] ints)
        {
            string msg = "[";
            for (int i = 0; i < ints.Length; i++)
            {
                if (i > 0)
                    msg = msg + ",";
                msg = msg + ints[i];
            }
            return msg;
        }
    }
}
