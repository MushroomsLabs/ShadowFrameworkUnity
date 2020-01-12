using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{ 

    public interface ICPNetInterpolator  {

        void RetrieveInfos(CPNPolygon buildingPolygonData);

        void UdpdateContent(OutputMesh mesh, CPNPolygon buildingPolygonData, int internalsIndex,
                int facesIndex, bool doUpdateStructure = true);

        int GetComputedInternals();

        int GetComputedTriangles();

        InterpolationMemory GetMemory();

    }

}