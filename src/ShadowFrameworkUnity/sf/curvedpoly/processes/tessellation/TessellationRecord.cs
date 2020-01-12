using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Processes
{
    public class TessellationRecord
    {
        public TessellationOutput tessellationOutput = null;
        public OutputMesh outputMesh = null;
        public bool used=false;

        public void Free() {
            used = false;
            tessellationOutput = null;
            outputMesh = null;
        }
    }
}
