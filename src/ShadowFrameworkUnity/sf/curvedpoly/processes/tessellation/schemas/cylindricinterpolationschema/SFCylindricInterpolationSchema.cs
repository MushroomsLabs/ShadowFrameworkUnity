using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLab.ShadowFramework.Interpolation.CylindricInterpolation;

namespace MLab.ShadowFramework.Interpolation
{
    public class SFCylindricInterpolationSchema
    {
        public const int CYLINDRIC_INTERPOLATION_SCHEMA_ID = 50;

        public static SFInterpolationSchema BuildSchema()
        {
            SFInterpolationSchema schema = new SFInterpolationSchema();

            schema.interpolators = new ICPNetInterpolator[SFInterpolationSchema.TESSELLATION_PROCESS_NET_INTERPOLATORS];
            schema.interpolators[0] = null;
            schema.interpolators[1] = null;
            schema.interpolators[2] = null;
            if (schema.interpolators[3] == null)
                schema.interpolators[3] = new SFCylindricInterpolationTriangleInterpolator();
            if (schema.interpolators[4] == null)
                schema.interpolators[4] = new SFCylindricInterpolationQuadInterpolator();
            for (int i = 5; i < SFInterpolationSchema.TESSELLATION_PROCESS_NET_INTERPOLATORS; i++)
            {
                if (schema.interpolators[i] == null)
                    schema.interpolators[i] = new NGonInterpolation(i);

            }
            return schema;
        }
    }
}
