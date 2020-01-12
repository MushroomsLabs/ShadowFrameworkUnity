using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    interface IMeshIndicesArray 
    {
        int Count();

        int GetIndex();

        int GetNext();

        int GetAtIndex(int index);

        void Move();

        Vector3 GetValue();

        Vector3 GetNextValue();

    }
}