using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace MLab.ShadowFramework.Interpolation
{
    public class CPNCornerSet {

#if DEBUG
        public static bool traceBuffersData = false;
        public static bool traceCornerData = false;
        public static bool applySecondOrderControl = true;
#endif
        
        public struct CurvedCorrection
        { 
            public Vector3 firstOrder;  
            public Vector3 secondOrder2;
            public float secondOrder2Factor;
            public float secondOrderCorrectorModulation;

            public CurvedCorrection(
               Vector3 N, Vector3 N0, Vector3 DB0, Vector3 P0, Vector3 P, Vector3 DA,
               Vector3 axis0, Vector3 axis)
            {
                secondOrder2 = Vector3.zero; 
                secondOrder2Factor = 0; 
                
                Vector3 perp0 = Vector3.Cross(N0, axis0).normalized;
                Vector3 perp = Vector3.Cross(N,axis).normalized;
                //Vector3 delta = perp1 - perp0; 
                float kA = Vector3.Dot(DB0, axis0);
                float kP = Vector3.Dot(DB0, perp0);

                Vector3 axisComponent = kA * axis;
                Vector3 perpComponent = kP * perp;
                 
                if (traceCornerData) {
                     //Debug.Log("P:" + P+ " P0:" + P0 + " N0: " + N0 + " axis0:" + axis0 + " DB0:" + DB0 + " perp0:" + perp0+" kA:"+kA+" kP:"+kP);
                }
                
                if (kP < 0) {
                    //Bad Obtuse angle 
                    float dot = Vector3.Dot(N, N0);
                    perpComponent = perpComponent * dot * dot * dot * dot * dot;
                } 
                 
                firstOrder = axisComponent + perpComponent - DB0;
                 
                //Second order correction will affect only. 
                secondOrderCorrectorModulation = kP * kP / (kP * kP + kA * kA); 
                 
                if (traceCornerData) {
                    //Debug.Log(" firstOrder "+firstOrder + 
                    //    " secondOrderCorrectorModulation:" + secondOrderCorrectorModulation);
                }

            }
              
        }

        private InterpolationBuffer bufferA; 
        private InterpolationBuffer bufferB;
          
        private Vector3 finalAB;
        private Vector3 finalBA;

        public CurvedCorrection[] cyA = new CurvedCorrection[8];
        public CurvedCorrection[] cyB = new CurvedCorrection[8];
          
        public void Set(InterpolationBuffer bufferA, InterpolationBuffer bufferB)
        { 
            this.bufferA = bufferA;
            this.bufferB = bufferB;
            int sizeA = bufferA.N;
            int sizeB = bufferB.N;
            if (cyA.Length < sizeA + 1)
            {
                cyA = new CurvedCorrection[sizeA + 1];
            }
            if (cyB.Length < sizeB + 1)
            {
                cyB = new CurvedCorrection[sizeB + 1];
            }

            Vector3 v0 = bufferA.vertices[0];
            Vector3 normal0 = bufferA.normals[0];
            Vector3 DA0 = bufferA.devFirst;
            Vector3 DB0 = -bufferB.devLast;

            for (int i = 0; i <= sizeA; i++)
            {
                cyA[i] = new CurvedCorrection(bufferA.normals[i], normal0, DB0, v0, bufferA.vertices[i],bufferA.devs[i],
                    bufferA.axis[0],bufferA.axis[i]);  
            }

            for (int backBIndex = 0; backBIndex <= sizeB; backBIndex++)
            {
                cyB[backBIndex] = new CurvedCorrection(bufferB.normals[backBIndex], normal0, DA0, v0, 
                    bufferB.vertices[backBIndex], bufferB.devs[backBIndex],
                    -bufferB.axis[sizeB], -bufferB.axis[backBIndex]); 
            }

             
            Vector3 finalA = bufferA.vertices[bufferA.N];
            Vector3 finalNA = bufferA.normals[bufferA.N];
            
            Vector3 finalB = bufferB.vertices[0];
            Vector3 finalNB = bufferB.normals[0];

            finalAB = (finalB - finalA) * 0.333333f;
            finalBA = -finalAB;

            finalAB = finalAB - Vector3.Dot(finalAB, finalNA) * finalNA;
            finalBA = finalBA - Vector3.Dot(finalBA, finalNB) * finalNB;

            float distance = Vector3.Distance(finalA, finalB) * 0.707107f;

            finalAB = finalAB * (0.58578643762f) * distance / finalAB.magnitude;
            finalBA = finalBA * (0.58578643762f) * distance / finalBA.magnitude;
            float weight = 0.80473785f;

            finalAB = finalAB + finalA;
            finalBA = finalBA + finalB;

            float t = 0;
            int index = 0;
            for (int i = 0; i <= sizeA; i++)
            {
                int Aindex = i;
                float U = bufferA.ts[Aindex];
                float V = 1 - U;//This is pretty important.
                Vector3 vA = bufferA.vertices[Aindex];
                GetInterpolation(1 - V, bufferB.N, out t, out index);
                Vector3 vB = bufferB.vertices[index] * (1 - t) + bufferB.vertices[index + 1] * (t);
                Vector3 firstOrder = cyA[Aindex].firstOrder;

                Vector3 finalPosition = vA + vB - v0 + firstOrder * V;

                Vector3 realFinalPosition = (U * U * U * finalA + 3 * U * U * V * weight * finalAB 
                    + 3 * U * V * V * weight * finalBA + V * V * V * finalB)/
                    (U * U * U  + 3 * U * U * V * weight  + 3 * U * V * V * weight  + V * V * V );

                cyA[Aindex].secondOrder2 = realFinalPosition - finalPosition;
                cyA[Aindex].secondOrder2 *= cyA[Aindex].secondOrderCorrectorModulation;
                cyA[Aindex].secondOrder2Factor = i == sizeA ? 0 : 1 / V;
            }


            for (int i = 0; i <= sizeB; i++)
            {
                int backBIndex = bufferB.N - i;
                float V = 1 - bufferB.ts[backBIndex];
                float U = 1 - V;
                Vector3 vB = bufferB.vertices[backBIndex];
                GetInterpolation(U, bufferA.N, out t, out index);
                Vector3 vA = bufferA.vertices[index] * (1 - t) + bufferA.vertices[index + 1] * (t); 
                Vector3 firstOrder = cyB[backBIndex].firstOrder;

                Vector3 finalPosition = vB + vA - v0 + firstOrder * U;

                Vector3 realFinalPosition = (U * U * U * finalA + 3 * U * U * V * weight * finalAB
                    + 3 * U * V * V * weight * finalBA + V * V * V * finalB) /
                    (U * U * U + 3 * U * U * V * weight + 3 * U * V * V * weight + V * V * V);

                cyB[backBIndex].secondOrder2 = realFinalPosition - finalPosition;
                cyB[backBIndex].secondOrder2 *= cyB[backBIndex].secondOrderCorrectorModulation;
                cyB[backBIndex].secondOrder2Factor = i == sizeB ? 0 : 1 / U;
            }

        }

        private void GetInterpolation(float U, int N,out float t,out int index) {
            //force positive
            U = U < 0 ? 0 : U;
            U = U > 1 ? 1 : U;
            float T = U * N;
            index = (int)T;
            index = index >= N ? N - 1 : index; 
            t = T-index;
        }


        public Vector3 evalVertex(int Aindex, int Bindex )
        { 
            int size = bufferA.N;

            int backBIndex = bufferB.N - Bindex;

            Vector3 v0 = bufferA.vertices[0];
            Vector3 vA = bufferA.vertices[Aindex];
            Vector3 vB = bufferB.vertices[backBIndex];
            
            float U = bufferA.ts[Aindex];
            float V = 1 - bufferB.ts[backBIndex];

            Vector3 DB = vB - v0;
            Vector3 DA = vA - v0;
             
            float U2 = U * U;
            float V2 = V * V;
             
#if DEBUG 
            if (TriangleInterpolator4.interpolationCornerSide == 3)
            { 
                return vA + vB - v0;
            }
#endif

            //float secondOrderFactorV = secondOrderFunction(getFV(V, U));
            //float secondOrderFactorU = secondOrderFunction(getFV(U, V));

            float secondOrderFactorV = secondOrderFunction(V, U);
            float secondOrderFactorU = secondOrderFunction(U, V);

            Vector3 part1, part2;

            if (applySecondOrderControl)
            {
                Vector3 rotatedDB = DB + cyA[Aindex].firstOrder * V +
                cyA[Aindex].secondOrder2 * secondOrderFactorV;
                Vector3 rotatedDA = DA + cyB[backBIndex].firstOrder * U +
                    cyB[backBIndex].secondOrder2 * secondOrderFactorU;
                part1 = vA + bufferA.thickness * rotatedDB;
                part2 = vB + bufferB.thickness * rotatedDA;
            } else {
                Vector3 rotatedDB = DB + cyA[Aindex].firstOrder * V;
                Vector3 rotatedDA = DA + cyB[backBIndex].firstOrder * U;
                part1 = vA + bufferA.thickness * rotatedDB;
                part2 = vB + bufferB.thickness * rotatedDA;
            }
             
#if DEBUG
            if (TriangleInterpolator4.interpolationCornerSide == 1) {
                return part1;
            }
            if (TriangleInterpolator4.interpolationCornerSide == 2)
            {
                return part2;
            }
#endif

            //Voglio che il second order 

            return (part1 * (U2) + part2 * (V2)) / (U2 + V2);
        }

        private float secondOrderFunction(float V, float U )
        {

            float D = 0.4f;
            float gamma = 1 - 2 * D;
            float beta = -D - 2 * gamma;

            if (U + V <= 1)
            {
                float v =  V / (1 - U);
                return v * v * (1.7f - v * 1.2f + v * v * 0.5f);
            }
            else {
                float v = 1 + D;
                if (V < 2 * (1 - U))
                {
                    float l = (V - (1 - U)) / (1 - U);
                    v = l + beta * l * l + gamma * l * l * l;
                    return 1.0f + v * 1.8f + 0.2f * v * v;
                }
                return v;
            }
            
        }


        private float getFV(float V, float U) {

            float D = 0.4f;
            float gamma = 1 - 2 * D;
            float beta = -D - 2 * gamma;

            if (U + V <= 1)
            {
                return V / (1 - U);
            }
            else if (V < 2 * (1 - U))
            {
                float l = (V - (1 - U)) / (1 - U);
                return  1 + l + beta * l * l + gamma * l * l * l;
            }
            else
            {
                return 1 + D;
            } 
        }



        private float secondOrderFunction(float v) {
            
            if (v <= 1)
                return v * v * (1.7f - v * 1.2f + v * v * 0.5f);
            else {
                v -= 1; 
                return 1.0f + v * 1.8f + 0.2f * v * v; 
            }
            
            //1.2\cdot x\cdot x\ -0.2\cdot x\cdot x\cdot x-4 * (x - 1) * (x - 1) * (x - 1)
        }

        public Vector3 evalUV( int Aindex, int Bindex )
        { 
            int backBIndex = bufferB.N - Bindex;
             
            Vector3 v0 = bufferA.uvs[0];
            Vector3 vA = bufferA.uvs[Aindex];
            Vector3 vB = bufferB.uvs[backBIndex];

            return vA + vB - v0;
        }
    }
}
