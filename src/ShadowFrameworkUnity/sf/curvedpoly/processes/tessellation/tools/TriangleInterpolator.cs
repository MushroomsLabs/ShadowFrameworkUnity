
//#define INTERPOLATION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;


namespace MLab.ShadowFramework.Interpolation
{
    public class TriangleInterpolator : ICPNetInterpolator
    {

#if DEBUG
        public static int interpolationCorner = 0;
#endif


        public TriangleMeshStructure triangleStructure = new TriangleMeshStructure(); 

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();

        private InterpolationBuffer buffer0 = new InterpolationBuffer();
        private InterpolationBuffer buffer1 = new InterpolationBuffer();
        private InterpolationBuffer buffer2 = new InterpolationBuffer();

        private InterpolationMemory memory = new InterpolationMemory();
         
        public TriangleInterpolator()
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

            int M = triangleStructure.GetM();
            float step = 1.0f / M;
            CPNSideEdge[] polylines = polygon.sideEdges;
            buffer0.writeWithGuide(polylines[0], M, mesh, evaluator);
            buffer1.writeWithGuide(polylines[1], M, mesh, evaluator);
            buffer2.writeWithGuide(polylines[2], M, mesh, evaluator);

            prepareMemory(M); 
             
            for (int i = 1; i < M - 1; i++)
            {
                for (int j = 1; j < M - 1 - (i - 1); j++)
                {  
                    int wIndex = M - i - j;
                        
                    Vector3 V1 = evalVertex(buffer0, buffer2, j, i);
                    Vector3 V1uv = evalUV(buffer0, buffer2, j, i);

                    Vector3 V2 = evalVertex(buffer1, buffer0, i, wIndex);
                    Vector3 V2uv = evalUV(buffer1, buffer0, i, wIndex); 

                    Vector3 V3 = evalVertex(buffer2, buffer1, wIndex, j);
                    Vector3 V3uv = evalUV(buffer2, buffer1, wIndex, j);

                    float U = j * step;
                    float V = i * step;
                    float W = 1 - U - V;

                    float a1 = W * W;
                    float a2 = U * U;
                    float a3 = V * V;


#if DEBUG
                    if (interpolationCorner != 0) {
                        switch (interpolationCorner) {
                            case 1: a1 = 1; a2 = 0; a3 = 0; break;
                            case 2: a1 = 0; a2 = 1; a3 = 0; break;
                            case 3: a1 = 0; a2 = 0; a3 = 1; break;
                        } 
                    }
#endif

                    float rec = 1.0f / (a1 + a2 + a3);
                    a1 *= rec;
                    a2 *= rec;
                    a3 *= rec;

                    Vector3 vertex = V1 * a1 + V2 * a2 + V3 * a3;
                    Vector3 uv = V1uv * a1 + V2uv * a2 + V3uv * a3;
                          
                    int memoryIndex = j + i * (M + 1) - (((i) * (i - 1)) >> 1);
                    memory.vertices[memoryIndex] = vertex;
                    memory.uv[memoryIndex] = uv; 
                }
            }

            int position = internalsIndex;

            for (int i = 1; i < M - 1; i++) {

                for (int j = 1; j < M - 1 - (i - 1); j++) {

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
        private Vector3 getTangent(Vector3 dSdu, Vector3 dSdv, Vector3 dTxdu, Vector3 dTxdv) {
            //Looking for dSds, where Tx = (s,t), so s is the first coordinate in the Tex Coords Array 
            float det = dTxdu.x * dTxdv.y - dTxdu.y * dTxdv.x; 
            Vector3 tangent = (dSdu * dTxdv.y - dSdv * dTxdu.y).normalized; 
            return det > 0 ? tangent : -tangent;
        }
         


        private void prepareMemory(int M) {

            int totalSize = ((M + 1) * (M + 2)) >> 1;

            memory.requestSize(totalSize);
            
            //Iterate one time less, since the last vertex on each buffer will be written by the following buffer as first
            for (int i = 0; i < M; i++) {

                //First Buffer
                memory.vertices[i] = buffer0.vertices[i];
                memory.uv[i] = buffer0.uvs[i];

                //Second Buffer
                int iSubtract = ((i) * (i - 1)) >> 1;
                memory.vertices[(i + 1) * M - iSubtract] = buffer1.vertices[i];
                memory.uv[(i + 1) * M - iSubtract] = buffer1.uvs[i];

                //Third Buffer
                iSubtract = ((i + 1) * (i + 2)) >> 1;
                memory.vertices[totalSize - iSubtract] = buffer2.vertices[i];
                memory.uv[totalSize - iSubtract] = buffer2.uvs[i];
            }
        }
         

        private Vector3 evalVertex(InterpolationBuffer buffer,
            InterpolationBuffer bufferPrev, int Aindex, int Bindex)
        {
            int backBIndex = bufferPrev.N - Bindex;

            Vector3 v0 = buffer.vertices[0];
            Vector3 vA = buffer.vertices[Aindex];
            Vector3 vB = bufferPrev.vertices[backBIndex];

            float V = Bindex * bufferPrev.step; 
            Vector3 nA = buffer.normals[Aindex];//already normalized by InterpolationBuffer
            Vector3 dB0 = -bufferPrev.devLast;
            float kA = -Vector3.Dot(dB0, nA)/* / (Vector3.Dot(nA, nA))*/;
            Vector3 part1 = vA + buffer.thickness * ((vB - v0) + kA * V * nA);

            float U = Aindex * buffer.step;
            Vector3 nB = bufferPrev.normals[backBIndex];//already normalized by InterpolationBuffer
            Vector3 dA0 = buffer.devFirst;
            float kB = -Vector3.Dot(dA0, nB)/* / (Vector3.Dot(nA, nA))*/;
            Vector3 part2 = vB + bufferPrev.thickness * ((vA - v0) + kB * U * nB);

            float U2 = U * U;
            float V2 = V * V;
             
            return (part1 * U2  + part2 * V2) / (U2 + V2);
        }
         
        public Vector3 evalUV(InterpolationBuffer buffer,
            InterpolationBuffer bufferPrev, int Aindex, int Bindex)
        {
            int backBIndex = bufferPrev.N - Bindex;

            Vector3 v0 = buffer.uvs[0];
            Vector3 vA = buffer.uvs[Aindex];
            Vector3 vB = bufferPrev.uvs[backBIndex];
            
            return vA + vB - v0;
        }
         

    }
}