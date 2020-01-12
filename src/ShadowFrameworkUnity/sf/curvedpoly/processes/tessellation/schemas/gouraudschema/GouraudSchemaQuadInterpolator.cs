
//#define INTERPOLATION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;


namespace MLab.ShadowFramework.Interpolation.GouraudSchema
{
    public class SFGouraudSchemaQuadInterpolator : ICPNetInterpolator
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

        public SFGouraudSchemaQuadInterpolator()
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

            corner0.Set(buffer0, buffer3);
            corner1.Set(buffer1, buffer0);
            corner2.Set(buffer2, buffer1);
            corner3.Set(buffer3, buffer2);
             
            int position = internalsIndex;


            for (int i = 1; i < MV; i++)
            {
                for (int j = 1; j < MH; j++)
                {
                    float U = (j) * stepH;
                    float V = (i) * stepV;

                    Vector3 V1 = corner0.evalVertex(j, i);
                    Vector3 V2 = corner1.evalVertex(i, MH - j);
                    Vector3 V3 = corner2.evalVertex(MH - j, MV - i);
                    Vector3 V4 = corner3.evalVertex(MV - i, j);
                     
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
                    
                    Vector3 vertex = a1 * V1 + a2 * V2 + a3 * V3 + a4 * V4;

                    Vector3 normal = Vector3.zero;
                    Vector3 uv = Vector3.zero;
                    Vector3 tangent = Vector3.zero;

                    if (useNormals) {
                        Vector3 V1N = corner0.evalNormal(j, i);
                        Vector3 V2N = corner1.evalNormal(i, MH - j);
                        Vector3 V3N = corner2.evalNormal(MH - j, MV - i);
                        Vector3 V4N = corner3.evalNormal(MV - i, j);
                        normal = a1 * V1N + a2 * V2N + a3 * V3N + a4 * V4N;
                    }

                    if (useUV) { 
                        Vector3 V1uv = corner0.evalUV(j, i);
                        Vector3 V2uv = corner1.evalUV(i, MH - j);
                        Vector3 V3uv = corner2.evalUV(MH - j, MV - i);
                        Vector3 V4uv = corner3.evalUV(MV - i, j);
                        uv = a1 * V1uv + a2 * V2uv + a3 * V3uv + a4 * V4uv;

                        if (useTangents)
                        {
                            float Uu = U + 0.001f;
                            float UMu = 1 - Uu; 
                            float a1u = (UMu * VM * UMu * VM);
                            float a2u = (Uu * VM * Uu * VM);
                            float a3u = (Uu * V * Uu * V);
                            float a4u = (UMu * V * UMu * V);
                            a1u *= rec;a2u *= rec;a3u *= rec;a4u *= rec;
                            float Vv = V + 0.001f;
                            float VMv = 1 - Vv;
                            float a1v = (UM * VMv * UM * VMv);
                            float a2v = (U * VMv * U * VMv);
                            float a3v = (U * Vv * U * Vv);
                            float a4v = (UM * Vv * UM * Vv);
                            a1v *= rec; a2v *= rec; a3v *= rec; a4v *= rec;
                            Vector3 DPu = (a1u * V1 + a2u * V2 + a3u * V3 + a4u * V4) - vertex;
                            Vector3 DPv = (a1v * V1 + a2v * V2 + a3v * V3 + a4v * V4) - vertex;
                            Vector3 DUVu = (a1u * V1uv + a2u * V2uv + a3u * V3uv + a4u * V4uv) - vertex;
                            Vector3 DUVv = (a1v * V1uv + a2v * V2uv + a3v * V3uv + a4v * V4uv) - vertex;
                            tangent = getTangent(DPu, DPv, DUVu, DUVv);
                        }

                    }

                    for (int k = 0; k < countProperties; k++)
                    {
                        Vector3 V1propK = corner0.evalProperty(k, j, i);
                        Vector3 V2propK = corner1.evalProperty(k, i, MH - j);
                        Vector3 V3propK = corner2.evalProperty(k, MH - j, MV - i);
                        Vector3 V4propK = corner3.evalProperty(k, MV - i, j);
                        Vector3 propK = a1 * V1propK + a2 * V2propK + a3 * V3propK + a4 * V4propK;
                        mesh.SetProperty3(position, k, propK);
                    }
                     
                    mesh.SetPNUV(position, vertex, normal, uv, tangent);
                    position++;
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


    }
}
