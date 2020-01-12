using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    public interface IGuideModel
    {
        int GetCurveTessellationSteps(int edgeLength, short[] edge_, short[] edgeHints,
            int[] edgesProfile, int edgeProfileIndex);
        
        void EvaluatePolyline(CurvedPolygonsNet net, OutputMesh mesh, CPNGuide guide);

        void EvaluateEdge(CurvedPolygonsNet net, OutputMesh mesh, CPNGuide guide,
                short edgeLength, short[] edge, int edgeIndex, short[] edgeHints,
                float[] edgeWeights, int[] edgeProfile, int realEdgeIndex);

        void EvaluateNormals(OutputMesh mesh, CPNGuide guide);
    }
}
