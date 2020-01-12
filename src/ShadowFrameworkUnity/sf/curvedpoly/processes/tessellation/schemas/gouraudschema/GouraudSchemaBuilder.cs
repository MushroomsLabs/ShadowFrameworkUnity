using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLab.ShadowFramework.Interpolation.GouraudSchema;

namespace MLab.ShadowFramework.Interpolation
{
    public class SFGouraudSchemaBuilder
    {
        public const int GOURAUD_SCHEMA_ID = 100;

        public static SFInterpolationSchema BuildSchema() {

            SFInterpolationSchema schema = new SFInterpolationSchema();
             
            schema.interpolators = new ICPNetInterpolator[SFInterpolationSchema.TESSELLATION_PROCESS_NET_INTERPOLATORS];
            schema.interpolators[0] = null;
            schema.interpolators[1] = null;
            schema.interpolators[2] = null;
            if (schema.interpolators[3] == null)
                schema.interpolators[3] = new SFGouraudSchemaTriangleInterpolator();
            if (schema.interpolators[4] == null)
                schema.interpolators[4] = new SFGouraudSchemaQuadInterpolator();
            for (int i = 5; i < SFInterpolationSchema.TESSELLATION_PROCESS_NET_INTERPOLATORS; i++)
            {
                if (schema.interpolators[i] == null)
                    schema.interpolators[i] = new NGonInterpolation(i);

            }
            return schema;
        }
    }
}
