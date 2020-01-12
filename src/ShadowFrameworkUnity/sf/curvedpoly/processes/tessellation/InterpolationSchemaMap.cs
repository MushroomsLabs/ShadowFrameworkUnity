using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Processes
{
    public interface InterpolationSchemaMap
    {
        int GetMappedInterpolatorId(int id);

        IGuideModel GetGuideModel();
    }
}
