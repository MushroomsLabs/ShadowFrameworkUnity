
//#define INTERPOLATION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Interpolation.GouraudSchema;

namespace MLab.ShadowFramework.Interpolation.CylindricInterpolation
{
    public class SFCylindricInterpolationTriangleInterpolator : ICPNetInterpolator
    {
        public static float ADD_FACTOR = 4;
 
        public TriangleMeshStructure triangleStructure = new TriangleMeshStructure();

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();

        private SFGouraudInterpolationBuffer buffer0 = new SFGouraudInterpolationBuffer();
        private SFGouraudInterpolationBuffer buffer1 = new SFGouraudInterpolationBuffer();
        private SFGouraudInterpolationBuffer buffer2 = new SFGouraudInterpolationBuffer();
         
        private SFGouraudSchemaCornerSurface corner0 = new SFGouraudSchemaCornerSurface();
        private SFGouraudSchemaCornerSurface corner1 = new SFGouraudSchemaCornerSurface();
        private SFGouraudSchemaCornerSurface corner2 = new SFGouraudSchemaCornerSurface();

        private InterpolationMemory memory = new InterpolationMemory();

        public SFCylindricInterpolationTriangleInterpolator()
        {
        }

        public int GetComputedInternals()
        {
            return triangleStructure.GetInternalsN();
        }

        public int GetComputedTriangles()
        {
            return triangleStructure.GetTrianglesN();
        }

        public InterpolationMemory GetMemory()
        {
            return memory;
        }

        public void RetrieveInfos(CPNPolygon buildingPolygonData)
        {
            triangleStructure.RetrieveInfos(buildingPolygonData);
        }

        public void UdpdateContent(OutputMesh mesh, CPNPolygon polygon, int internalsIndex, 
            int facesIndex, bool doUpdateStructure = true)
        {
            triangleStructure.RetrieveInfos(polygon);

            bool useUV = mesh.DoUseUVs();
            bool useNormals = mesh.DoNormals();
            bool useTangents = mesh.DoUseUVs();
            int countProperties = mesh.CountProperties();
            buffer0.requestProperties(countProperties);
            buffer1.requestProperties(countProperties);
            buffer2.requestProperties(countProperties); 

            int M = triangleStructure.GetM();
            float step = 1.0f / M;
            CPNSideEdge[] polylines = polygon.sideEdges;
            buffer0.writeWithGuide(polylines[0], M, mesh, evaluator);
            buffer1.writeWithGuide(polylines[1], M, mesh, evaluator);
            buffer2.writeWithGuide(polylines[2], M, mesh, evaluator);

            corner0.Set(buffer0, buffer2);
            corner1.Set(buffer1, buffer0);
            corner2.Set(buffer2, buffer1);

            prepareMemory(M);

            int position = internalsIndex;

            for (int i = 1; i < M - 1; i++)
            {
                for (int j = 1; j < M - 1 - (i - 1); j++)
                {
                    int wIndex = M - i - j;

                    float U = j * step;
                    float V = i * step;
                    float W = 1 - U - V;

                    float a1 = W * W;
                    float a2 = U * U;
                    float a3 = V * V;
 
                    float rec = 1.0f / (a1 + a2 + a3);
                    a1 *= rec;
                    a2 *= rec;
                    a3 *= rec;

                    Vector3 V1 = corner0.evalVertex(j, i); 
                    Vector3 V2 = corner1.evalVertex(i, wIndex); 
                    Vector3 V3 = corner2.evalVertex(wIndex, j); 
                    Vector3 vertex = V1 * a1 + V2 * a2 + V3 * a3; 
                    int memoryIndex = j + i * (M + 1) - (((i) * (i - 1)) >> 1);
                    memory.vertices[memoryIndex] = vertex;

                    if (useUV) {
                        Vector3 V1uv = corner0.evalUV(j, i);
                        Vector3 V2uv = corner1.evalUV(i, wIndex);
                        Vector3 V3uv = corner2.evalUV(wIndex, j);
                        Vector3 uv = V1uv * a1 + V2uv * a2 + V3uv * a3;
                        memory.uv[memoryIndex] = uv;
                    }

                    for (int k = 0; k < countProperties; k++)
                    {

                        Vector3 V1prop = corner0.evalProperty(k, j, i);
                        Vector3 V2prop = corner1.evalProperty(k, i, wIndex);
                        Vector3 V3prop = corner2.evalProperty(k, wIndex, j);
                        Vector3 prop = V1prop * a1 + V2prop * a2 + V3prop * a3; 
                        mesh.SetProperty3(position, k, prop);
                    }
                     
                    position++;
                }
            }

            position = internalsIndex;

            for (int i = 1; i < M - 1; i++)
            {

                for (int j = 1; j < M - 1 - (i - 1); j++)
                {
                    int rowIndex = i * (M + 1) - (((i) * (i - 1)) >> 1);
                    int rowIndexPrev = (i - 1) * (M + 1) - (((i - 1) * (i - 2)) >> 1);
                    int rowIndexNext = (i + 1) * (M + 1) - (((i + 1) * (i)) >> 1);

                    int memoryIndex = j + rowIndex;
                    Vector3 vertex = memory.vertices[memoryIndex];
                    Vector3 uv = memory.uv[memoryIndex];

                    //Normal (S is the vertices, the surface)
                    Vector3 dSdu = memory.vertices[memoryIndex + 1] - memory.vertices[memoryIndex - 1];
                    Vector3 dSdv = memory.vertices[rowIndexNext + j] - memory.vertices[rowIndexPrev + j];
                    Vector3 normal = Vector3.Cross(dSdu, dSdv).normalized;

                    //Tangent
                    Vector3 dTxdu = memory.uv[memoryIndex + 1] - memory.uv[memoryIndex - 1];
                    Vector3 dTxdv = memory.uv[rowIndexNext + j] - memory.uv[rowIndexPrev + j];
                    Vector3 tangent = getTangent(dSdu, dSdv, dTxdu, dTxdv);

                    mesh.SetPNUV(position, vertex, normal, uv, tangent); 

                    position++;
                }
            }

            if(doUpdateStructure)
                triangleStructure.CreateTriangleTessellation(mesh, internalsIndex, facesIndex, polygon);
        }

        /*  (u,v,w): triangle Homogeneous Coordinates, S(u,v) = (x(u,v),y(u,v),z(u,v)) patch model, Tx(u, v) = (s(u,v),t(u,v)) texture coordinates*/
        private Vector3 getTangent(Vector3 dSdu, Vector3 dSdv, Vector3 dTxdu, Vector3 dTxdv)
        {
            //Looking for dSds, where Tx = (s,t), so s is the first coordinate in the Tex Coords Array 
            float det = dTxdu.x * dTxdv.y - dTxdu.y * dTxdv.x;
            Vector3 tangent = (dSdu * dTxdv.y - dSdv * dTxdu.y).normalized;
            return det > 0 ? tangent : -tangent;
        }



        private void prepareMemory(int M)
        {

            int totalSize = ((M + 1) * (M + 2)) >> 1;

            memory.requestSize(totalSize); 

            //Iterate one time less, since the last vertex on each buffer will be written by the following buffer as first
            for (int i = 0; i < M; i++)
            {

                //First Buffer
                memory.vertices[i] = buffer0.vertices[i];
                memory.uv[i] = buffer0.uvs[i]; 

                //Second Buffer
                int iSubtract = ((i) * (i - 1)) >> 1;
                int index = (i + 1) * M - iSubtract;
                memory.vertices[index] = buffer1.vertices[i];
                memory.uv[index] = buffer1.uvs[i]; 

                //Third Buffer
                iSubtract = ((i + 1) * (i + 2)) >> 1;
                index = totalSize - iSubtract;
                memory.vertices[index] = buffer2.vertices[i];
                memory.uv[index] = buffer2.uvs[i]; 
            }
        }

    }
}
