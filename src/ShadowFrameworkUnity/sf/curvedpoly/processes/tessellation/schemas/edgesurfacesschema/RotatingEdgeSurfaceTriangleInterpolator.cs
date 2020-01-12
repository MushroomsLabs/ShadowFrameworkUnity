
//#define INTERPOLATION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;


namespace MLab.ShadowFramework.Interpolation
{
    public class RotatingEdgeSurfaceTriangleInterpolator : ICPNetInterpolator
    {
        public static float ADD_FACTOR = 4;

#if DEBUG
        public static int interpolationCorner = 0;
        public static int interpolationCornerSide = 0;
        public static int thicknessMode = 0;
#endif

        public TriangleMeshStructure triangleStructure = new TriangleMeshStructure(); 

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();

        private InterpolationBuffer buffer0 = new InterpolationBuffer();
        private InterpolationBuffer buffer1 = new InterpolationBuffer();
        private InterpolationBuffer buffer2 = new InterpolationBuffer();
        //private CPNCornerSet corner0 = new CPNCornerSet();
        //private CPNCornerSet corner1 = new CPNCornerSet();
        //private CPNCornerSet corner2 = new CPNCornerSet();
        private CPNEdgeSurface edgeSurface0 = new CPNEdgeSurface();
        private CPNEdgeSurface edgeSurface1 = new CPNEdgeSurface();
        private CPNEdgeSurface edgeSurface2 = new CPNEdgeSurface();

        private float[] ks = new float[3];

        private InterpolationMemory memory = new InterpolationMemory();
         
        public RotatingEdgeSurfaceTriangleInterpolator()
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

        public void computeCorners() {
            InterpolationBuffer[] buffers = { buffer0,buffer1,buffer2};
            for (int i = 0; i < 3; i++)
            {
                int prev = i == 0 ? 2 : i - 1;
                Vector3 dev1 = buffers[i].devFirst.normalized;
                Vector3 dev2 = -buffers[prev].devLast.normalized;
                ks[i] = Vector3.Dot(dev1,dev2);
                ks[i] = ks[i] < 0 ? 1 : 1 - ks[i];
            }
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

            edgeSurface0.Set(buffer0, buffer2, buffer1);
            edgeSurface1.Set(buffer1, buffer0, buffer2);
            edgeSurface2.Set(buffer2, buffer1, buffer0);
            
            computeCorners();

            float l1 = ks[0] * ks[1]* ks[0] * ks[1];
            float l2 = ks[1] * ks[2]* ks[1] * ks[2];
            float l3 = ks[2] * ks[0]* ks[2] * ks[0];

            prepareMemory(M, countProperties);

            int position = internalsIndex;
            for (int i = 1; i < M - 1; i++)
            {
                for (int j = 1; j < M - 1 - (i - 1); j++)
                {  
                    int k = M - i - j;
                    
                    float U = j * step;
                    float V = i * step;
                    float W = 1 - U - V;
                    
                    float a1 = U * U * W * W * l1 /** buffer0.thickness*/;
                    float a2 = V * V * U * U * l2 /** buffer1.thickness*/;
                    float a3 = W * W * V * V * l3 /** buffer2.thickness*/;
                     
                    //float a1 = W * W;
                    //float a2 = U * U;
                    //float a3 = V * V;
 
                    if (TriangleInterpolator4.interpolationCorner != 0)
                    {
                        switch (TriangleInterpolator4.interpolationCorner)
                        {
                            case 1: a1 = 1; a2 = 0; a3 = 0; break;
                            case 2: a1 = 0; a2 = 1; a3 = 0; break;
                            case 3: a1 = 0; a2 = 0; a3 = 1; break;
                        }
                    }
                     
                    float rec = 1.0f / (a1 + a2 + a3);
                    a1 *= rec;
                    a2 *= rec;
                    a3 *= rec;


                    Vector3 V1 = edgeSurface0.evalVertex(/*j,*/ i, polylines[0], U / (1 - V));
                    Vector3 V2 = edgeSurface1.evalVertex(/*i,*/ k, polylines[1], V / (1 - W));
                    Vector3 V3 = edgeSurface2.evalVertex(/*k,*/ j, polylines[2], W / (1 - U));
                    Vector3 vertex = V1 * a1 + V2 * a2 + V3 * a3;

                    int memoryIndex = j + i * (M + 1) - (((i) * (i - 1)) >> 1);
                    memory.vertices[memoryIndex] = vertex;

                    if (useUV) {
                        Vector3 V1uv = edgeSurface0.evalUV(/*j,*/ i, polylines[0], U / (1 - V));
                        Vector3 V2uv = edgeSurface1.evalUV(/*i,*/ k, polylines[1], V / (1 - W));
                        Vector3 V3uv = edgeSurface2.evalUV(/*k,*/ j, polylines[2], W / (1 - U));
                        Vector3 uv = V1uv * a1 + V2uv * a2 + V3uv * a3;
                        //Vector3 vertex = V3;
                        //Vector3 uv = V3uv;

                        memory.uv[memoryIndex] = uv;
                    }

                    for (int pIndex = 0; pIndex < countProperties; pIndex++)
                    { 
                        Vector3 V1prop = edgeSurface0.evalProperty(pIndex, i, polylines[0], U / (1 - V));
                        Vector3 V2prop = edgeSurface1.evalProperty(pIndex, k, polylines[1], V / (1 - W));
                        Vector3 V3prop = edgeSurface2.evalProperty(pIndex, j, polylines[2], W / (1 - U));
                        Vector3 prop = V1prop * a1 + V2prop * a2 + V3prop * a3; 
                        mesh.SetProperty3(position, pIndex, prop); 
                    }

                    position++;
                }
            }

            position = internalsIndex;

            for (int i = 1; i < M - 1; i++) {

                for (int j = 1; j < M - 1 - (i - 1); j++) {

                    int rowIndex = i * (M + 1) - (((i) * (i - 1)) >> 1);
                    int rowIndexPrev = (i - 1) * (M + 1) - (((i - 1) * (i - 2)) >> 1);
                    int rowIndexNext = (i + 1) * (M + 1) - (((i + 1) * (i)) >> 1);

                    int memoryIndex = j + rowIndex;
                    Vector3 vertex = memory.vertices[memoryIndex];
                    Vector3 uv = memory.uv[memoryIndex];

                    Vector3 normal = Vector3.zero;
                    Vector3 tangent = Vector3.zero;

                    if (useNormals) {
                        //Normal (S is the vertices, the surface)
                        Vector3 dSdu = memory.vertices[memoryIndex + 1] - memory.vertices[memoryIndex - 1];
                        Vector3 dSdv = memory.vertices[rowIndexNext + j] - memory.vertices[rowIndexPrev + j];
                        normal = Vector3.Cross(dSdu, dSdv).normalized;

                        if (useTangents) { 
                            //Tangent
                            Vector3 dTxdu = memory.uv[memoryIndex + 1] - memory.uv[memoryIndex - 1];
                            Vector3 dTxdv = memory.uv[rowIndexNext + j] - memory.uv[rowIndexPrev + j];
                            tangent = getTangent(dSdu, dSdv, dTxdu, dTxdv);
                        }
                    }
                     
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
         


        private void prepareMemory(int M, int countProperties) {

            int totalSize = ((M + 1) * (M + 2)) >> 1;

            memory.requestSize(totalSize); 

            //Iterate one time less, since the last vertex on each buffer will be written by the following buffer as first
            for (int i = 0; i < M; i++) {

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