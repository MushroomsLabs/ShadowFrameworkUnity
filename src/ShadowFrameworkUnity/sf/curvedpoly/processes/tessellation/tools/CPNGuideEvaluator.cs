using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{ 
    public class CPNGuideEvaluator : IGuideModel
    {
        public const float ONE_THIRD = 1.0f / 3.0f;
        public const float TWO_THIRDS = 2.0f / 3.0f;

        public static short[] CPNGuide_loqs;

        private float T1 = 0, T2 = 0, T3 = 0, T4 = 0, t = 0, tm = 0, G, recG;
        private int polylineIndex=0;

        /*
        if (linear) {   //forgot linear evaluation.. uff.
            mesh.SetVertex(index, vBuffer[0] * t + vBuffer[1] * tm);
            if (uvAvailable)
                mesh.SetUV(index, uvsBuffer[0] * t + uvsBuffer[1] * tm);
            if (nsAvailable)
            {

                Vector3 Dev = vBuffer[1] - vBuffer[0];
                Vector3 Perp = ((1 - t) * perpA + t * perpB).normalized;

                Vector3 normal = Vector3.Normalize(Vector3.Cross(Dev, Perp)).normalized;

                mesh.SetNormal(index, normal);
            }
        }*/
         
        public float EvalAt(float t, CPNGuide guide)
        {
            if (guide.GetN() == -1) {//Polylines

                int N = guide.GetIndices().Length - 1;

                if (N > 1)
                {
                    float T = t * N;
                    int pI = (int)T;
                    pI = (pI == N) ? pI - 1 : pI;
                    this.polylineIndex = pI;
                    this.T2 = T - pI;
                    this.T1 = 1 - this.T2;
                }
                else {
                    this.polylineIndex = 0;
                }

                return t;
            }

            float j = t * guide.GetN();
            t = j * (guide.tessellationStepA + j * guide.tessellationStepB);
            this.t = t;
            this.tm = 1 - this.t;
            
            this.T1 = tm * tm * tm;
            this.T2 = 3 * t * tm * tm;
            this.T3 = 3 * t * t * tm;
            this.T4 = t * t * t;
            
            float w2 = guide.w2;
            float w3 = guide.w3;
            this.G = (T1 + T2 * w2 + T3 * w3 + T4);
            this.recG = 1.0f / G;

            return t;
        }

        public Vector3 EvalVertex(CPNGuide guide) {
            
            if (guide.GetN() == -1)
            {
                return guide.vBuffer[this.polylineIndex] * T1 + guide.vBuffer[this.polylineIndex+1] * T2;
            }

            Vector3[] vBuffer = guide.vBuffer;
            float w2 = guide.w2;
            float w3 = guide.w3;
            Vector3 F = (vBuffer[0] * T1 + vBuffer[1] * T2 * w2 + vBuffer[2] * T3 * w3 + vBuffer[3] * T4);
            return F * this.recG;
        }

        public Vector3 EvalDev(CPNGuide guide)
        {
            if (guide.GetN() == -1)
            {
                return (guide.vBuffer[this.polylineIndex + 1] - guide.vBuffer[this.polylineIndex]) * (guide.GetIndices().Length - 1);
            }

            float tp = t + 0.001f;
            float tmp = 1 - tp;

            float T1p = tmp * tmp * tmp;
            float T2p = 3 * tp * tmp * tmp;
            float T3p = 3 * tp * tp * tmp;
            float T4p = tp * tp * tp;
            float w2 = guide.w2;
            float w3 = guide.w3;

            Vector3[] vBuffer = guide.vBuffer;
            Vector3 F1 = (vBuffer[0] * T1 + vBuffer[1] * T2 * w2 + vBuffer[2] * T3 * w3 + vBuffer[3] * T4) /
                (T1 + T2 * w2 + T3 * w3 + T4);
            Vector3 F2 = (vBuffer[0] * T1p + vBuffer[1] * T2p * w2 + vBuffer[2] * T3p * w3 + vBuffer[3] * T4p) /
                (T1p + T2p * w2 + T3p * w3 + T4p);

            return (F2 - F1) * (1000f);
        }

        public Vector3 EvalAxis(CPNGuide guide)
        {
            if (guide.GetN() == -1)
            {
                int N = guide.GetIndices().Length;
                if (N > 0)
                {
                    Vector3 pDev1 = (guide.vBuffer[1] - guide.vBuffer[0]).normalized;
                    Vector3 pPerp1 = Vector3.Cross(pDev1, guide.nBuffer[0]).normalized;

                    Vector3 pDev2 = (guide.vBuffer[N - 1] - guide.vBuffer[N - 2]).normalized;
                    Vector3 pPerp2 = Vector3.Cross(pDev2, guide.nBuffer[N - 1]).normalized;

                    Vector3 pPerp = ((T1 + T2) * pPerp1 + (T3 + T4) * pPerp2);

                    return pPerp.normalized;
                }
                else {
                    return Vector3.zero;
                }
                
            }

            Vector3 Dev1 = (guide.vBuffer[1] - guide.vBuffer[0]).normalized;
            Vector3 Perp1 = Vector3.Cross(Dev1, guide.firstNormal).normalized;
            
            Vector3 Dev2 = (guide.vBuffer[3] - guide.vBuffer[2]).normalized;
            Vector3 Perp2 = Vector3.Cross(Dev2, guide.lastNormal).normalized;
           
            Vector3 Perp = ((T1 + T2) * Perp1 + (T3 + T4) * Perp2);

            return Perp.normalized;

        }


        public Vector3 EvalNormal(CPNGuide guide, Vector3 Dev)
        {
            if (guide.GetN() == -1)
            {
                Vector3 n= guide.nBuffer[this.polylineIndex] * T1 + guide.nBuffer[this.polylineIndex + 1] * T2;
                n = n - Vector3.Dot(Dev, n) * Dev;
                n = n.normalized;
                return n;
            }

            Dev = Dev.normalized;

            Vector3 AN = guide.firstNormal;
            Vector3 ABN = guide.edgeNormal;
            Vector3 BN = guide.lastNormal;

            Vector3 normal = tm * tm * AN +
                2 * t * tm * (2 * ABN - 0.5f * AN - 0.5f * BN) +
                t * t * BN;
            normal = normal - Vector3.Dot(Dev, normal) * Dev;
            normal = normal.normalized;

            /*Vector3 Dev1 = (guide.vBuffer[1] - guide.vBuffer[0]).normalized; 
            Vector3 Perp1 = Vector3.Cross(Dev1, guide.firstNormal).normalized;
            Perp1 = Perp1 * guide.rotCosF + Dev1 * guide.rotSinF;
             
            Vector3 Dev2 = (guide.vBuffer[3] - guide.vBuffer[2]).normalized;  
            Vector3 Perp2 = Vector3.Cross(Dev2, guide.lastNormal).normalized;
            Perp2 = Perp2 * guide.rotCosL + Dev2 * guide.rotSinL;
              
            Vector3 Perp = ((T1 + T2) * Perp1 + (T3 + T4) * Perp2);
            
            Vector3 normal = Vector3.Cross(Perp, Dev).normalized;*/

            return normal;
            
        }
         
        public Vector3 EvalUV(CPNGuide guide)
        {
            if (guide.GetN() == -1)
            {
                return guide.uvsBuffer[this.polylineIndex] * T1 + guide.uvsBuffer[this.polylineIndex + 1] * T2;
            }
            Vector3[] uvsBuffer = guide.uvsBuffer;
            return (uvsBuffer[0] * T1 + uvsBuffer[1] * T2 + uvsBuffer[2] * T3 + uvsBuffer[3] * T4);
        }

        public Vector3 EvalProperty(CPNGuide guide,int index)
        {
            if (guide.GetN() == -1)
            {
                return guide.propertiesBuffer[index][this.polylineIndex] * T1 + 
                    guide.propertiesBuffer[index][this.polylineIndex + 1] * T2;
            }
            Vector3[] b = guide.propertiesBuffer[index];
            return (b[0] * T1 + b[1] * T2 + b[2] * T3 + b[3] * T4);
        }

        public Vector3 EvalUVDev(CPNGuide guide)
        {
            if (guide.GetN() == -1)
            {
                return guide.uvsBuffer[this.polylineIndex + 1] - guide.uvsBuffer[this.polylineIndex];
            }
             
            float tp = t + 0.001f;
            float tmp = 1 - tp;

            float T1p = tmp * tmp * tmp;
            float T2p = 3 * tp * tmp * tmp;
            float T3p = 3 * tp * tp * tmp;
            float T4p = tp * tp * tp;

            Vector3[] uvsBuffer = guide.uvsBuffer;
            Vector3 F1 = (uvsBuffer[0] * T1 + uvsBuffer[1] * T2 + uvsBuffer[2] * T3 + uvsBuffer[3] * T4);
            Vector3 F2 = (uvsBuffer[0] * T1p + uvsBuffer[1] * T2p + uvsBuffer[2] * T3p + uvsBuffer[3] * T4p);

            return (F2 - F1) * 1000;
        }


        public Vector3 EvalNormal(CPNGuide guide)
        {
            if (guide.GetN() == -1)
            {
                return guide.vBuffer[this.polylineIndex] * T1 + guide.vBuffer[this.polylineIndex + 1] * T2;
            }
            Vector3 Dev = EvalDev(guide);

            return EvalNormal(guide, Dev);
        }
        public Vector3 EvalNormal(CPNGuide guide,out Vector3 Dev)
        {
            if (guide.GetN() == -1)
            {
                Dev = guide.vBuffer[this.polylineIndex + 1] - guide.vBuffer[this.polylineIndex];
                return guide.vBuffer[this.polylineIndex] * T1 + guide.vBuffer[this.polylineIndex + 1] * T2;
            }
            Dev = EvalDev(guide);

            return EvalNormal(guide, Dev);
        }

        private int sideEdgeIndex;
        private float direction;

        public float EvalAt(float t, CPNSideEdge sideEdge)
        {
            int N = sideEdge.guide.Length;
            float index = t * N;
            this.sideEdgeIndex = (int)index;
            if (sideEdgeIndex < 0)
                sideEdgeIndex = 0;
            if (sideEdgeIndex >= N)
                sideEdgeIndex = N - 1;
            t = index - this.sideEdgeIndex;

            /*if(sideEdgeIndex<0 || sideEdgeIndex>=sideEdge.direct.Length)
                Debug.Log("N " + N + " sideEdgeIndex:" + sideEdgeIndex + " sideEdge.guide.Length:" + sideEdge.guide.Length 
                    + " this.sideEdgeIndex:"+ this.sideEdgeIndex+" t:"+t);*/
            bool direct = sideEdge.direct[sideEdgeIndex];
            CPNGuide guide = sideEdge.guide[sideEdgeIndex];

            if (!direct)
                t = 1 - t;
            this.direction = direct ? 1 : -1;
            float updatedT = EvalAt(t, guide);
            updatedT = direct ? updatedT : 1 - updatedT;

            float sideEdgeT = sideEdge.position[sideEdgeIndex] + updatedT *sideEdge.size[sideEdgeIndex];

            return sideEdgeT;
        }
        

        public Vector3 EvalVertex(CPNSideEdge sideEdge)
        {
            return EvalVertex(sideEdge.guide[sideEdgeIndex]); 
        }

        public Vector3 EvalDev(CPNSideEdge sideEdge)
        {
            return direction * EvalDev(sideEdge.guide[sideEdgeIndex]);
        }
         
        

        public Vector3 EvalUV(CPNSideEdge sideEdge)
        {
            return EvalUV(sideEdge.guide[sideEdgeIndex]);
        }

        public Vector3 EvalProperty(CPNSideEdge sideEdge,int property)
        {
            return EvalProperty(sideEdge.guide[sideEdgeIndex], property);
        }

        public Vector3 EvalNormal(CPNSideEdge sideEdge)
        {
            return EvalNormal(sideEdge.guide[sideEdgeIndex]);
        }

        public Vector3 EvalNormal(CPNSideEdge sideEdge,Vector3 Dev)
        {
            return EvalNormal(sideEdge.guide[sideEdgeIndex], Dev * direction);
        }


        public Vector3 EvalAxis(CPNSideEdge sideEdge)
        {
            return EvalAxis(sideEdge.guide[sideEdgeIndex]) * direction;//E' da verificare, però SI
        }




        public int GetCurveTessellationSteps(int edgeLength, short[] edge_, short[] edgeHints,
            int[] edgesProfile, int edgeProfileIndex)
        {
            
            int edgeHintsIndex = (edgeProfileIndex) << 1;

            int N1 = CPNGuide_loqs[edgeHints[edgeHintsIndex + 0]];
            int N2 = CPNGuide_loqs[edgeHints[edgeHintsIndex + 1]];

            if (N1 + N2 == 0)
            {
                edgesProfile[((edgeProfileIndex) << 2) + 3] = 0;
                return 0;
            }

            if (edgeLength == 2)
            {
                edgesProfile[((edgeProfileIndex) << 2) + 3] = (short)1;
                return 1;
            }

            int count = (2 * (N1 * N2) / (N1 + N2));

            if (count * (N1 + N2) < (2 * (N1 * N2)))
                count++;

            edgesProfile[((edgeProfileIndex) << 2) + 3] = (short)count;

            return count;
        }
         
        private void FillBuffer(Vector3[] buffer, Vector3[] inputBuffer, short[] edge, int edgeIndex)
        { 
            buffer[0] = inputBuffer[edge[edgeIndex + 0]];
            buffer[1] = inputBuffer[edge[edgeIndex + 1]];
            buffer[2] = inputBuffer[edge[edgeIndex + 2]];
            buffer[3] = inputBuffer[edge[edgeIndex + 3]];
        }

        private void FillBufferWithLine(Vector3[] buffer, Vector3[] inputBuffer, short[] edge, int edgeIndex)
        {
            Vector3 A = inputBuffer[edge[edgeIndex + 0]];
            Vector3 B = inputBuffer[edge[edgeIndex + 1]];
            buffer[0] = A;
            buffer[1] = (A + A + B) * ONE_THIRD;
            buffer[2] = (A + B + B) * ONE_THIRD;
            buffer[3] = B;
        }

        private Vector3[] CreateBufferWithIndices(Vector3[] buffer, Vector3[] inputBuffer, int[] indices)
        {
            Vector3[] output = new Vector3[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                output[i] = inputBuffer[indices[i]];
            }
            return output;
        }

        private Vector3[] CreateNormalBufferWithIndices(Vector3[] buffer, Vector3[] inputBuffer, int[] indices,
            int verticesCount,int startOfPrecomputedData)
        {
            Vector3[] output = new Vector3[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                index = index > verticesCount ? (index - startOfPrecomputedData + verticesCount) : index;
                output[i] = inputBuffer[index];
            }
            return output;
        }


        public void EvaluatePolyline(CurvedPolygonsNet net, OutputMesh mesh, CPNGuide guide)
        {

            bool uvAvailable = net.GetUv() != null && net.GetUv().Length != 0 && mesh.DoUseUVs();
            int countP = mesh.CountProperties();
            //bool tgAvailable = net.GetTangents() != null && net.GetTangents().Length != 0;

            short[] tessellationDegrees = CPNGuide_loqs;

            guide.vBuffer= CreateBufferWithIndices(guide.vBuffer, net.GetVertices(), guide.GetIndices());
            guide.nBuffer = CreateBufferWithIndices(guide.vBuffer, net.GetNormals(), guide.GetIndices());
            if (uvAvailable)
                guide.uvsBuffer = CreateBufferWithIndices(guide.uvsBuffer, net.GetUv(), guide.GetIndices());
            if (countP > 0) {
                guide.PreparePropertiesBuffers(countP);
                for (int k=0;k<countP;k++)
                {
                    guide.propertiesBuffer[k] = CreateBufferWithIndices(guide.propertiesBuffer[k],
                        net.GetProperties3()[k], guide.GetIndices());
                }
            }
        }

        public void EvaluateEdge(CurvedPolygonsNet net, OutputMesh mesh, CPNGuide guide,
                short edgeLength, short[] edge, int edgeIndex, short[] edgeHints, 
                float[] edgeWeights, int[] edgeProfile, int realEdgeIndex)
        {
            
            bool isLinear = edgeLength == 2;

            bool uvAvailable = net.GetUv() != null && net.GetUv().Length != 0 && mesh.DoUseUVs();
            int countP = mesh.CountProperties();

            short[] tessellationDegrees = CPNGuide_loqs;

            if (isLinear)
            {
                FillBufferWithLine(guide.vBuffer, net.GetVertices(), edge, edgeIndex);
                if (uvAvailable)
                    FillBufferWithLine(guide.uvsBuffer, net.GetUv(), edge, edgeIndex); 
                guide.PreparePropertiesBuffers(countP);
                for (int k = 0; k < countP; k++)
                {
                    FillBufferWithLine(guide.propertiesBuffer[k],net.GetProperties3()[k], edge, edgeIndex);
                } 
            } else { 
                FillBuffer(guide.vBuffer, net.GetVertices(), edge, edgeIndex);
                if (uvAvailable)
                    FillBuffer(guide.uvsBuffer, net.GetUv(), edge, edgeIndex);
                guide.PreparePropertiesBuffers(countP);
                for (int k = 0; k < countP; k++)
                {
                    FillBuffer(guide.propertiesBuffer[k], net.GetProperties3()[k], edge, edgeIndex);
                }
            }
            
            //if (tgAvailable)  //Avoid tangents for now
            //    FillBuffer(linear, polyline.tgsBuffer, net.GetTangents(), edge, edgeIndex);

            int handleIndex = (realEdgeIndex) << 1;
            guide.w2 = edgeWeights[handleIndex + 0];
            guide.w3 = edgeWeights[handleIndex + 1];
             

            guide.edgeNormal = net.GetNormals()[net.GetNumberOfVertices() + realEdgeIndex];
            guide.firstNormal = net.GetNormals()[edge[edgeIndex + 0]];
            guide.lastNormal = net.GetNormals()[edge[edgeIndex + (isLinear? 1 : 3)]];

            //float r = edgeRots == null ? 0 : edgeRots[edgeProfileIndex];
            //float r3 = edgeRots == null ? 0 : edgeRots[handleIndex + 1];

            int N1 = 1;
            int N2 = 1;

            if (!isLinear) { 
                N1 = tessellationDegrees[edgeHints[handleIndex + 0]];
                N2 = tessellationDegrees[edgeHints[handleIndex + 1]];
            }

            int N = guide.GetN();

            float c = (2.0f * N1 * N2) / (N * (N1 + N2));
            guide.tessellationStepA = c / N1;
            guide.tessellationStepB = c * 0.5f * (N1 - N2) / (N1 * N2 * N * 1.0f);
            
            int edgeInternal = edgeProfile[((realEdgeIndex) << 2) + 2];
             
            float step = 1.0f / N; 
            for (int j = 1; j < N; j++)
            {
                int index = edgeInternal + j - 1;
                EvalAt(j * step, guide);
                mesh.SetVertex(index, EvalVertex(guide));
                if (uvAvailable)
                {
                    mesh.SetUV(index, EvalUV(guide));
                }
                for (int k = 0; k < countP; k++) {
                    mesh.SetProperty3(index, k,EvalProperty(guide, k));
                } 
            }
             
        }
         
        public void EvaluateNormals(OutputMesh mesh, CPNGuide guide) {
 
            int N = guide.GetN();

            if (N <= 0)
                return;

            bool doTangents = mesh.DoUseTangents();
            bool doNormals = mesh.DoNormals();

            if (doNormals) {

                float step = 1.0f / N;
                for (int j = 1; j < N; j++)
                {
                    EvalAt(j * step, guide);
                    Vector3 dev;
                    Vector3 normal = EvalNormal(guide, out dev);
                    int index = guide.GetIndex(j);
                    mesh.SetNormal(index, normal.normalized);
                    if (doTangents)
                    {
                        mesh.SetTangent(index, GetTangent(guide, dev, normal).normalized);
                    }
                }
                if (doTangents)
                {
                    for (int j = 0; j <= N; j += N)
                    {
                        EvalAt(j * step, guide);
                        Vector3 dev = EvalDev(guide);
                        int index = guide.GetIndex(j);
                        Vector3 normal = mesh.GetNormal(index);
                        mesh.SetTangent(index, GetTangent(guide, dev, normal).normalized);
                    }
                }
            }
        }

        private Vector3 GetTangent(CPNGuide guide,Vector3 dev,Vector3 normal) {
            Vector3 DCTdt = EvalUVDev(guide);
            Vector3 DCTds = Vector3.Cross(DCTdt, Vector3.forward);
            Vector3 DCds = Vector3.Cross(dev, normal);
            float det = DCTdt.x * DCTds.y - DCTdt.y * DCTds.x;
            Vector3 tangent = (dev * DCTds.y - DCds * DCTdt.y).normalized;
            return det > 0 ? tangent : -tangent;
        }
    }
}
