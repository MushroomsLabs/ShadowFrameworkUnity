using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using MLab.ShadowFramework;

namespace MLab.ShadowFramework
{
    public class OutputMesh 
    {
        private Vector3[] vertices;
        private Vector2[] uvs;
        private Vector3[] normals;
        private Vector3[] tangents;
        private int[][] indices;

        private Vector3[][] properties3 = new Vector3[0][]; 

        private bool doNormals = true;
        private bool doUvs = true;
        private bool doTangents = true;

        private int actualGeometry = 0;

        public OutputMesh()
        {
        }

        public OutputMesh(Vector3[] vertices, Vector2[] uvs, Vector3[] normals,
            int[][] indices)
        {
            this.tangents = null;
            this.vertices = vertices;
            this.uvs = uvs;
            this.normals = normals;
            this.indices = indices;
            doTangents = false;
        }

        public void SetupStructure(bool doNormals,bool doUvs,bool doTangents,int countP3) {
            this.doNormals = doNormals;
            this.doUvs = doUvs;
            this.doTangents = doTangents;
            this.properties3 = new Vector3[countP3][]; 
        }

        public void Build(int builtVerticesCount,int[] builtTrianglesCount)
        {
            this.vertices = new Vector3[builtVerticesCount];
            this.indices = new int[builtTrianglesCount.Length][];
            if (doUvs)
                this.uvs = new Vector2[builtVerticesCount];
            if(doNormals)
                this.normals = new Vector3[builtVerticesCount];
            if (doTangents)
                this.tangents = new Vector3[builtVerticesCount]; 
            for (int k = 0; k < properties3.Length; k++)
            {
                properties3[k] = new Vector3[builtVerticesCount];
            }
            for (int i = 0; i < builtTrianglesCount.Length; i++) {
                this.indices[i] = new int[builtTrianglesCount[i] * 3];
            }
        }

        public bool DoUseTangents() {
            return doTangents && doUvs && doNormals;
        }

        public bool DoNormals() {
            return doNormals;
        }
        
        public bool DoUseUVs() {
            return doUvs;
        }

        public int CountProperties() {
            return properties3.Length;
        }

        public Vector3[] GetProperty3(int index) {
            return properties3[index];
        }

        public Vector3[] GetProperty2(int index) {
            return properties3[index];
        }

        public Vector3[] GetVertices() {
            return vertices;
        }

        public Vector3[] GetNormals()
        {
            return normals;
        }

        public Vector2[] GetUVs()
        {
            return uvs;
        }

        public Vector3[] GetTangents()
        {
            return tangents;
        }

        public void SetGeometry(int index) {
            actualGeometry = index;
        }

        public void SetTangents(Vector3[] tangents) {
            this.tangents = tangents;
        }
        
        public int GetVerticesSize()
        {
            return vertices.Length;
        }

        public int GetTrianglesSize()
        {
            return indices.Length;
        }

        public int[][] GetTriangles()
        {
            return indices;
        } 


        public void SetVertex(int index, Vector3 value)
        {
            vertices[index] = value;
        }

        public void SetNormal(int index, Vector3 value)
        {
            normals[index] = value;
        }

        public void SetTangent(int index, Vector3 value)
        {
            tangents[index] = value;
        }

        public void SetUV(int index, Vector3 value)
        {
            uvs[index] = value;
        }


        public void SetPNUV(int index, Vector3 vertex,Vector3 normal,Vector3 uv,Vector3 tg)
        {
            vertices[index] = vertex;
            if(doNormals)
                normals[index] = normal;
            if (doUvs)
                uvs[index] = uv;
            if (doTangents)
                tangents[index] = tg;
        }


        public void SetProperty3(int index, int pIndex, Vector3 property)
        {
            properties3[pIndex][index] = property;
        }
         
        public Vector3 GetVertex( int index)
        {
            return vertices[index];
        }

        public Vector3 GetNormal(int index)
        {
            return normals[index];
        }

        public Vector3 GetUV(int index)
        {
            return uvs[index];
        }

        public OutputMesh GetNewCloneVariant() {
            OutputMesh oMesh = new OutputMesh();
            oMesh.vertices = new Vector3[this.vertices.Length];
            System.Buffer.BlockCopy(this.vertices, 0, oMesh.vertices, 0, this.vertices.Length);
            oMesh.normals = new Vector3[this.normals.Length];
            System.Buffer.BlockCopy(this.normals, 0, oMesh.normals, 0, this.normals.Length);
            oMesh.tangents = new Vector3[this.tangents.Length];
            System.Buffer.BlockCopy(this.tangents, 0, oMesh.tangents, 0, this.tangents.Length);
            oMesh.uvs = new Vector2[this.uvs.Length];
            System.Buffer.BlockCopy(this.uvs, 0, oMesh.uvs, 0, this.uvs.Length);
            oMesh.indices = indices;
            return oMesh;
        }

        public int WriteTriangle(int index, int a, int b, int c)
        {
            int idx = index * 3;
            indices[actualGeometry][idx] = a;
            indices[actualGeometry][idx+1] = b;
            indices[actualGeometry][idx+2] = c;
            
            return index + 1;
        }


        public int WriteQuad(int index, int a, int b, int c, int d)
        {
            /*
                d------c      d------c
                |   /  |      |  \   |
                |  /   |      |   \  |
                a ---- b  o   a ---- b  ?
            */
            if (doNormals)
            {

                Vector3 ax1 = vertices[a] + vertices[c];
                Vector3 ax2 = vertices[b] + vertices[d];
                Vector3 delta = ax1 - ax2;
                Vector3 n = normals[a] + normals[b] + normals[c] + normals[d];
                if (Vector3.Dot(n, delta) > 0)
                {
                    int idx = index * 3;
                    indices[actualGeometry][idx] = a;
                    indices[actualGeometry][idx + 1] = b;
                    indices[actualGeometry][idx + 2] = c;
                    indices[actualGeometry][idx + 3] = a;
                    indices[actualGeometry][idx + 4] = c;
                    indices[actualGeometry][idx + 5] = d;
                }
                else
                {
                    int idx = index * 3;
                    indices[actualGeometry][idx] = a;
                    indices[actualGeometry][idx + 1] = b;
                    indices[actualGeometry][idx + 2] = d;
                    indices[actualGeometry][idx + 3] = d;
                    indices[actualGeometry][idx + 4] = b;
                    indices[actualGeometry][idx + 5] = c;
                }
            }
            else {
                 
                int idx = index * 3;
                indices[actualGeometry][idx] = a;
                indices[actualGeometry][idx + 1] = b;
                indices[actualGeometry][idx + 2] = c;
                indices[actualGeometry][idx + 3] = a;
                indices[actualGeometry][idx + 4] = c;
                indices[actualGeometry][idx + 5] = d;
            }
             
            return index + 2;
        }
    }
}