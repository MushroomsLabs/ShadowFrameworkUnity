using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    class QuadraticMeshIndicesArray : IMeshIndicesArray
    {


        int move_;
        int first;
        int basemove;
        int deltaMove;
        int count_;
        OutputMesh builder;
        int layer;
        int next = 0;
        int index = 0;

        public QuadraticMeshIndicesArray()
        {

        }

        public void Setup(int first, int move, int deltaMove, int count, OutputMesh builder, int layer)
        {
            this.deltaMove = deltaMove;
            this.basemove = move;
            this.move_ = move + deltaMove;
            this.count_ = count;
            this.builder = builder;
            this.layer = layer;
            index = first;
            next = first + this.move_;
            this.first = first;
        }

        public int Count()
        {
            return count_;
        }

        public int GetIndex()
        {
            return index;
        }

        public int GetNext()
        {
            return next;
        }

        public int GetAtIndex(int index) {
            return first + index * basemove + deltaMove*(((index)*(index+1))>>1);
        }

        public Vector3 GetNextValue()
        {
            return builder.GetVertex(GetNext());
        }

        public Vector3 GetValue()
        {
            return builder.GetVertex( GetIndex());
        }

        public void Move()
        {
            index = next;
            move_ += deltaMove;
            next += move_;
        }

    }
}