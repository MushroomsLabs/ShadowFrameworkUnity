using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using MLab.ShadowFramework; 

namespace MLab.ShadowFramework.Interpolation
{
    public class QuadsMeshStructure 
    { 
        private const int DEFAULT_VERTEX_LAYER = 0; 

        int[] first = new int[4];
        int[] move = new int[4];
        int[] count = new int[4];

        int MH, MV;

        int nInternals;
        int nTriangles;

        public void RetrieveInfos(CPNPolygon polygon)
        {
            CPNSideEdge[] polylines = polygon.sideEdges;

            int M1 = polylines[0].GetN();
            int M2 = polylines[1].GetN();
            int M3 = polylines[2].GetN();
            int M4 = polylines[3].GetN();

            this.MV = M4 > M2 ? M4 : M2;
            this.MH = M3 > M1 ? M3 : M1; 

            this.nInternals = (MH - 1) * (MV - 1);

            if (MV == 1 && MH == 1)
            {
                this.nTriangles = 2;
            }
            else
            {
                this.nTriangles = 2 * (MH - 2) * (MV - 2) + M1 + M2 + M3 + M4 + 2 * (MH - 2) + 2 * (MV - 2);
            }

        }
        public void CreateQuadTessellation(OutputMesh builder, int internalsIndex, int facesIndex,
                CPNPolygon polygon)
        { 
            /*---- SPECIAL CASES  ----*/
            if (DealWithSpecialCases(builder,facesIndex,polygon))
            {  
                return;
            }

            CPNSideEdge[] polylines = polygon.sideEdges; 
            int trPosition = facesIndex; 
            int innerVerticesPosition = internalsIndex;
            //Internal Quads -- Pretty Straightforward
            for ( int i = 0; i < MV - 2; i++)
            {
                int rowPosition1 = innerVerticesPosition + (i) * (MH - 1);
                int rowPosition2 = innerVerticesPosition + (i + 1) * (MH - 1);

                for (int j = 0; j < MH - 2; j++)
                {  
                    trPosition = builder.WriteQuad(trPosition, rowPosition1 + j, rowPosition1 + j + 1,
                        rowPosition2 + j + 1, rowPosition2 + j);
                }
            }

            UpdateSides(builder, polygon, trPosition, innerVerticesPosition); 
        }

        public int UpdateSides(OutputMesh builder, CPNPolygon polygon, int trPosition,
                int innerVerticesPosition)
        {
            CPNSideEdge[] polylines = polygon.sideEdges;

            first[0] = innerVerticesPosition;
            first[1] = innerVerticesPosition + (MH - 2);
            first[2] = innerVerticesPosition + (MV - 1) * (MH - 1) - 1;
            first[3] = innerVerticesPosition + (MV - 2) * (MH - 1);

            move[0] = 1;
            move[1] = (MH - 1);
            move[2] = -1;
            move[3] = -(MH - 1);

            count[0] = MH - 1;
            count[1] = MV - 1;
            count[2] = MH - 1;
            count[3] = MV - 1;

            for (int i = 0; i < 4; i++)
            { 
                if (polylines[i].GetN() > 1)
                { 
                    LinearMeshIndicesArray lmi = new LinearMeshIndicesArray(first[i], move[i], count[i], builder); 
                    NetPolylineInternalIndicesArray npi = new NetPolylineInternalIndicesArray(polylines[i], builder);
                    trPosition = MeshStructures.CreateSideTriangles(builder, lmi, npi, trPosition);
                } else { 
                    LinearMeshIndicesArray lmi = new LinearMeshIndicesArray(first[i], move[i], count[i], builder); 
                    NetPolylineIndicesArray npi = new NetPolylineIndicesArray(polylines[i], builder);
                    trPosition = MeshStructures.CreateSideTriangles(builder, lmi, npi, trPosition);
                }

                int prev = i == 0 ? 3 : i - 1;
                if (polylines[i].GetN() > 1 && polylines[prev].GetN() > 1)
                {
                    trPosition = builder.WriteQuad(trPosition, polylines[i].GetIndex(0), polylines[i].GetIndex(1), first[i],
                            polylines[prev].GetBackIndex(1));
                }
                else if (polylines[i].GetN() > 1)
                {
                    trPosition = builder.WriteTriangle(trPosition, polylines[i].GetIndex(0), polylines[i].GetIndex(1),
                            first[i]);
                }
                else if (polylines[prev].GetN() > 1)
                {
                    trPosition = builder.WriteTriangle(trPosition, polylines[i].GetIndex(0), first[i],
                            polylines[prev].GetBackIndex(1));
                }

                // System.err.println("trPosition on sides " + trPosition);
            }

            return trPosition;

        }

        private bool DealWithSpecialCases(OutputMesh builder, int facesIndex,
                CPNPolygon polygon)
        {
            CPNSideEdge[] polylines = polygon.sideEdges;
            /*---- SPECIAL CASES  ----*/
            if (MV == 1 && MH != 1)
            {
                NetPolylineIndicesArray array1 = new NetPolylineIndicesArray(polylines[0], builder);
                NetPolylineIndicesArray array2 = new NetPolylineIndicesArray(polylines[2], builder, true);
                MeshStructures.CreateSideTriangles(builder, array2, array1, facesIndex);
                return true;
            }

            if (MV != 1 && MH == 1)
            {
                NetPolylineIndicesArray array1 = new NetPolylineIndicesArray(polylines[1], builder);
                NetPolylineIndicesArray array2 = new NetPolylineIndicesArray(polylines[3], builder, true);
                MeshStructures.CreateSideTriangles(builder, array2, array1, facesIndex);
                return true;
            }

            if (MV == 1 && MH == 1)
            {
                builder.WriteQuad(facesIndex, polylines[0].GetFirstVertex(), polylines[1].GetFirstVertex(),
                        polylines[2].GetFirstVertex(), polylines[3].GetFirstVertex());
                return true;
            }
            return false;
        }


        public int GetnInternals()
        {
            return nInternals;
        }

        public int GetnTriangles()
        {
            return nTriangles;
        }

        public int GetMH()
        {
            return MH;
        }

        public int GetMV()
        {
            return MV;
        }

    }
}