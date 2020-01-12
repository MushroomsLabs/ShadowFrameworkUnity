using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation{

    public class CPNGuide {

        int[] indices;
        int N=-1; 

        public Vector3 firstNormal, lastNormal; 
        public float tessellationStepA, tessellationStepB;
        public float w2, w3;
        //public float rotCosF,rotSinF, rotCosL, rotSinL, thickness;
        public Vector3 edgeNormal;
        //public Vector3 perpA, perpB;
        public Vector3[] vBuffer = new Vector3[4];
        public Vector3[] uvsBuffer = new Vector3[4];
        public Vector3[][] propertiesBuffer = new Vector3[0][];
        public Vector3[] nBuffer;

        public CPNGuide()
        { 
        }

        public void Clean()
        {
        }
        
        public int[] GetIndices()
        {
            return indices;
        }
         
        public CPNGuide(int[] profile, int profileIndex, int N)
        {
            this.N = N;
            if (N == 0) {
                this.indices = new int[0];
                return;
            }
            this.indices = new int[N + 1];
            int index = (profileIndex) << 2;
            int first = profile[index];
            int last = profile[index + 1];
            int internal_ = profile[index + 2];
            short n = (short)profile[index + 3];
            this.indices[0] = first;
            this.indices[0 + n] = last;
            for (int j = 1; j < n; j++)
            {
                this.indices[0 + j] = (short)(internal_ + j - 1);
            }
             
        }

        public void PreparePropertiesBuffers(int countP) {
            this.propertiesBuffer=new Vector3[countP][];
            for (int k = 0; k < countP; k++)
            {
                this.propertiesBuffer[k] = new Vector3[4];
            }
        }

        public void SetPolyline(int position,short[] indices,int count) {
            this.N = -1;
            this.indices = new int[count];
            for (int i=0;i<count;i++) {
                this.indices[i] = indices[position + i];
            }
        }
          

        /*
        public int WriteRotationAt(int position, float[] rots, int direction)
        {
            int rotLength = this.rotations.Length;
            if (direction > 0)
            {
                for (int i = 0; i < rotLength; i++)
                {
                    rots[position + i] = this.rotations[i];
                }
            }
            else
            {
                for (int i = 0; i < rotLength; i++)
                {
                    rots[position + i] = this.rotations[rotLength - 1 - i];
                }
            }
            return position + rotLength;
        }*/

        public static CPNGuide Build(int[] indices/*, float[] rots*/, 
            Vector3 suggestedStartDev, Vector3 suggestedEndDev,
            float tessellationStepA, float tessellationStepB)
        {
            CPNGuide built = new CPNGuide
            {
                indices = indices,
                N = indices.Length - 1,
                //rotations = rots, 
                //suggestedStartDev = suggestedStartDev,
                //suggestedEndDev = suggestedEndDev,
                tessellationStepA = tessellationStepA,
                tessellationStepB = tessellationStepB
            };
            return built;
        }
         

        public int GetFreeIndex(int index)
        {
            return indices[index];
        }

        public int GetIndex(int index)
        { 
            return indices[index];
        }

        public int GetBackIndex(int index)
        {
            int n = this.N == -1 ? this.indices.Length - 1 : this.N;
            return indices[n - index];
        }
         

        public int GetN()
        {
            return N;
        }
         
        public int GetFirstVertex()
        {
            return indices[0];
        }

        public int GetLastVertex()
        {
            int n = this.N == -1 ? this.indices.Length - 1 : this.N;
            return indices[n];
        } 
         
        public void GetTwoConsecutiveIndices(int index, int[] output)
        {
            output[0] = GetFreeIndex(index);
            output[1] = GetFreeIndex(index + 1);
        }
          
    }
    
}
