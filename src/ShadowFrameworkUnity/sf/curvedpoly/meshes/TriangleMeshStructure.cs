using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{
    public class TriangleMeshStructure  {


        private const int DEFAULT_VERTEX_LAYER = 0;
        public const int DEFAULT_VERTICES_LAYER_DIM = 3;
        private const int DEFAULT_VERTICES_LAYER = 0;

        private int[] first = new int[3];
        private int[] move = new int[3];
        private int[] deltaMove = new int[3];

        QuadraticMeshIndicesArray meshIndicesArray = new QuadraticMeshIndicesArray();

        int M;

        int internalsN;
        int trianglesN;

        public void RetrieveInfos(CPNPolygon polygon)
        {
            CPNSideEdge[] polylines = polygon.sideEdges;

            int M1 = polylines[0].GetN();
            int M2 = polylines[1].GetN();
            int M3 = polylines[2].GetN();

            this.M = M3 > M1 ? M3 : M1;
            this.M = M2 > M ? M2 : M;

            this.internalsN = (((M - 1) * (M - 2)) >> 1);

            if (M == 1)
            {
                this.trianglesN = 1;
            }
            else if (M == 2)
            {
                this.trianglesN = (M1 + M2 + M3 - 2);
            }
            else
            {
                this.trianglesN = (M - 3) * (M - 3) + M1 + M2 + M3 + 3 * (M - 3);
            }

            // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
            // printf("\nSFCPNTrianglesMeshStructure.retrieveInfos inputs M1:%d
            // M2:%d M3:%d evaluated M:%d internalsN:%d trianglesN:%d",
            // M1,M2,M3,M,internalsN,trianglesN);
            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
        }

        private int[] MS = new int[3];

        public void CreateTriangleTessellation(OutputMesh mesh, int internalsIndex, int facesIndex,
                CPNPolygon polygon)
        {

            // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
            // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
            // internalsIndex:%d facesIndex:%d", internalsIndex, facesIndex);
            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

            CPNSideEdge[] polylines = polygon.sideEdges;
            RetrieveInfos(polygon);

            int trPosition = facesIndex;
            if (M == 1) {
                trPosition = mesh.WriteTriangle(trPosition, polylines[0].GetIndex(0), polylines[1].GetIndex(0),
                        polylines[2].GetIndex(0));
                return;
            }
            else if (M == 2)
            {
                // int[] MS=new int[3];
                MS[0] = polylines[0].GetN();
                MS[1] = polylines[1].GetN();
                MS[2] = polylines[2].GetN();
                for (int i = 0; i < 3; i++)
                {
                    int prev = i == 0 ? 2 : i - 1;
                    if (MS[prev] == 2)
                    {
                        // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                        // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                        // M == 2 i:%d trPosition:%d writing CASE 1",i,trPosition);
                        // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                        trPosition = mesh.WriteTriangle(trPosition,
                            polylines[i].GetIndex(0), polylines[i].GetIndex(1),
                            polylines[prev].GetBackIndex(1));
                    }
                    else if (MS[i] == 2)
                    {
                        int other = (i == 2 ? 0 : i + 1);
                        if (MS[other] == 2)
                        {
                            // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                            // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                            // M == 2 i:%d trPosition:%d writing CASE 2", i,
                            // trPosition);
                            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                            trPosition = mesh.WriteTriangle(trPosition, polylines[i].GetIndex(0), polylines[i].GetIndex(1),
                                    polylines[other].GetIndex(1));
                        }
                        else
                        {
                            // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                            // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                            // M == 2 i:%d trPosition:%d writing CASE 3", i,
                            // trPosition);
                            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                            trPosition = mesh.WriteTriangle(trPosition, polylines[i].GetIndex(0), polylines[i].GetIndex(1),
                                    polylines[other].GetIndex(1));
                        }
                    }
                }
                if (MS[0] == 2 && MS[1] == 2 && MS[2] == 2)
                {
                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // M == 2 writing CASE 4");
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    trPosition = mesh.WriteTriangle(trPosition, polylines[0].GetIndex(1), polylines[1].GetIndex(1),
                            polylines[2].GetIndex(1));
                }

                // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                // if(facesIndex+trianglesN!=trPosition)
                // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                // ERROR on trianglesN facesIndex:%d trianglesN:%d trPosition:%d",
                // facesIndex, trianglesN, trPosition);
                // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

                return;
            }

            int innerVerticesPosition = internalsIndex;
            int rowPosition1 = innerVerticesPosition;
            int rowPosition2 = innerVerticesPosition + M - 2;

            // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
            // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
            // writing Internals innerVerticesPosition:%d rowPosition1:%d
            // rowPosition2:%d", innerVerticesPosition, rowPosition1, rowPosition2);
            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

            for (int i = 0; i < M - 3; i++)
            {

                for (int j_ = 0; j_ < M - 4 - i; j_++)
                {
                    trPosition = mesh.WriteTriangle(trPosition, rowPosition1 + j_, rowPosition1 + j_ + 1, rowPosition2 + j_);
                    trPosition = mesh.WriteTriangle(trPosition, rowPosition2 + j_, rowPosition1 + j_ + 1,
                            rowPosition2 + j_ + 1);
                }

                int j = M - 4 - i;

                trPosition = mesh.WriteTriangle(trPosition, rowPosition1 + j, rowPosition1 + j + 1, rowPosition2 + j);

                rowPosition1 = rowPosition2;
                rowPosition2 = rowPosition2 + (M - 3 - i);
                // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                // writing Internals rowPosition1:%d rowPosition2:%d", rowPosition1,
                // rowPosition2);
                // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

            }

            first[0] = innerVerticesPosition;
            first[1] = innerVerticesPosition + M - 3;
            first[2] = innerVerticesPosition + (((M - 1) * (M - 2)) >> 1) - 1;

            move[0] = 1;
            move[1] = M - 2;
            move[2] = -1;

            deltaMove[0] = 0;
            deltaMove[1] = -1;
            deltaMove[2] = -1;

            int count = M - 2;

            // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
            // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
            // preparing Sides count:%d first:[%d,%d,%d] move:[%d,%d,%d]
            // deltaMove:[%d,%d,%d]",
            // count, first[0], first[1], first[2], move[0], move[1], move[2],
            // deltaMove[0], deltaMove[1], deltaMove[2]);
            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

            for (int i = 0; i < 3; i++)
            {

                int N = polylines[i].GetN();
                if (N >= 2)  {
                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // side Triangle case A trPosition:%d", trPosition);
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    meshIndicesArray.Setup(first[i], move[i], deltaMove[i], count, mesh,
                            TriangleMeshStructure.DEFAULT_VERTICES_LAYER);

                    NetPolylineInternalIndicesArray npi = new NetPolylineInternalIndicesArray(polylines[i], mesh);
                    trPosition = MeshStructures.CreateSideTriangles(mesh, meshIndicesArray, npi, trPosition);

                } else if (N == 1) {

                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // side Triangle case B trPosition:%d (Begin)", trPosition);
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    meshIndicesArray.Setup(first[i], move[i], deltaMove[i], count, mesh, DEFAULT_VERTICES_LAYER);

                    NetPolylineIndicesArray npi = new NetPolylineIndicesArray(polylines[i], mesh);
                    trPosition = MeshStructures.CreateSideTriangles(mesh, meshIndicesArray, npi, trPosition);

                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // side Triangle case B trPosition:%d (End)", trPosition);
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                }

                int prev = i == 0 ? 2 : i - 1;
                int prevN = polylines[prev].GetN();
                if (N > 1 && prevN > 1)
                {

                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // writing corner Index as Quad a:%d b:%d c:%d d:%d",
                    // polylines[i].getIndex(1),first[i],
                    // polylines[prev].getBackIndex(1), polylines[i].getIndex(0));
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

                    if (prevN + N < 1.5f * M)
                    {
                        trPosition = mesh.WriteQuad(trPosition, polylines[i].GetIndex(0), polylines[i].GetIndex(1),
                                first[i], polylines[prev].GetBackIndex(1));
                    }
                    else
                    {
                        trPosition = mesh.WriteQuad(trPosition, polylines[i].GetIndex(1), first[i],
                                polylines[prev].GetBackIndex(1), polylines[i].GetIndex(0));
                    }

                } else if (polylines[i].GetN() > 1) {

                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // writing corner Index as Triangle a:%d b:%d c:%d ",
                    // polylines[i].getIndex(0), polylines[i].getIndex(1),
                    // first[i]);
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

                    trPosition = mesh.WriteTriangle(trPosition, polylines[i].GetIndex(0), polylines[i].GetIndex(1),
                            first[i]);
                }
                else if (polylines[prev].GetN() > 1)
                {
                    // #ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
                    // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
                    // writing corner Index as Triangle a:%d b:%d c:%d ",
                    // polylines[i].getIndex(0),
                    // first[i],polylines[i].getBackIndex(1));
                    // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR

                    trPosition = mesh.WriteTriangle(trPosition, polylines[i].GetIndex(0), first[i],
                            polylines[prev].GetBackIndex(1));
                }
            }

            // #
            // ifdef SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
            // if(facesIndex+trianglesN!=trPosition)
            //
            // printf("\nSFCPNTrianglesMeshStructure.createTriangleTessellation
            // ERROR on trianglesN facesIndex:%d trianglesN:%d trPosition:%d",
            // facesIndex, trianglesN, trPosition);
            // #endif //SF_RAW_DEBUG_TRIANGLE_TESSELLATOR
        }

        public int GetInternalsN()
        {
            return internalsN;
        }

        public int GetM()
        {
            return M;
        }

        public int GetTrianglesN()
        {
            return trianglesN;
        }

    }
}