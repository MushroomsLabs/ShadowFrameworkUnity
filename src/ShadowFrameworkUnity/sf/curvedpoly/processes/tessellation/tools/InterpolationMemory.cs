using System;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    public class InterpolationMemory {

        public Vector3[] vertices = new Vector3[8];
        public Vector3[] normals = new Vector3[8];
        public Vector3[] uv = new Vector3[8]; 
        private int size;
        private int countP;

        public int GetSize() {
            return size;
        } 

        public void requestSize(int size)
        {
            if (vertices.Length < size)
            {
                vertices = new Vector3[size];
                normals = new Vector3[size];
                uv = new Vector3[size];  
            }
            this.size = size;
        }

        public InterpolationMemory clone() {
            InterpolationMemory memory = new InterpolationMemory();
            memory.vertices = new Vector3[size];
            memory.uv = new Vector3[size];
            memory.normals = new Vector3[size];
            memory.size = size;
            for (int i = 0; i < size; i++)
            {
                memory.vertices[i] = vertices[i];
                memory.normals[i] = normals[i];
                memory.uv[i] = uv[i];
            } 
            return memory;
        }
    }
}
