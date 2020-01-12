using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace MLab.ShadowFramework.Interpolation
{
    public class CPNRotatingEdgeSurface
    { 
        private InterpolationBuffer buffer;
        private InterpolationBuffer prev;
        private InterpolationBuffer next;

        //private CPNormalsRotationMatrix[] BTransforms;
        //private CPNormalsRotationMatrix[] CTransforms;
        private CPDerivativeRotationMatrix[] BTransforms;
        private CPDerivativeRotationMatrix[] CTransforms;
        private float[] resize;

        public void Set(InterpolationBuffer buffer, InterpolationBuffer prev, InterpolationBuffer next)
        {
            this.buffer = buffer;
            this.prev = prev;
            this.next = next;

            //s0 = 1;
            //Normal and Vector at the beginnig of A (where B begins) 
            Vector3 A0 = buffer.vertices[0];
            Vector3 A0N = buffer.normals[0];
            //Normal and Vector at the end of A (A Final, where C begins)
            Vector3 AF = buffer.vertices[buffer.N];
            Vector3 AFN = buffer.normals[buffer.N];
            //Normal and Vector on the opposite end of B (B Final)
            Vector3 BNF = prev.normals[0];
            Vector3 BF = prev.vertices[0]; 
            //Normal and Vector on the opposite end of C (C Final)
            Vector3 CNF = next.normals[next.N];
            Vector3 CF = next.vertices[next.N];

            BTransforms = new CPDerivativeRotationMatrix[buffer.N + 1];
            CTransforms = new CPDerivativeRotationMatrix[buffer.N + 1];
            resize = new float[buffer.N+1];

           // Vector3 Ortho = AF - A0;
            //CPNormalsRotationMatrix BFRot = new CPNormalsRotationMatrix(A0N, BNF, Ortho);
            //CPNormalsRotationMatrix CFRot = new CPNormalsRotationMatrix(AFN,CNF, Ortho);

            float BResize = Vector3.Dot(buffer.devFirst.normalized, -prev.devLast.normalized);
            float CResize = Vector3.Dot(-buffer.devLast.normalized, next.devFirst.normalized);
            BResize = 1 - BResize * 0.5f;
            CResize = 1 - CResize * 0.5f;

            //Distance between BF and A0
            //float sizeB = Vector3.Magnitude(BF - A0);
            //Distance between CF and AF
            //float sizeC = Vector3.Magnitude(CF - AF);
            Vector3 DB0 = -prev.devLast.normalized;
            Vector3 DC0 = next.devFirst.normalized;
            float step = 1.0f / buffer.N;
            for (int i = 0; i <= buffer.N; i++)
            {
                Vector3 normal = buffer.normals[i];
                BTransforms[i] = new CPDerivativeRotationMatrix(normal, DB0);
                CTransforms[i] = new CPDerivativeRotationMatrix(normal, DC0);

                float t = i * step;
                float tm = 1 - t;
                //resize[i] = t * t * t + tm * tm * tm + 3 * t * tm * (tm * BResize + t * CResize);

                //Anche questo resize è inutile sai?
                resize[i] = t * t  + tm * tm  + 2 * t * tm * (BResize + CResize);

                resize[i] = 1;

                //Vector3 A = buffer.vertices[i];
                //Vector3 AB = BFRot.Rotate(A - A0) + BF;
                //Vector3 AC = CFRot.Rotate(A - AF) + CF;

                //Vector3 Aend = AB * (1 - t) + AC * t; 
                //float expectedSize = sizeB * (1 - t) + sizeC * t;
                //float size = Vector3.Magnitude(Aend - A);
                //resize[i] = size / expectedSize;
            }
        }


        public Vector3 evalVertex(int tIndex, int sIndex)
        {

            int backSIndex = prev.N - sIndex;

            Vector3 A = buffer.vertices[tIndex];
            Vector3 B = prev.vertices[backSIndex];
            Vector3 C = next.vertices[sIndex];

            Vector3 A0 = buffer.vertices[0];
            Vector3 AN = buffer.vertices[buffer.N];

            Vector3 DB = this.BTransforms[tIndex].Rotate(B - A0);
            Vector3 DC = this.CTransforms[tIndex].Rotate(C - AN);

            float t = buffer.ts[tIndex];
            return A + ((1 - t) * DB + DC * t) * resize[tIndex];

        }
         

        public Vector3 evalUV(int tIndex, int sIndex)
        { 
            int backSIndex = prev.N - sIndex;

            Vector3 A = buffer.uvs[tIndex];
            Vector3 B = prev.uvs[backSIndex];
            Vector3 C = next.uvs[sIndex];

            Vector3 A0 = buffer.uvs[0];
            Vector3 AN = buffer.uvs[buffer.N];

            float t = buffer.ts[tIndex];

            return A + (1 - t) * (B - A0) + t * (C - AN);
        }


        public Vector3 evalProperty(int k,int tIndex, int sIndex)
        {
            int backSIndex = prev.N - sIndex;

            Vector3 A = buffer.properties[k][tIndex];
            Vector3 B = prev.properties[k][backSIndex];
            Vector3 C = next.properties[k][sIndex];

            Vector3 A0 = buffer.properties[k][0];
            Vector3 AN = buffer.properties[k][buffer.N];

            float t = buffer.ts[tIndex];

            return A + (1 - t) * (B - A0) + t * (C - AN);
        }

    }

}
