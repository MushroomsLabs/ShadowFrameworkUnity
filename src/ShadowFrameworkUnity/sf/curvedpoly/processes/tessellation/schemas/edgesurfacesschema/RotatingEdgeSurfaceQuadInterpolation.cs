using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{
    public class RotatingEdgeSurfaceQuadInterpolation : ICPNetInterpolator
    {
        public static float ADD_FACTOR = 1.0f;
        
        QuadsMeshStructure quadStructure = new QuadsMeshStructure(); 

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();

        //This buffers are used to store (temporarly) data read from Side Edges (Guides)
        private InterpolationBuffer buffer0 = new InterpolationBuffer();
        private InterpolationBuffer buffer1 = new InterpolationBuffer();
        private InterpolationBuffer buffer2 = new InterpolationBuffer();
        private InterpolationBuffer buffer3 = new InterpolationBuffer();
         
        private CPNRotatingEdgeSurface edgeSurface0 = new CPNRotatingEdgeSurface();
        private CPNRotatingEdgeSurface edgeSurface1 = new CPNRotatingEdgeSurface();
        private CPNRotatingEdgeSurface edgeSurface2 = new CPNRotatingEdgeSurface();
        private CPNRotatingEdgeSurface edgeSurface3 = new CPNRotatingEdgeSurface();
         
        private InterpolationMemory memory = new InterpolationMemory();

        public RotatingEdgeSurfaceQuadInterpolation()
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

        public InterpolationMemory GetMemory() {
            return memory;
        }

        public void RetrieveInfos(CPNPolygon buildingPolygonData)
        {
            quadStructure.RetrieveInfos(buildingPolygonData); 
        }
        

        private float GetBufferLength(InterpolationBuffer buffer) {

            return Vector3.Distance(buffer.vertices[0], buffer.vertices[buffer.N]);
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
            buffer3.requestProperties(countProperties);

            int MV = quadStructure.GetMV();
            int MH = quadStructure.GetMH();
            float stepV = 1.0f / MV;
            float stepH = 1.0f / MH;

            CPNSideEdge[] polylines = polygon.sideEdges;
            buffer0.writeWithGuide(polylines[0], MH, mesh, evaluator);
            buffer1.writeWithGuide(polylines[1], MV, mesh, evaluator);
            buffer2.writeWithGuide(polylines[2], MH, mesh, evaluator);
            buffer3.writeWithGuide(polylines[3], MV, mesh, evaluator);

            edgeSurface0.Set(buffer0, buffer3, buffer1);
            edgeSurface1.Set(buffer1, buffer0, buffer2);
            edgeSurface2.Set(buffer2, buffer1, buffer3);
            edgeSurface3.Set(buffer3, buffer2, buffer0);
            
            //float l1 = ks[0] * ks[1] * ks[0] * ks[1];
            //float l2 = ks[1] * ks[2] * ks[1] * ks[2];
            //float l3 = ks[2] * ks[3] * ks[2] * ks[3];
            //float l4 = ks[3] * ks[0] * ks[3] * ks[0];
            float l1 = GetBufferLength(buffer0);
            float l2 = GetBufferLength(buffer1);
            float l3 = GetBufferLength(buffer2);
            float l4 = GetBufferLength(buffer3);

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

                    float a1 = U * U * VM * VM * UM * UM * l1 /** buffer0.thickness*/;
                    float a2 = VM * VM * U * U * V * V * l2 /** buffer1.thickness*/;
                    float a3 = UM * UM * V * V * U * U * l3 /** buffer2.thickness*/;
                    float a4 = V * V * UM * UM * VM * VM * l4 /** buffer3.thickness*/;
                     
#if DEBUG
                    if (TriangleInterpolator4.interpolationCorner != 0)
                    {
                        switch (TriangleInterpolator4.interpolationCorner)
                        {
                            case 1: a1 = 1; a2 = 0; a3 = 0; a4 = 0; break;
                            case 2: a1 = 0; a2 = 1; a3 = 0; a4 = 0; break;
                            case 3: a1 = 0; a2 = 0; a3 = 1; a4 = 0; break;
                            case 4: a1 = 0; a2 = 0; a3 = 0; a4 = 1; break;
                        }
                    }
#endif

                    float rec = 1.0f / (a1 + a2 + a3 + a4);
                    a1 *= rec;
                    a2 *= rec;
                    a3 *= rec;
                    a4 *= rec;
                     
                    Vector3 V1 = edgeSurface0.evalVertex(j, i);
                    Vector3 V2 = edgeSurface1.evalVertex(i, MH - j);
                    Vector3 V3 = edgeSurface2.evalVertex(MH - j, MV - i);
                    Vector3 V4 = edgeSurface3.evalVertex(MV - i, j);
                    Vector3 vertex = a1 * V1 + a2 * V2 + a3 * V3 + a4 * V4;
                    int memoryIndex = j + i * (MH + 1);
                    memory.vertices[memoryIndex] = vertex;

                    if (useUV) {
                        Vector3 V1uv = edgeSurface0.evalUV(j, i);
                        Vector3 V2uv = edgeSurface1.evalUV(i, MH - j);
                        Vector3 V3uv = edgeSurface2.evalUV(MH - j, MV - i);
                        Vector3 V4uv = edgeSurface3.evalUV(MV - i, j);
                        //Vector3 vertex = V3;
                        Vector3 uv = a1 * V1uv + a2 * V2uv + a3 * V3uv + a4 * V4uv;
                        //Vector3 uv = V3uv; 
                        memory.uv[memoryIndex] = uv;
                    }

                    for (int k = 0; k < countProperties; k++)
                    { 
                        Vector3 V1propK = edgeSurface0.evalProperty(k,j, i);
                        Vector3 V2propK = edgeSurface1.evalProperty(k, i, MH - j);
                        Vector3 V3propK = edgeSurface2.evalProperty(k, MH - j, MV - i);
                        Vector3 V4propK = edgeSurface3.evalProperty(k, MV - i, j);
                        Vector3 propK = a1 * V1propK + a2 * V2propK + a3 * V3propK + a4 * V4propK;
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
                        Vector3 dSdu = memory.vertices[memoryIndex + 1] - memory.vertices[memoryIndex - 1];
                        Vector3 dSdv = memory.vertices[rowIndexNext + j] - memory.vertices[rowIndexPrev + j];
                        dSdu = dSdu.normalized;
                        dSdv = dSdv.normalized;
                        normal = Vector3.Cross(dSdu, dSdv).normalized;

                        if (useTangents) {
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