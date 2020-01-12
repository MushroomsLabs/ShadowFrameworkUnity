using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Processes
{
    public class ProcessesKeeper
    {
        private static CPNTessellationProcess tessellationProcess;

        public static CPNTessellationProcess GetTessellationProcess() {
            if (tessellationProcess == null) {
                tessellationProcess = new CPNTessellationProcess(new SFDefaultInterpolationManager());
            }
            return tessellationProcess;
        }
    }
}
