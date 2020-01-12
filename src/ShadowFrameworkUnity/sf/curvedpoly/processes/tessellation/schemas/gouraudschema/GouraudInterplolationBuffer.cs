using System;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation.GouraudSchema
{ 

    public class SFGouraudInterpolationBuffer
    { 
        public static float DELTA = 0.01f;

        public float[] ts = new float[8];
        public Vector3[] vertices = new Vector3[8];
        public Vector3[] normals = new Vector3[8];
        public Vector3[] uvs = new Vector3[8];
        public Vector3[][] properties = new Vector3[0][];
        int countP = 0;

        public int N;
        public float step; 

        public float thickness = 1.0f;

        public void requestSize(int size) {
            if (vertices.Length < size) {
                vertices = new Vector3[size];
                uvs = new Vector3[size]; 
                normals = new Vector3[size]; 
                ts = new float[size];
            }
            for (int k = 0; k < countP; k++)
            {
                if (properties[k] == null || properties[k].Length < N + 1)
                    properties[k] = new Vector3[N + 1];
            }
            this.N = size - 1;
        }

        public void requestProperties(int countP)
        {
            if (this.properties.Length < countP)
            {
                properties = new Vector3[countP][];
            } 
            this.countP = countP;
        }


        public void writeWithGuide(CPNSideEdge guide, int N, OutputMesh mesh,
            CPNGuideEvaluator evaluator) {
            writeWithGuide(guide, N, 1.0f / N, mesh, evaluator);
        }

        public void writeWithGuide(CPNSideEdge guide, int N, float step, OutputMesh mesh,
            CPNGuideEvaluator evaluator)
        { 
            this.step = step;
            requestSize(N + 1);

            //We should have only one thickness on multisided edge, you know?
            this.thickness = 1;

            for (int i = 0; i <= N; i++)
            {
                ts[i] = evaluator.EvalAt(i * step, guide);
                Vector3 dev = evaluator.EvalDev(guide);
                vertices[i] = evaluator.EvalVertex(guide);
                uvs[i] = evaluator.EvalUV(guide); 
                normals[i] = evaluator.EvalNormal(guide, dev).normalized;
                for (int k = 0; k < countP; k++)
                {
                    properties[k][i] = evaluator.EvalProperty(guide, k);
                }
            }
             
        }
        
    }
}
