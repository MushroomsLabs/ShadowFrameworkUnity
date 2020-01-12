using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace MLab.ShadowFramework.Interpolation
{
    public class CPNEdgeSurface
    {
        public static float ShapeControlValue = 1.18f;

        private InterpolationBuffer buffer;
        private InterpolationBuffer prev;
        private InterpolationBuffer next;

        private Vector3[] firtsOrder;

        private float /*s0,*/ s1, s2, s3;

        public void Set(InterpolationBuffer buffer, InterpolationBuffer prev,InterpolationBuffer next)
        {
            this.buffer = buffer;
            this.prev = prev;
            this.next = next;
             
            //s0 = 1;
            Vector3 A0 = buffer.vertices[0];
            Vector3 AN = buffer.vertices[buffer.N];
            Vector3 B = prev.vertices[0];
            Vector3 C = next.vertices[next.N];
            //Vector3 B_AB = A0 - CPNGuideEvaluator.ONE_THIRD * prev.devLast;
            Vector3 B_BA = B + CPNGuideEvaluator.ONE_THIRD * prev.devFirst;
            //Vector3 C_AB = AN + CPNGuideEvaluator.ONE_THIRD * next.devFirst;
            Vector3 C_BA = C - CPNGuideEvaluator.ONE_THIRD * next.devLast;
            s1 = 3;
            s2 = 3;
            s3 = 1;
            //float distance0 = Vector3.Distance(A0, AN);
            float distance0 = Vector3.Dot(A0 - AN, A0 - AN);
            if (distance0 > 0)
            {
                //this.s1 = 1.5f + 1.5f * Vector3.Dot(B_AB - C_AB, B_AB - C_AB) / distance0;
                //this.s2 = 1.5f + 1.5f * (Vector3.Dot(B_BA - C_BA, B_BA - C_BA) / distance0);
                this.s3 = Vector3.Dot(B - C, B - C) / distance0;

                //this.s1 = 3.0f + 0.0f * Vector3.Dot(B_AB - C_AB, B_AB - C_AB) / distance0;
                this.s2 = (3f - ShapeControlValue) +
                    ShapeControlValue * (Vector3.Dot(B_BA - C_BA, B_BA - C_BA) / distance0);
            }

            firtsOrder = new Vector3[buffer.N + 1];
            Vector3 DBs = - prev.devLast;
            Vector3 DCs = next.devFirst;

            Vector3 N0 = buffer.normals[0];
            Vector3 NN = buffer.normals[buffer.N];
            Vector3 axis0 = buffer.axis[0];
            Vector3 axisN = buffer.axis[buffer.N];

            float step = buffer.step;

            //So: now the 'First order' is wrong.

            for (int i = 0; i < firtsOrder.Length; i++) {
                float t = i * step;
                Vector3 delta = (1 - t) * DBs + t * DCs;
                Vector3 A = buffer.vertices[i]; 
                Vector3 N = buffer.normals[i];
                Vector3 axis = buffer.axis[i];
                //firtsOrder[i] = getFirstOrder(N, N0, NN, DBs, DCs, axis0, axisN, axis, t); 

                //private Vector3 getFirstOrder(Vector3 N, Vector3 N0, Vector3 NN, Vector3 DB0, Vector3 DBN,
                //       Vector3 axis0, Vector3 axisN, Vector3 axis, float t)
                //{
                
                //Vector3 At = (1 - t) * A0 + t * AN;
                //Vector3 PA = (A - At) * size;
                //Vector3 PBC = (1 - t) * B + t * C;

                //if (CPNCornerSet.applySecondOrderControl)
                //    return PA + PBC + firstOrder * (1 - s) * (1 - s) * s /** (1 - s * s)+
                // ((1 - t) * B + t * C ) * (s * s)*/;

                Vector3 Delta = DBs * (1 - t) + DCs * t + (s1 - 3)*(A - A0 * (1 - t) - AN * t); 
                Vector3 CorrectDelta = Delta*buffer.thickness - Vector3.Dot(N, Delta) * N;
                firtsOrder[i] = CorrectDelta - Delta;
            }

        }
        

        public Vector3 evalVertex(int tIndex, int sIndex) {

            int backSIndex = prev.N - sIndex;

            Vector3 A = buffer.vertices[tIndex];
            Vector3 B = prev.vertices[backSIndex];
            Vector3 C = next.vertices[sIndex];
            
            Vector3 A0 = buffer.vertices[0];
            Vector3 AN = buffer.vertices[buffer.N];

            //Debug.Log("Verify "+ (A0-prev.vertices[prev.N])+" "+ (AN - next.vertices[0]));

            float t = buffer.ts[tIndex];
            float sB = 1 - prev.ts[backSIndex];
            float sC = prev.ts[sIndex]; 
            float s = (1 - t) * sB + t * sC;

            Vector3 firstOrder = firtsOrder[tIndex];

            float size = (1 - s) * (1 - s) * (1 - s) /* * s0[==1] */ +
            (s1 * (1 - s) * (1 - s) + s2 * s * (1 - s) + s3 * s * s) * s;

            Vector3 At = (1 - t) * A0 + t * AN;
            //Vector3 PA = A - At;
            Vector3 PA = (A - At) * size;
            Vector3 PBC = (1 - t) * B + t * C;
            
            //if (CPNCornerSet.applySecondOrderControl)
            //{
            return PA + PBC + firstOrder * (1 - s) * (1 - s) * s; /** (1 - s * s)+

            if (CPNCornerSet.applySecondOrderControl)
            {
                return A + (1 - t) * (B - A0) + t * (C - AN)
                 + firtsOrder[tIndex] * (1 - s) * (1 - s) * s;
            }
            else {
                return A + (1 - t) * (B - A0) + t * (C - AN);
            }*/
            
        }

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();

        public Vector3 GetFirstOrder(float t) {

            float T = t * buffer.N;
            int index = (int)T;
            index = index == buffer.N ? index - 1 : index;
            t = T - index;
            return (1 - t) * firtsOrder[index] + t * firtsOrder[index + 1];
        }

        public Vector3 evalVertex(int sIndex, CPNSideEdge sidedEdge, float t)
        {

            int backSIndex = prev.N - sIndex;

            evaluator.EvalAt(t, sidedEdge);
            Vector3 A = evaluator.EvalVertex(sidedEdge);
            Vector3 B = prev.vertices[backSIndex];
            Vector3 C = next.vertices[sIndex];

            Vector3 A0 = buffer.vertices[0];
            Vector3 AN = buffer.vertices[buffer.N];
              
            float sB = 1 - prev.ts[backSIndex];
            float sC = prev.ts[sIndex];
            float s = (1 - t) * sB + t * sC;

            float size = (1 - s) * (1 - s) * (1 - s) /* * s0[==1] */ +
            (s1 * (1 - s) * (1 - s) + s2 * s * (1 - s) + s3 * s * s) * s;

            if (!CPNCornerSet.applySecondOrderControl)
                size = 1;

            Vector3 firstOrder = GetFirstOrder(t);

            Vector3 At = (1 - t) * A0 + t * AN;
            //Vector3 PA = A - At;
            Vector3 PA = (A - At) * size;
            Vector3 PBC = (1 - t) * B + t * C;


            //if (CPNCornerSet.applySecondOrderControl)
            //{
                return PA + PBC + firstOrder * (1 - s) * (1 - s) * s /** (1 - s * s)+
                 ((1 - t) * B + t * C ) * (s * s)*/;
            //}
            //else
            //{
             //   return PA+PBC;
            //}

        } 

        public Vector3 evalUV(int tIndex, int sIndex) {

            int backSIndex = prev.N - sIndex;

            Vector3 A = buffer.uvs[tIndex];
            Vector3 B = prev.uvs[backSIndex];
            Vector3 C = next.uvs[sIndex];

            Vector3 A0 = buffer.uvs[0];
            Vector3 AN = buffer.uvs[buffer.N];

            float t = buffer.ts[tIndex];
               
            return A + (1 - t) * (B - A0) + t * (C - AN);
        }

        public Vector3 evalUV(int sIndex, CPNSideEdge sidedEdge, float t)
        { 
            int backSIndex = prev.N - sIndex;

            evaluator.EvalAt(t, sidedEdge);
            Vector3 A = evaluator.EvalUV(sidedEdge);
            Vector3 B = prev.uvs[backSIndex];
            Vector3 C = next.uvs[sIndex];

            Vector3 A0 = buffer.uvs[0];
            Vector3 AN = buffer.uvs[buffer.N];


            float sB = 1 - prev.ts[backSIndex];
            float sC = prev.ts[sIndex];
            float s = (1 - t) * sB + t * sC;

            float size = (1 - s) * (1 - s) * (1 - s) /* * s0[==1] */+
                (s1 * (1 - s) * (1 - s) + s2 * s * (1 - s) + s3 * s * s) * s;
            //float sB = 1 - prev.ts[backSIndex];
            //float sC = prev.ts[sIndex];
            //float s = (1 - t) * sB + t * sC;

            Vector3 At = (1 - t) * A0 + t * AN;
            //Vector3 PA = A - At;
            Vector3 PA = (A - At) * size;
            Vector3 PBC = (1 - t) * B + t * C;
            return PA + PBC;
                 
            //return A + (1 - t) * (B - A0) + t * (C - AN);
        }


        public Vector3 evalProperty(int k,int sIndex, CPNSideEdge sidedEdge, float t)
        {
            int backSIndex = prev.N - sIndex;

            evaluator.EvalAt(t, sidedEdge);
            Vector3 A = evaluator.EvalProperty(sidedEdge,k);
            Vector3 B = prev.properties[k][backSIndex];
            Vector3 C = next.properties[k][sIndex];

            Vector3 A0 = buffer.properties[k][0];
            Vector3 AN = buffer.properties[k][buffer.N];


            float sB = 1 - prev.ts[backSIndex];
            float sC = prev.ts[sIndex];
            float s = (1 - t) * sB + t * sC;

            float size = (1 - s) * (1 - s) * (1 - s) /* * s0[==1] */+
                (s1 * (1 - s) * (1 - s) + s2 * s * (1 - s) + s3 * s * s) * s;
            //float sB = 1 - prev.ts[backSIndex];
            //float sC = prev.ts[sIndex];
            //float s = (1 - t) * sB + t * sC;

            Vector3 At = (1 - t) * A0 + t * AN;
            //Vector3 PA = A - At;
            Vector3 PA = (A - At) * size;
            Vector3 PBC = (1 - t) * B + t * C;
            return PA + PBC;

            //return A + (1 - t) * (B - A0) + t * (C - AN);
        }


        private Vector3 getFirstOrder(Vector3 N, Vector3 N0, Vector3 NN, Vector3 DB0, Vector3 DBN,
               Vector3 axis0, Vector3 axisN, Vector3 axis, float t)
        {
            Vector3 Delta = DB0 * (1 - t) + DBN * t;

            Vector3 CorrectDelta = Delta - Vector3.Dot(N, Delta) * N;

            return CorrectDelta - Delta;

            /*
            Vector3 perp0 = Vector3.Cross(N0, axis0).normalized;
            Vector3 perpN = Vector3.Cross(NN, axisN).normalized;
            Vector3 perp = Vector3.Cross(N, axis).normalized;
            float kA0 = Vector3.Dot(DB0, axis0);
            float kP0 = Vector3.Dot(DB0, perp0);
            float kAN = Vector3.Dot(DBN, axisN);
            float kPN = Vector3.Dot(DBN, perpN);

            float kA = (1 - t) * kA0 + t * kAN;
            float kP = (1 - t) * kP0 + t * kPN;
            

            Debug.Log("getFirstOrder t:" + t+" kA:"+kA+" kP:"+kP+" ");
            Debug.Log("         kA0:" + kA0 + " kP0:" + kP0 + " (1-t):"+(1-t));
            Debug.Log("         kAN:" + kAN + " kPN:" + kPN + " (t):" + (t));

            Vector3 axisComponent = kA * axis;
            Vector3 perpComponent = kP * perp;
              
            if (kP < 0) { 
                float dot = Vector3.Dot(N, N0);
                perpComponent = perpComponent * dot * dot * dot * dot * dot;
            }

            Vector3 firstOrder = axisComponent + perpComponent - DB0;

            return firstOrder;
            */
        }

    }

}
