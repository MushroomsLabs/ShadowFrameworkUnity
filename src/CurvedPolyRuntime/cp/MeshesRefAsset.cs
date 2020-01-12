using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;

namespace MLab.CurvedPoly
{
    public class MeshesRefAsset : ScriptableObject
    { 
        public Mesh[] meshes;

        public int Count()
        {
            return meshes.Length;
        }

        public Mesh GetMesh(int index)
        {
            index = index % (meshes.Length);
            return meshes[index];
        }

        public Mesh GetMeshOrNull(int index)
        {
            if (index < 0 || index >= meshes.Length)
                return null;
            return meshes[index];
        }
    }
}
