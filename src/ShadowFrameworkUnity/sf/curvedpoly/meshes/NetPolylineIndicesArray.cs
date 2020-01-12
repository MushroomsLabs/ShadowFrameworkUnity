using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    class NetPolylineIndicesArray : IMeshIndicesArray
    {
        CPNSideEdge guide; 
        OutputMesh builder; 
        int position;
        bool back;

        public NetPolylineIndicesArray(CPNSideEdge guide, OutputMesh builder)
        {
            this.guide = guide;
            this.builder = builder; 
            this.position = 0;
            this.back = false;
        }

        public NetPolylineIndicesArray(CPNSideEdge guide, OutputMesh builder,
                bool back)
        {
            this.guide = guide;
            this.builder = builder;
            this.position = 0;
            this.back = back;
        }

        public int Count()
        {
            return guide.GetN() + 1;
        }

        public int GetIndex()
        {
            if (back)
                return guide.GetBackIndex(position);
            return guide.GetIndex(position);
        }

        public int GetAtIndex(int index)
        {
            if (back)
                return guide.GetBackIndex(index);
            return guide.GetIndex(index);
        }

        public int GetNext()
        {
            if (back)
                return guide.GetBackIndex(position + 1);
            return guide.GetIndex(position + 1);
        }

        public Vector3 GetNextValue()
        {
            return builder.GetVertices()[GetNext()];
        }

        public Vector3 GetValue()
        {
            return builder.GetVertices()[GetIndex()];
        }

        public void Move()
        {
            position++;
        }
    }

}