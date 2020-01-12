using System;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{  

    public struct CPIMatrix
    {

        private Vector3 perp0;
        private Vector3 delta;
         
        public CPIMatrix(/*Vector3 NA, Vector3 devA,Vector3 DB0*/
        Vector3 N, Vector3 N0, Vector3 DB0)
        {

            Vector3 dev = N;
            Vector3 dev0 = N0;
            Vector3 RotationAxis = Vector3.Cross(N0, N).normalized;
            if (RotationAxis == Vector3.zero) {
                perp0 = Vector3.zero;
                delta = Vector3.zero;
                return;
            }

            perp0 = Vector3.Cross(RotationAxis, N0).normalized;
            Vector3 perp1 = Vector3.Cross(RotationAxis, N).normalized;
            delta = perp1 - perp0;

            return; 
        }

        public Vector3 Rotate(Vector3 v)
        {
            float k = Vector3.Dot(v, perp0);
            return v + k * delta;
        }
         
    }

    class MatricesBuffer
    {
        public CPIMatrix[] cyForward = new CPIMatrix[8];
        public CPIMatrix[] cyBackward = new CPIMatrix[8];
        
        public void requestSize(int size)
        {
            if (cyForward.Length < size)
            { 
                cyForward = new CPIMatrix[size];
                cyBackward = new CPIMatrix[size];
            } 
        }

        public void writeWithInterpolationBuffer(InterpolationBuffer buffer, Vector3 prevDevOut,
            Vector3 postDevOut) {

            requestSize(buffer.N + 1);

            for (int i = 0; i <= buffer.N; i++) {
                //cyForward[i] = new CPIMatrix(buffer.normals[i], buffer.devs[i], prevDevOut);
                //cyBackward[i] = new CPIMatrix(buffer.normals[i], buffer.devs[i], postDevOut);
                cyForward[i] = new CPIMatrix(buffer.normals[i], buffer.normals[0], prevDevOut);
                cyBackward[i] = new CPIMatrix(buffer.normals[i], buffer.normals[buffer.N], postDevOut);
            }
        }

    }
}
