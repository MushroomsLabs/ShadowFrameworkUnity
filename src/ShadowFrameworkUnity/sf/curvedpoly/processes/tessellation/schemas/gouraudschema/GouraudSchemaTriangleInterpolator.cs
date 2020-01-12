
//#define INTERPOLATION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;


namespace MLab.ShadowFramework.Interpolation.GouraudSchema
{
    public class SFGouraudSchemaTriangleInterpolator : ICPNetInterpolator
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

        public SFGouraudSchemaTriangleInterpolator()
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

                    Vector3 normal = Vector3.zero;
                    Vector3 uv = Vector3.zero;
                    Vector3 tangent = Vector3.zero;

                    if (useNormals) {
                        Vector3 V1N = corner0.evalNormal(j, i);
                        Vector3 V2N = corner1.evalNormal(i, wIndex);
                        Vector3 V3N = corner2.evalNormal(wIndex, j);
                        normal = V1N * a1 + V2N * a2 + V3N * a3;
                    }

                    if (useUV) {
                        Vector3 V1uv = corner0.evalUV(j, i);
                        Vector3 V2uv = corner1.evalUV(i, wIndex);
                        Vector3 V3uv = corner2.evalUV(wIndex, j);
                        uv = V1uv * a1 + V2uv * a2 + V3uv * a3;

                        if (useTangents)
                        {
                            float Uu = U + 0.001f;
                            float Wu = 1 - Uu - V;
                            float a1u = Wu * Wu;
                            float a2u = Uu * Uu;
                            float a3u = V * V;
                            a1u *= rec; a2u *= rec; a3u *= rec;
                            float Vv = V + 0.001f;
                            float Wv = 1 - U - Vv;
                            float a1v = Wv * Wv;
                            float a2v = U * U;
                            float a3v = Vv * Vv;
                            a1v *= rec; a2v *= rec; a3v *= rec;
                            Vector3 DPu = (a1u * V1 + a2u * V2 + a3u * V3) - vertex;
                            Vector3 DPv = (a1v * V1 + a2v * V2 + a3v * V3) - vertex;
                            Vector3 DUVu = (a1u * V1uv + a2u * V2uv + a3u * V3uv) - vertex;
                            Vector3 DUVv = (a1v * V1uv + a2v * V2uv + a3v * V3uv) - vertex;
                            tangent = getTangent(DPu, DPv, DUVu, DUVv);
                        }
                    } 

                    for (int k = 0; k < countProperties; k++)
                    {
                        Vector3 V1propK = corner0.evalUV(j, i);
                        Vector3 V2propK = corner1.evalUV(i, wIndex);
                        Vector3 V3propK = corner2.evalUV(wIndex, j);
                        Vector3 propK = a1 * V1propK + a2 * V2propK + a3 * V3propK;
                        mesh.SetProperty3(position, k, propK);
                    }

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
 
    }
}
