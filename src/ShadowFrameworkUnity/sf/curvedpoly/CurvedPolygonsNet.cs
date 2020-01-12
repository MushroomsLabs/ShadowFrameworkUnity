using UnityEngine;

namespace MLab.ShadowFramework
{
    public class CurvedPolygonsNet 
    {
        private Vector3[] vertices = new Vector3[0];
        private Vector3[] uv = new Vector3[0];
        private Vector3[] normals = new Vector3[0];
        private Vector3[] tangents = null; 
        private Vector3[][] property3 = new Vector3[0][]; 

        //numberOfVertices must contain, also, polylines and triangles and quads vertices
        int numberOfVertices; 
        int edgesCount; 
        short[] edges = new short[0];
        short[] edgesIndex = new short[1];
        short[] edgeHints = new short[0]; 
        float[] edgeWeights = new float[0];  

        //Polylines
        int polylinesCount;
        short[] polylines = new short[0];
        short[] polylinesIndex = new short[0];

        int geometriesCount;
        CPNGeometry[] geometries = new CPNGeometry[0];

        public CurvedPolygonsNet() { 
        }
        
           

        public void SetNumberOfVertices(int numberOfVertices)
        {
            this.numberOfVertices = numberOfVertices;
        }

        public void SetEdges(int totalEdgesCount, short[] edges, short[] edgesIndex, short[] edgeHints,
                /*float[] edgeRots, float[] edgeThickness,*/
                float[] edgeWeights)
        {
            /*//Fix possible null edgeRots or thinkesses
            if (edgeRots.Length == 0)
            {
                edgeRots = new float[edgesIndex.Length - 1];
            }
            if (edgeThickness.Length == 0) {
                edgeThickness = new float[edgesIndex.Length - 1];
                for (int i = 0; i < edgeThickness.Length; i++)
                {
                    edgeThickness[i] = 1.0f;
                }
            }*/

            this.edgesCount = totalEdgesCount; 
            this.edges = edges;
            this.edgesIndex = edgesIndex;
            this.edgeHints = edgeHints;
            //this.edgeRots = edgeRots;
            //this.edgeThickness = edgeThickness;
            this.edgeWeights = edgeWeights; 
        }

        public void setPolylines(int polylinesCount,short[] polylines,short[] polylinesIndex) {
            this.polylinesCount = polylinesCount; 
            this.polylines = polylines;
            this.polylinesIndex = polylinesIndex;
        }
        

        public void SetGeometries(int geometriesCount, CPNGeometry[] geometries)
        { 
            this.geometriesCount = geometriesCount;
            this.geometries = geometries;
        }

        public int GetNumberOfVertices()
        {
            return this.numberOfVertices;
        }

        public int GetEdgesCount()
        {
            return this.edgesCount;
        }

        public int GetPolylinesCount()
        {
            return this.polylinesCount;
        }

        public short[] GetEdges()
        {
            return this.edges;
        }

        public short[] GetPolylines()
        {
            return this.polylines;
        }

        public int GetEdgeLength(int index)
        {
            return edgesIndex[index + 1] - edgesIndex[index];
        }

        public short[] GetEdgesIndex()
        {
            return this.edgesIndex;
        }

        public short[] GetPolylinesIndex()
        {
            return this.polylinesIndex;
        }

        public int GetPolylineLength(int index)
        {
            return polylinesIndex[index + 1] - polylinesIndex[index];
        }
        
        public int GetEdgePosition(int index)
        {
            return edgesIndex[index];
        }

        public int GetPolylinePosition(int index)
        {
            return polylinesIndex[index];
        }
         
        public bool IsEdgeLinear(int index)
        {
            int size = edgesIndex[index + 1] - edgesIndex[index];
            return size == 2;
        }

        public short[] GetEdgeHints()
        {
            return edgeHints;
        }

        // public short[] getEdgeHints(int index) {
        // return edgeHints+2*index;
        // }

        public int GetEdgeHintsPosition(int index)
        {
            return 2 * index;
        }

        /*public float[] GetEdgesRots()
        {
            return edgeRots;
        }

        public float[] GetEdgesThickness()
        {
            return edgeThickness;
        }*/

        // public float[] getEdgeRots(int index) {
        // return edgeRots + 2 * index;
        // }

        public int GetEdgeRotsPosition(int index)
        {
            return index;
        }

        public float[] GetEdgeWeights()
        {
            return edgeWeights;
        }

        // public float[] getEdgeWeights(int index) {
        // return edgeWeights+2*index;
        // }
        public int GetEdgeWeightsPosition(int index)
        {
            return 2 * index;
        }

        public int GetGeometriesCount()
        {
            return geometriesCount;
        }

        public int GetPolygonsCount()
        {
            int polygonsCount = 0;
            for (int i = 0; i < geometriesCount; i++) {
                polygonsCount += geometries[i].polygonsCount;
            }
            return polygonsCount;
        }

        public int WriteVerticesNormals()
        {
            return geometriesCount;
        }

        public CPNGeometry[] GetGeometries()
        {
            return geometries;
        }

        public int GetTotalPolygonsCount()
        {
            int polygonsCount = 0;
            for (int i = 0; i < geometriesCount; i++)
            {
                polygonsCount += geometries[i].GetPolygonsCount();
            }
            return polygonsCount;
        }


        public Vector3[] GetVertices() {
            return vertices;
        }

        public void SetVertices(Vector3[] vertices) {
            this.vertices = vertices;
        }


        public Vector3[][] GetProperties3()
        {
            return property3;
        }

        public void SetProperty3(Vector3[][] property3)
        {
            this.property3 = property3;
        }
         


        public Vector3[] GetNormals()
        {
            return normals;
        }

        public void SetNormals(Vector3[] normals)
        {
            this.normals = normals;
        }

        public Vector3[] GetTangents()
        {
            return tangents;
        }

        public void SetTangents(Vector3[] tangents)
        {
            this.tangents = tangents;
        }

        public Vector3[] GetUv()
        {
            return uv;
        }

        public void SetUv(Vector3[] uv)
        {
            this.uv = uv;
        }
          
    }
}