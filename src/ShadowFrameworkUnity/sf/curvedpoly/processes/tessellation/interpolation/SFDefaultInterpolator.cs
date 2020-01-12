using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLab.ShadowFramework.Interpolation
{
    public class SFDefaultInterpolationManager : SFInterpolationSchemaManager
    {
        public SFDefaultInterpolationManager(){

            this.RegisterSchema(0, SFCylindricInterpolationSchema.BuildSchema());
            this.RegisterSchema(SFCylindricInterpolationSchema.CYLINDRIC_INTERPOLATION_SCHEMA_ID,
                SFCylindricInterpolationSchema.BuildSchema());
            this.RegisterSchema(SFGouraudSchemaBuilder.GOURAUD_SCHEMA_ID,
                SFGouraudSchemaBuilder.BuildSchema());
            this.RegisterSchema(SFEdgeSurfaceSchemaBuilder.EDGE_SURFACE_SCHEMA_ID,
                SFEdgeSurfaceSchemaBuilder.BuildSchema());
        }
    }
}
