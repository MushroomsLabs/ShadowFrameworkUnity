using System;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{

    /* Cylindric Interpolation Matrix */
    public struct CyIMatrix {

        private float xx, xy, xz;
        private float yx, yy, yz;
        private float zx, zy, zz;
        
        public CyIMatrix(Vector3 N, Vector3 DB0)
        {
            DB0 = DB0.normalized;

            //Remov the N component
            Vector3 correctedDB0 = DB0 - N * Vector3.Dot(N, DB0);

            Vector3 dev = correctedDB0;
            Vector3 dev0 = DB0;
            Vector3 Perp = Vector3.Cross(DB0, correctedDB0);
            if (Perp == Vector3.zero)
            {
                xx = 1; xy = 0; xz = 0;
                yx = 0; yy = 1; yz = 0;
                zx = 0; zy = 0; zz = 1;
                return;
            }
            Perp = Perp.normalized;

            //Subtract from dev its Perp component
            dev = dev - (Vector3.Dot(Perp, dev)) * Perp;
            //Renormalize dev
            dev = dev.normalized;

            //Check bad null case... 
            if (dev == Vector3.zero)
            {
                dev = dev0;
            }

            Vector3 dev0T = Vector3.Cross(dev0, Perp);
            Vector3 devT = Vector3.Cross(dev, Perp);

            xx = Perp.x * Perp.x + dev.x * dev0.x + devT.x * dev0T.x;
            xy = Perp.x * Perp.y + dev.x * dev0.y + devT.x * dev0T.y;
            xz = Perp.x * Perp.z + dev.x * dev0.z + devT.x * dev0T.z;

            yx = Perp.y * Perp.x + dev.y * dev0.x + devT.y * dev0T.x;
            yy = Perp.y * Perp.y + dev.y * dev0.y + devT.y * dev0T.y;
            yz = Perp.y * Perp.z + dev.y * dev0.z + devT.y * dev0T.z;

            zx = Perp.z * Perp.x + dev.z * dev0.x + devT.z * dev0T.x;
            zy = Perp.z * Perp.y + dev.z * dev0.y + devT.z * dev0T.y;
            zz = Perp.z * Perp.z + dev.z * dev0.z + devT.z * dev0T.z;
        }


        /*
        public CyIMatrix(Vector3 N0, Vector3 N)
        {
            Vector3 dev = N;
            Vector3 dev0 = N0;
            Vector3 Perp = Vector3.Cross(N0, N);
            if (Perp == Vector3.zero) {
                xx = 1; xy = 0; xz = 0;
                yx = 0; yy = 1; yz = 0;
                zx = 0; zy = 0; zz = 1;
                return;
            }
            Perp = Perp.normalized;

            //Subtract from dev its Perp component
            dev = dev - (Vector3.Dot(Perp, dev)) * Perp;
            //Renormalize dev
            dev = dev.normalized;

            //Check bad null case... 
            if (dev == Vector3.zero)
            {
                dev = dev0;
            }

            Vector3 dev0T = Vector3.Cross(dev0, Perp);
            Vector3 devT = Vector3.Cross(dev, Perp);

            xx = Perp.x * Perp.x + dev.x * dev0.x + devT.x * dev0T.x;
            xy = Perp.x * Perp.y + dev.x * dev0.y + devT.x * dev0T.y;
            xz = Perp.x * Perp.z + dev.x * dev0.z + devT.x * dev0T.z;

            yx = Perp.y * Perp.x + dev.y * dev0.x + devT.y * dev0T.x;
            yy = Perp.y * Perp.y + dev.y * dev0.y + devT.y * dev0T.y;
            yz = Perp.y * Perp.z + dev.y * dev0.z + devT.y * dev0T.z;

            zx = Perp.z * Perp.x + dev.z * dev0.x + devT.z * dev0T.x;
            zy = Perp.z * Perp.y + dev.z * dev0.y + devT.z * dev0T.y;
            zz = Perp.z * Perp.z + dev.z * dev0.z + devT.z * dev0T.z;
        }*/

        /**All vectors MUST be unit vector, so normalize them first.*/
        /*public CyIMatrix(Vector3 dev0, Vector3 Perp, Vector3 dev) {

            //Subtract from dev its Perp component
            dev = dev - (Vector3.Dot(Perp, dev)) * Perp;
            //Renormalize dev
            dev = dev.normalized;

            //Check bad null case... 
            if (dev == Vector3.zero) {
                dev = dev0;
            }

            Vector3 dev0T = Vector3.Cross(dev0,Perp);
            Vector3 devT = Vector3.Cross(dev, Perp);

            xx = Perp.x * Perp.x + dev.x * dev0.x + devT.x * dev0T.x;
            xy = Perp.x * Perp.y + dev.x * dev0.y + devT.x * dev0T.y;
            xz = Perp.x * Perp.z + dev.x * dev0.z + devT.x * dev0T.z;

            yx = Perp.y * Perp.x + dev.y * dev0.x + devT.y * dev0T.x;
            yy = Perp.y * Perp.y + dev.y * dev0.y + devT.y * dev0T.y;
            yz = Perp.y * Perp.z + dev.y * dev0.z + devT.y * dev0T.z;

            zx = Perp.z * Perp.x + dev.z * dev0.x + devT.z * dev0T.x;
            zy = Perp.z * Perp.y + dev.z * dev0.y + devT.z * dev0T.y;
            zz = Perp.z * Perp.z + dev.z * dev0.z + devT.z * dev0T.z;

            //Test
            Vector3 testA = Rotate(dev0);

            Vector3 testB = Rotate(Perp);

            Vector3 testC = Rotate(dev0T);

            int a = 0;
        }*/

        public Vector3 Rotate(Vector3 v) {
            return new Vector3(
                    xx * v.x + xy * v.y + xz * v.z,
                    yx * v.x + yy * v.y + yz * v.z,
                    zx * v.x + zy * v.y + zz * v.z
                );
        }
    }

    public class InterpolationBuffer {

        public static float DELTA = 0.01f;

        public float[] ts = new float[8];
        public Vector3[] vertices = new Vector3[8];
        public Vector3[] verticesDplus = new Vector3[8];
        public Vector3[] verticesDminus = new Vector3[8];
        public Vector3[] devs = new Vector3[8];
        public CyIMatrix[] cyForward = new CyIMatrix[8];
        public CyIMatrix[] cyBackward = new CyIMatrix[8];
        public Vector3[] axis = new Vector3[8];
        public Vector3 devFirst;
        public Vector3 devLast;

        public Vector3[] uvs = new Vector3[8];
        public Vector3[] normals = new Vector3[8];
        public Vector3[][] properties = new Vector3[0][];
        int countP = 0;

        public int N;
        public float step; 

        public float thickness = 1.0f;

        public void requestSize(int size) {
            if (vertices.Length < size) {
                vertices = new Vector3[size];
                verticesDplus = new Vector3[size];
                verticesDminus = new Vector3[size];
                uvs = new Vector3[size];
                devs = new Vector3[size];
                normals = new Vector3[size];
                cyForward = new CyIMatrix[size];
                cyBackward = new CyIMatrix[size];
                axis = new Vector3[size];
                ts = new float[size]; 
            }
            for (int k = 0; k < countP; k++)
            {
                if (properties[k] == null || properties[k].Length < size)
                    properties[k] = new Vector3[size];
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
                for (int k=0;k<countP;k++) {
                    properties[k][i] = evaluator.EvalProperty(guide, k);
                }
                if (i == 0)
                {
                    devFirst = dev;
                }
                if (i == N)
                {
                    devLast = dev;
                }
                evaluator.EvalAt(i * step + DELTA, guide);
                verticesDplus[i] = evaluator.EvalVertex(guide);
                evaluator.EvalAt(i * step - DELTA, guide);
                verticesDminus[i] = evaluator.EvalVertex(guide);
                normals[i] = evaluator.EvalNormal(guide, dev).normalized;
                axis[i] = evaluator.EvalAxis(guide);
                devs[i] = dev;
            }
             
        }



        public void writeWithGuideBack(CPNSideEdge guide, int N, OutputMesh mesh,
            CPNGuideEvaluator evaluator)
        {
            writeWithGuideBack(guide, N, 1.0f / N, mesh, evaluator);
        }

        public void writeWithGuideBack(CPNSideEdge guide, int N, float step, OutputMesh mesh,
            CPNGuideEvaluator evaluator)
        {
            this.step = step;
            requestSize(N + 1);

            //We should have only one thickness on multisided edge, you know?
            this.thickness = 1;

            for (int i = 0; i <= N; i++)
            {
                //This is the eval-back
                evaluator.EvalAt(1.0f - i * step, guide);
                Vector3 dev = evaluator.EvalDev(guide);
                vertices[i] = evaluator.EvalVertex(guide);
                if (i == 0)
                {
                    devFirst = dev;
                }
                if (i == N)
                {
                    devLast = dev;
                }
                evaluator.EvalAt(i * step + DELTA, guide);
                verticesDplus[i] = evaluator.EvalVertex(guide);
                evaluator.EvalAt(i * step - DELTA, guide);
                verticesDminus[i] = evaluator.EvalVertex(guide);
                normals[i] = evaluator.EvalNormal(guide, dev).normalized;
                uvs[i] = evaluator.EvalUV(guide);
                for (int k = 0; k < countP; k++)
                {
                    properties[k][i] = evaluator.EvalProperty(guide, k);
                }
            }

        }
    }
}
