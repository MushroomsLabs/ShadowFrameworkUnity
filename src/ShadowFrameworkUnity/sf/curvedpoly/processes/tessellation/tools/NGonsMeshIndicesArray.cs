using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    
    class NGonsMeshIndicesArray : IMeshIndicesArray
    {

        int first;
        int move_;
        int count_;
        OutputMesh source;
        int layer;
        int last;
        int position = 0;

        public NGonsMeshIndicesArray()
        {
            // TODO Auto-generated constructor stub
        }

        public NGonsMeshIndicesArray(int first, int move, int count, int last, OutputMesh builder, int layer)
        {

            this.first = first;
            this.move_ = move;
            this.count_ = count;
            this.last = last;
            this.source = builder;
            this.layer = layer;
            position = 0;
        }

        public void Setup(int first, int move, int count, int last, OutputMesh builder, int layer)
        {
            this.first = first;
            this.move_ = move;
            this.count_ = count;
            this.last = last;
            this.source = builder;
            this.layer = layer;
            position = 0;
        }

        public int Count()
        {
            return count_;
        }

        public int GetIndex()
        {
            if (position < count_ - 1)
                return first + move_ * position;
            else
                return last;
        }

        public int GetAtIndex(int index)
        {
            if (index < count_ - 1)
                return first + move_ * index;
            else
                return last;
        }

        public int GetNext()
        {
            if (position < count_ - 2)
                return first + move_ * position + move_;
            else
                return last;
        }

        public Vector3 GetNextValue()
        {
            return source.GetVertex(GetNext());
        }

        public Vector3 GetValue()
        {
            return source.GetVertex(GetIndex());
        }

        public void Move()
        {
            position++;
        }


    }
}