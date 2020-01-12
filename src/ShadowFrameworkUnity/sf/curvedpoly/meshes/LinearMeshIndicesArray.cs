using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    class LinearMeshIndicesArray : IMeshIndicesArray
    {

        int first;
        int move_;
        int count_;
        OutputMesh source; 
        int position;

        public LinearMeshIndicesArray(int first, int move, int count, OutputMesh builder)
        {

            this.first = first;
            this.move_ = move;
            this.count_ = count;
            this.source = builder;
            this.position = 0;
        }

        public int Count()
        {
            return count_;
        }

        public int GetIndex()
        {
            return first + move_ * position;
        }

        public int GetAtIndex(int index) {
            return first + move_ * index;
        }

        public int GetNext()
        {
            return first + move_ * position + move_;
        }

        public Vector3 GetNextValue()
        {
            return source.GetVertices()[GetNext()];
        }

        public Vector3 GetValue()
        {
            return source.GetVertices()[GetIndex()]; 
        }

        public void Move()
        {
            position++;
        }


    }
}