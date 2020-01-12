using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Interpolation.GouraudSchema;

namespace MLab.ShadowFramework.Interpolation.GouraudSchema
{
    public class SFGouraudSchemaCornerSurface
    {  
        private SFGouraudInterpolationBuffer bufferA;
        private SFGouraudInterpolationBuffer bufferB;
         
        public void Set(SFGouraudInterpolationBuffer bufferA, SFGouraudInterpolationBuffer bufferB)
        {
            this.bufferA = bufferA;
            this.bufferB = bufferB; 
        }
         
        public Vector3 evalVertex(int Aindex, int Bindex)
        {
            int size = bufferA.N;

            int backBIndex = bufferB.N - Bindex;

            Vector3 v0 = bufferA.vertices[0];
            Vector3 vA = bufferA.vertices[Aindex];
            Vector3 vB = bufferB.vertices[backBIndex];

            return vA + vB - v0;
        }

        public Vector3 evalNormal(int Aindex, int Bindex)
        {
            int size = bufferA.N;

            int backBIndex = bufferB.N - Bindex;

            Vector3 v0 = bufferA.normals[0];
            Vector3 vA = bufferA.normals[Aindex];
            Vector3 vB = bufferB.normals[backBIndex];

            return vA + vB - v0;
        } 

        public Vector3 evalUV(int Aindex, int Bindex)
        {
            int backBIndex = bufferB.N - Bindex;

            Vector3 v0 = bufferA.uvs[0];
            Vector3 vA = bufferA.uvs[Aindex];
            Vector3 vB = bufferB.uvs[backBIndex];

            return vA + vB - v0;
        }

        public Vector3 evalProperty(int k,int Aindex, int Bindex)
        {
            int backBIndex = bufferB.N - Bindex;

            Vector3 v0 = bufferA.properties[k][0];
            Vector3 vA = bufferA.properties[k][Aindex];
            Vector3 vB = bufferB.properties[k][backBIndex];

            return vA + vB - v0;
        }
    }
}
