
//#define INTERPOLATION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Interpolation.GouraudSchema;


namespace MLab.ShadowFramework.Interpolation.CylindricInterpolation
{
    public class SFCylindricInterpolationQuadInterpolator : ICPNetInterpolator
    {
        public static float ADD_FACTOR = 4;

        private QuadsMeshStructure quadStructure = new QuadsMeshStructure();

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();

        private SFGouraudInterpolationBuffer buffer0 = new SFGouraudInterpolationBuffer();
        private SFGouraudInterpolationBuffer buffer1 = new SFGouraudInterpolationBuffer();
        private SFGouraudInterpolationBuffer buffer2 = new SFGouraudInterpolationBuffer();
        private SFGouraudInterpolationBuffer buffer3 = new SFGouraudInterpolationBuffer();

        private SFGouraudSchemaCornerSurface corner0 = new SFGouraudSchemaCornerSurface();
        private SFGouraudSchemaCornerSurface corner1 = new SFGouraudSchemaCornerSurface();
        private SFGouraudSchemaCornerSurface corner2 = new SFGouraudSchemaCornerSurface();
        private SFGouraudSchemaCornerSurface corner3 = new SFGouraudSchemaCornerSurface();

        private InterpolationMemory memory = new InterpolationMemory();

        public SFCylindricInterpolationQuadInterpolator()
        {
        }

        public int GetComputedInternals()
        {
            return quadStructure.GetnInternals();
        }

        public int GetComputedTriangles()
        {
            return quadStructure.GetnTriangles();
        }

        public InterpolationMemory GetMemory()
        {
            return memory;
        }

        public void RetrieveInfos(CPNPolygon buildingPolygonData)
        {
            quadStructure.RetrieveInfos(buildingPolygonData);
        }

        public void UdpdateContent(OutputMesh mesh, CPNPolygon polygon, int internalsIndex, 
            int facesIndex, bool doUpdateStructure = true)
        {
            quadStructure.RetrieveInfos(polygon);

            bool useUV = mesh.DoUseUVs();
            bool useNormals = mesh.DoNormals();
            bool useTangents = mesh.DoUseUVs();
            int countProperties = mesh.CountProperties();
            buffer0.requestProperties(countProperties);
            buffer1.requestProperties(countProperties);
            buffer2.requestProperties(countProperties); 

            int MV = quadStructure.GetMV();
            int MH = quadStructure.GetMH();
            float stepV = 1.0f / MV;
            float stepH = 1.0f / MH;

            CPNSideEdge[] polylines = polygon.sideEdges;
            buffer0.writeWithGuide(polylines[0], MH, mesh, evaluator);
            buffer1.writeWithGuide(polylines[1], MV, mesh, evaluator);
            buffer2.writeWithGuide(polylines[2], MH, mesh, evaluator);
            buffer3.writeWithGuide(polylines[3], MV, mesh, evaluator);

            corner0.Set(buffer0, buffer3);
            corner1.Set(buffer1, buffer0);
            corner2.Set(buffer2, buffer1);
            corner3.Set(buffer3, buffer2);

            prepareMemory(MH, MV);

            int index = internalsIndex;
            for (int i = 1; i < MV; i++)
            {
                for (int j = 1; j < MH; j++)
                {
                    float U = (j) * stepH;
                    float V = (i) * stepV;

                    float UM = 1 - U;
                    float VM = 1 - V;

                    float a1 = (UM * VM * UM * VM);
                    float a2 = (U * VM * U * VM);
                    float a3 = (U * V * U * V);
                    float a4 = (UM * V * UM * V);

                    float rec = 1.0f / (a1 + a2 + a3 + a4);
                    a1 *= rec;
                    a2 *= rec;
                    a3 *= rec;
                    a4 *= rec;

                    Vector3 V1 = corner0.evalVertex(j, i);
                    Vector3 V2 = corner1.evalVertex(i, MH - j);
                    Vector3 V3 = corner2.evalVertex(MH - j, MV - i);
                    Vector3 V4 = corner3.evalVertex(MV - i, j);


                    Vector3 vertex = a1 * V1 + a2 * V2 + a3 * V3 + a4 * V4;

                    int memoryIndex = j + i * (MH + 1);
                    memory.vertices[memoryIndex] = vertex;

                    if (useUV) {
                        //Vector3 vertex = V3;
                        Vector3 V1uv = corner0.evalUV(j, i);
                        Vector3 V2uv = corner1.evalUV(i, MH - j);
                        Vector3 V3uv = corner2.evalUV(MH - j, MV - i);
                        Vector3 V4uv = corner3.evalUV(MV - i, j);
                        Vector3 uv = a1 * V1uv + a2 * V2uv + a3 * V3uv + a4 * V4uv;
                        //Vector3 uv = V3uv;

                        memory.uv[memoryIndex] = uv;
                    }

                    for (int k = 0; k < countProperties; k++)
                    {
                        Vector3 V1propK = corner0.evalProperty(k, j, i);
                        Vector3 V2propK = corner1.evalProperty(k, i, MH - j);
                        Vector3 V3propK = corner2.evalProperty(k, MH - j, MV - i);
                        Vector3 V4propK = corner3.evalProperty(k, MV - i, j);
                        //Vector3 vertex = V3;
                        Vector3 propK = a1 * V1propK + a2 * V2propK + a3 * V3propK + a4 * V4propK;
                        //Vector3 uv = V3uv; 
                        mesh.SetProperty3(index, k, propK); 
                    }

                    index++;
                }
            }

            index = internalsIndex;
            for (int i = 1; i < MV; i++)
            {
                for (int j = 1; j < MH; j++)
                {
                    int rowIndex = i * (MH + 1);
                    int rowIndexPrev = (i - 1) * (MH + 1);
                    int rowIndexNext = (i + 1) * (MH + 1);

                    int memoryIndex = j + rowIndex;
                    Vector3 vertex = memory.vertices[memoryIndex];
                    Vector3 uv = memory.uv[memoryIndex];

                    Vector3 normal = Vector3.zero;
                    Vector3 tangent = Vector3.zero;

                    if (useNormals) {

                        //Normal (S is the vertices, the surface)
                        Vector3 dSdu = memory.vertices[memoryIndex + 1] - memory.vertices[memoryIndex - 1];
                        Vector3 dSdv = memory.vertices[rowIndexNext + j] - memory.vertices[rowIndexPrev + j];
                        dSdu = dSdu.normalized;
                        dSdv = dSdv.normalized;
                        normal = Vector3.Cross(dSdu, dSdv).normalized;

                        if (useTangents)
                        {
                            //Debug.Log("dSdu: (" + dSdu.x + "," + dSdu.y + "," + dSdu.z + ") dSdv:"
                            //    + dSdv.x + "," + dSdv.y + "," + dSdv.z + ") Vector3.Cross(dSdu, dSdv).magnitude:" + Vector3.Cross(dSdu, dSdv).magnitude);
                            //Debug.Log("Normal (" + normal.x + "," + normal.y + "," + normal.z + ")"); 

                            //Tangent
                            Vector3 dTxdu = memory.uv[memoryIndex + 1] - memory.uv[memoryIndex - 1];
                            Vector3 dTxdv = memory.uv[rowIndexNext + j] - memory.uv[rowIndexPrev + j];
                            tangent = getTangent(dSdu, dSdv, dTxdu, dTxdv);
                        }
                    }
                    mesh.SetPNUV(index, vertex, normal, uv, tangent);  

                    index++;
                }
            }

            if(doUpdateStructure)
                quadStructure.CreateQuadTessellation(mesh, internalsIndex, facesIndex, polygon);

        }

        /*  (u,v,w): triangle Homogeneous Coordinates, S(u,v) = (x(u,v),y(u,v),z(u,v)) patch model, Tx(u, v) = (s(u,v),t(u,v)) texture coordinates*/
        private Vector3 getTangent(Vector3 dSdu, Vector3 dSdv, Vector3 dTxdu, Vector3 dTxdv)
        {
            //Looking for dSds, where Tx = (s,t), so s is the first coordinate in the Tex Coords Array 
            float det = dTxdu.x * dTxdv.y - dTxdu.y * dTxdv.x;
            Vector3 tangent = (dSdu * dTxdv.y - dSdv * dTxdu.y).normalized;
            return det > 0 ? tangent : -tangent;
        }


        private void prepareMemory(int MH, int MV)
        {
            int totalSize = ((MH + 1) * (MV + 1));

            memory.requestSize(totalSize);

            //Iterate one time less, since the last vertex on each buffer will be written by the following buffer as first
            for (int i = 0; i < MH; i++)
            {
                //First Buffer
                memory.vertices[i] = buffer0.vertices[i];
                memory.uv[i] = buffer0.uvs[i];

                //Third Buffer
                int backIndex = totalSize - 1 - i;
                memory.vertices[backIndex] = buffer2.vertices[i];
                memory.uv[backIndex] = buffer2.uvs[i];
                
            }

            for (int i = 0; i < MV; i++)
            {
                //First Buffer
                int frontIndex = (i + 1) * (MH + 1) - 1;
                memory.vertices[frontIndex] = buffer1.vertices[i];
                memory.uv[frontIndex] = buffer1.uvs[i];

                //Third Buffer
                int backIndex = (MV - i) * (MH + 1);
                memory.vertices[backIndex] = buffer3.vertices[i];
                memory.uv[backIndex] = buffer3.uvs[i];
                
            }
        }
    }
}
