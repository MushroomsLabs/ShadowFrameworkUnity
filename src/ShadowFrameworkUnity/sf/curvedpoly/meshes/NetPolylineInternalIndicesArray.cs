using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    class NetPolylineInternalIndicesArray : IMeshIndicesArray
    {
        CPNSideEdge sideEdge;
        OutputMesh builder; 
        int position; 

        public NetPolylineInternalIndicesArray(CPNSideEdge sideEdge, OutputMesh builder)
        {
            this.sideEdge = sideEdge;
            this.builder = builder;
            this.position = 0; 
        } 

        public int Count()
        {
            return sideEdge.GetN() - 1;
        }

        public int GetIndex()
        { 
            return sideEdge.GetIndex(position + 1);
        }

        public int GetAtIndex(int index)
        { 
            return sideEdge.GetIndex(index + 1);
        }

        public int GetNext()
        { 
            return sideEdge.GetIndex(position + 2);
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