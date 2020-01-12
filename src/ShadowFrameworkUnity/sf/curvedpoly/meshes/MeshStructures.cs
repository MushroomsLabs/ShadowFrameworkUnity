using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace MLab.ShadowFramework.Interpolation
{
    class MeshStructures 
    {
        private const float T_INNER_ADVANCEMENT_PRECISION = 0.1f;


        public static int CreateSideTriangles(OutputMesh builder,
            IMeshIndicesArray inside, IMeshIndicesArray outside,
            int intTrIndex)
        {

            /*Vector3[] vs = builder.GetVertices();
            int outerEdgeLength = outside.Count();
            int innerEdgeLength = inside.Count();
            int outerIndex = 0;
            int innerIndex = 0;

            while (innerIndex < innerEdgeLength - 1 || outerIndex < outerEdgeLength - 1)
            {
                if (innerIndex < innerEdgeLength - 1)
                {
                    if (outerIndex < outerEdgeLength - 1)
                    {
                        int id1 = outerIndex;
                        int id2 = innerIndex;
                        int id3 = outside.GetNext();
                        int id4 = inside.GetNext();

                        Vector3 P1 = vs[id1];
                        Vector3 P2 = vs[id2];
                        Vector3 P3 = vs[id3];
                        Vector3 P4 = vs[id4];

                        float dInner = (P4 - P1).sqrMagnitude;
                        float dOuter = (P3 - P2).sqrMagnitude; 

                        if (dInner < dOuter) {
                            intTrIndex = builder.WriteTriangle(intTrIndex, id1, id4, id2);
                            innerIndex++;
                            inside.Move();
                        } else {
                            intTrIndex = builder.WriteTriangle(intTrIndex, id1, id3, id2);
                            outerIndex++;
                            outside.Move();

                        }
                    }
                    else
                    {

                        int id1 = outerIndex;
                        int id2 = innerIndex;
                        int id4 = inside.GetNext();

                        intTrIndex = builder.WriteTriangle(intTrIndex, id1, id4, id2);
                        innerIndex++;
                        inside.Move();
                    }
                }
                else
                {
                    int id1 = outerIndex;
                    int id2 = innerIndex;
                    int id3 = outside.GetNext();
                    intTrIndex = builder.WriteTriangle(
                        intTrIndex, id1, id3, id2);
                    outerIndex++;
                    outside.Move();
                }
            }

            return intTrIndex;*/


            Vector3[] vs = builder.GetVertices();

            int outerEdgeLength = outside.Count();
            int innerEdgeLength = inside.Count();
            int outerIndex = 0;
            int innerIndex = 0; 

            while (innerIndex < innerEdgeLength - 1 || outerIndex < outerEdgeLength - 1)
            {
                if (innerIndex < innerEdgeLength - 1)
                {
                    if (outerIndex < outerEdgeLength - 1)
                    {
                        int id1 = outside.GetIndex();
                        int id2 = inside.GetIndex();
                        int id3 = outside.GetNext();
                        int id4 = inside.GetNext();

                        Vector3 P1 = vs[id1];
                        Vector3 P2 = vs[id2];
                        Vector3 P3 = vs[id3];
                        Vector3 P4 = vs[id4];

                        float dInner = (P4 - P1).sqrMagnitude;
                        float dOuter = (P3 - P2).sqrMagnitude;
                         
                        if (dInner <= dOuter)
                        {
                            intTrIndex = builder.WriteTriangle(intTrIndex, id1, id4, id2);
                            innerIndex++;
                            inside.Move();
                        }
                        else
                        {
                            intTrIndex = builder.WriteTriangle(intTrIndex, id1, id3, id2);
                            outerIndex++;
                            outside.Move();

                        }
                    }
                    else
                    {

                        int id1 = outside.GetIndex();
                        int id2 = inside.GetIndex();
                        int id4 = inside.GetNext();

                        intTrIndex = builder.WriteTriangle(intTrIndex, id1, id4, id2);
                        innerIndex++;
                        inside.Move();
                    }
                }
                else
                {
                    int id1 = outside.GetIndex();
                    int id2 = inside.GetIndex();
                    int id3 = outside.GetNext();
                    intTrIndex = builder.WriteTriangle(
                        intTrIndex, id1, id3, id2);
                    outerIndex++;
                    outside.Move();
                }
            }
            return intTrIndex;
        }

        public static int CreateSideTriangles2(OutputMesh builder,
            IMeshIndicesArray inside, IMeshIndicesArray outside,
            int intTrIndex)
        {   

            Vector3[] vs= builder.GetVertices();
            
            int outerEdgeLength = outside.Count();
            int innerEdgeLength = inside.Count(); 
            int outerIndex = 0;
            int innerIndex = 0;
            float dtInner = innerEdgeLength > 1 ? 1.0f / (innerEdgeLength - 1) : 1;
            float dtOuter = outerEdgeLength > 1 ? 1.0f / (outerEdgeLength - 1) : 1;

            float kIn = 0;
            float kOut = 1;

            if (outerEdgeLength > 1 && innerEdgeLength > 1) { 
                Vector3 A = vs[outside.GetIndex()];
                Vector3 B = vs[outside.GetNext()];
                Vector3 C = vs[inside.GetIndex()];
                Vector3 D = vs[outside.GetAtIndex(outerEdgeLength - 2)];
                Vector3 E = vs[outside.GetAtIndex(outerEdgeLength - 1)];
                Vector3 F = vs[inside.GetAtIndex(innerEdgeLength - 1)];

                kIn = Vector3.Dot(C - A, B - A) / Vector3.Dot(B - A, B - A);
                kOut = Vector3.Dot(F - D, E - D) / Vector3.Dot(E - D, E - D);
                 
                float rect = 1.0f/ (outerEdgeLength - 1);
                kIn = kIn * rect;
                kOut = 1 - (1 - kOut)*rect; 
            }

            while (innerIndex < innerEdgeLength - 1 || outerIndex < outerEdgeLength - 1)
            {
                if (innerIndex < innerEdgeLength - 1)
                {
                    if (outerIndex < outerEdgeLength - 1)
                    {
                        int id1 = outside.GetIndex();
                        int id2 = inside.GetIndex();
                        int id3 = outside.GetNext();
                        int id4 = inside.GetNext();

                        float tOuter = dtOuter * (outerIndex);
                        float tInner = dtInner * (innerIndex) * (kOut - kIn) + kIn;

                        if (tInner <= tOuter)
                        {
                            intTrIndex = builder.WriteTriangle(intTrIndex, id1, id4, id2);
                            innerIndex++;
                            inside.Move();
                        }
                        else
                        {
                            intTrIndex = builder.WriteTriangle(intTrIndex, id1, id3, id2);
                            outerIndex++;
                            outside.Move();

                        }
                    }
                    else
                    {

                        int id1 = outside.GetIndex();
                        int id2 = inside.GetIndex();
                        int id4 = inside.GetNext();

                        intTrIndex = builder.WriteTriangle(intTrIndex, id1, id4, id2);
                        innerIndex++;
                        inside.Move();
                    }
                }
                else
                {
                    int id1 = outside.GetIndex();
                    int id2 = inside.GetIndex();
                    int id3 = outside.GetNext();
                    intTrIndex = builder.WriteTriangle(
                        intTrIndex, id1, id3, id2);
                    outerIndex++;
                    outside.Move();
                }
            }
            return intTrIndex;
        }
    }
}