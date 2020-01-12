using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{
    public class CPNSideEdge {
        public CPNGuide[] guide;
        public bool[] direct;
        public float[] position;
        public float[] size;
        int N = 0;
        public bool skip;

        public void Set(CPNGuide guide, bool direct) {
            this.guide = new CPNGuide[] { guide };
            this.direct = new bool[] { direct };
            this.position = new float[] { 0 };
            this.size = new float[] { 1 };
            int n = guide.GetN();
            this.skip = n == 0;
            n = (n == -1) ? guide.GetIndices().Length - 1 : n;
            this.N = n;
        }

        public void Set(CPNGuide[] guide, bool[] direct)
        {
            this.guide = guide;
            this.direct = direct;
            this.N = 0;
            for (int i = 0; i < guide.Length; i++) {
                int n = guide[i].GetN();
                n = (n == -1) ? guide[i].GetIndices().Length - 1 : n;
                this.skip = this.skip || (n == 0);
                N += n;
            }
             
            this.position = new float[direct.Length];
            this.size = new float[direct.Length];
        }

        public int GetN() {
            return N;
        }

        public int GetIndex(int position) {
            int id = 0;
            int n = guide[id].GetN();
            n = (n == -1) ? guide[id].GetIndices().Length - 1 : n;
            while (n < position) {
                position -= n;
                id++;
                //n = guide[id].GetN();
            }
            if(this.direct[id])
                return guide[id].GetIndex(position);
            else
                return guide[id].GetBackIndex(position);
        }

        public int GetBackIndex(int position)
        {
            int id = guide.Length-1;
            int n = guide[id].GetN();
            n = (n == -1) ? guide[id].GetIndices().Length - 1 : n;
            while (n < position)
            {
                position -= n;
                id--;
                n = guide[id].GetN();
            }
            if (this.direct[id])
                return guide[id].GetBackIndex(position);
            else
                return guide[id].GetIndex(position);
        }

        public void GetTwoConsecutiveIndices(int index, int[] output)
        {
            output[0] = GetIndex(index);
            output[1] = GetIndex(index + 1);
        }

        public void SwitchDirection()
        {
            int half = this.guide.Length >> 1;
            for (int i = 0; i < half; i++)
            {
                CPNGuide tmp = guide[i];
                guide[i] = guide[guide.Length - i - 1];
                guide[guide.Length - i - 1] = tmp;
                bool tmpD = direct[i];
                direct[i] = direct[guide.Length - i - 1];
                direct[guide.Length - i - 1] = tmpD;
            }
            for (int i = 0; i < direct.Length; i++) {
                direct[i] = !direct[i];
            }
        }

        public int GetFirstVertex() {
            if (direct[0])
                return guide[0].GetFirstVertex();
            else
                return guide[0].GetLastVertex();
        }

        public int GetLastVertex()
        {
            if (direct[direct.Length-1])
                return guide[guide.Length-1].GetLastVertex();
            else
                return guide[guide.Length - 1].GetFirstVertex();
        }

        private float approximateGuideSize(CPNGuide guide) {

            CPNGuideEvaluator evaluator = new CPNGuideEvaluator();
            int STEPS = 8;
            float step = 1.0f / STEPS;
            Vector3 preV = guide.vBuffer[0];
            float distance = 0;
            for (int i = 1; i <= STEPS; i++) {
                evaluator.EvalAt(i * step,guide);
                Vector3 V = evaluator.EvalVertex(guide);
                distance += Vector3.Distance(V, preV);
                preV = V;
            }
            if (distance == 0) {
                distance = 0.001f;
            }
            return distance;
            
        }
          

        public void computeStepsSizes() {
            int size = position.Length;
            if (size == 1) {
                position[0] = 0;
                this.size[0] = 1;
                return;
            }

            float sum = 0;
            for (int i = 0; i < position.Length; i++)
            {
                this.size[i] = approximateGuideSize(this.guide[i]);
                sum += this.size[i]; 
            } 
            float recSum = 1.0f / sum;
            this.position[0] = 0;
            this.size[0] *= recSum; 
            for (int i = 1; i < position.Length; i++)
            {
                this.size[i] *= recSum;
                this.position[i] = this.position[i - 1] + this.size[i - 1]; 
            }
        }
         
    }

    public class CPNPolygon {
         
        public CPNSideEdge[] sideEdges;
        public int schemaIndex;
        public bool skip = false;

        /*Compute the relative size for side edge Matches*/
        public void computeSideEdgesSizes() {
            for (int i = 0; i < sideEdges.Length; i++) {
                sideEdges[i].computeStepsSizes();
            }
        }

        public void computeSkip()
        {
            this.skip = false;
            for (int i = 0; i < sideEdges.Length; i++)
            {
                this.skip = this.skip || sideEdges[i].skip;
            }
        }
    }
}