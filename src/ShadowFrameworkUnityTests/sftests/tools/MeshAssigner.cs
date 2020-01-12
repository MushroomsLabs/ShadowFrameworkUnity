using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;
using MLab.ShadowFramework;
using MLab.ShadowFramework;

class MeshAssigner
{

    public static void AssignMesh(GameObject gameObject, OutputMesh outputMesh) {
        AssignMesh(gameObject, outputMesh.GetVertices(), outputMesh.GetNormals(),
            outputMesh.GetUVs(), outputMesh.GetTriangles());
    }

    public static void AssignUV2(GameObject gameObject, Vector2[] uv2)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.sharedMesh.uv2 = uv2;
        }
    }

    public static void AssignMesh(GameObject gameObject,Vector3[] vertices_,Vector3[] normals_,
        Vector2[] uvs_,int[][] indices_) {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        { 
            Mesh m = new Mesh();
            m.vertices = (vertices_);
            m.normals = (normals_);
            m.uv = (uvs_);//so cool so cool so cool so cool 

            m.subMeshCount = indices_.Length;
            for (int i = 0; i < indices_.Length; i++) {
                m.SetTriangles(indices_[i], i);
            }
            //Debug.Log("mesh subMeshCount " + m.subMeshCount);
            meshFilter.sharedMesh = m;

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            Material[] materials = renderer.sharedMaterials;
            if (materials.Length != indices_.Length)
            {
                Material[] newMaterials = new Material[indices_.Length];
                for (int i = 0; i < materials.Length && i < newMaterials.Length; i++)
                {
                    newMaterials[i] = materials[i];
                }
                for (int i = materials.Length; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = materials[0];
                }
                renderer.sharedMaterials = newMaterials;
            }

        }
    }
} 
