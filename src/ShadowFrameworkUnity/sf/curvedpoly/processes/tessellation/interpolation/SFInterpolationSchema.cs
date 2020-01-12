using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{
    public struct SFInterpolationSchema
    {
        public const int MAX_VERTICES_SIZE = 65000;

        public const int TESSELLATION_PROCESS_NET_INTERPOLATORS = 10;

        public ICPNetInterpolator[] interpolators;
    }
}
