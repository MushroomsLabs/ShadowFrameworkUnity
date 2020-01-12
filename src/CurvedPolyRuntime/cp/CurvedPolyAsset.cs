using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using MLab.ShadowFramework; 
using MLab.ShadowFramework.Data;
using MLab.ShadowFramework.Processes;

namespace MLab.CurvedPoly
{
     
    [CreateAssetMenu(fileName = "NewCurvedPolyAsset", menuName = "CurvedPoly/CurvedPolyAsset", order = 2)]
    [PreferBinarySerialization]
    public class CurvedPolyAsset : ScriptableObject{

        [Serializable]
        public class CPGeometryAsset { 
            public byte[] polygons;
            public byte[] polygonsIndex;
            public byte[] polygonsSchema;
        }

        //public bool useUnityNormals = false;

        private const float FLOAT_PRECISION = 0.0001f;

        public const short DEFAULT_COMPRESSION_MASK = 0x411;

#if CP_DEBUG_TOOLS
        //[HideInInspector]
        public short compressionMask;
        //[HideInInspector]
        public byte[] vertices;
        //[HideInInspector]
        public int numberOfVertices;
        //[HideInInspector]
        public byte[] uvs;
        //[HideInInspector]
        public byte[] normals;
        //[HideInInspector]
        public byte[] tangents;
        //[HideInInspector]
        public byte[] edges;
        //[HideInInspector]
        public byte[] edgesIndex;
        //[HideInInspector]
        public byte[] edgeHints; 
        //[HideInInspector]
        public byte[] edgeWeights;
        //[HideInInspector]
        public CPGeometryAsset[] geometries;
#else
        [HideInInspector]
        public short compressionMask;
        [HideInInspector]
        public byte[] vertices;
        [HideInInspector]
        public int numberOfVertices;
        [HideInInspector]
        public byte[] uvs;
        [HideInInspector]
        public byte[] normals;
        [HideInInspector]
        public byte[] tangents;
        [HideInInspector]
        public byte[] edges;
        [HideInInspector]
        public byte[] edgesIndex;
        [HideInInspector]
        public byte[] edgeHints; 
        [HideInInspector]
        public byte[] edgeWeights;
        [HideInInspector]
        public CPGeometryAsset[] geometries;
#endif
         
        private CurvedPolyMeshItemDB meshItemDB = new CurvedPolyMeshItemDB();
        
        //private CPNTessellationProcess tessellationProcess;

        public CurvedPolyAsset() { 
            //tessellationProcess = new CPNTessellationProcess(new SFDefaultInterpolationManager());
            WritePolygonsNet(meshItemDB.GetCPN());
            meshItemDB.MarkChanged();
        }
         
        public void OnEnable() {
            //if(tessellationProcess!=null)
                ReadPolygonsNet(meshItemDB.GetCPN());
        }

        CurvedPolyMeshItemDB GetItemDB() {
            return meshItemDB;
        }

        public long GetChangedTimestamp() {
            return meshItemDB.GetChangedTimestamp();
        }

        public TessellationOutput GetTessellationOutput(LoDs lods,int index) {
            return meshItemDB.GetTessellationOutput(lods, index);
        }

        public void MarkChanged()
        {
            //Debug.Log("Marked Changed!!");
            meshItemDB.MarkChanged();
        } 

        public CurvedPolygonsNet GetCPN() { 
            return meshItemDB.GetCPN();
        }

        public void RecomputeShape(LoDs lods, int lodsIndex, CPNSubset subset) {
            meshItemDB.RecomputeShape(lods, lodsIndex, subset);
        }

        public Mesh GetMesh(LoDs lods,int lodsIndex,bool dynamicMode, Mesh input=null) {
             
            Mesh mesh = meshItemDB.GetMesh(lods, lodsIndex, dynamicMode, input/*, useUnityNormals*/);
            
            return mesh;
        }

        public void WriteBackData() {
            WritePolygonsNet(meshItemDB.GetCPN());
        }

        public void ReadData()
        {
            ReadPolygonsNet(meshItemDB.GetCPN());
        }

        public void WritePolygonsNet(CurvedPolygonsNet net) {

            //Debug.Log("On write this.compressionMask:" + this.compressionMask);
            if (compressionMask == 0) {
                compressionMask = DEFAULT_COMPRESSION_MASK; 
                /*Debug.Log("Since it was 0 we set it to a new Value this.compressionMask:" + this.compressionMask);
                Debug.Log("Where Vertices Compression IS:" + GetVertexCompressionMode());
                Debug.Log("Where UV Compression IS:" + GetUVCompressionMode());*/
            }
             
            this.numberOfVertices = net.GetNumberOfVertices(); 
            this.vertices = CPVertexArrayData.CompressVertexArray(net.GetVertices(), GetVertexCompressionMode());
            this.uvs = CPUVArrayData.compressUVArray(net.GetUv(), GetUVCompressionMode());
            //Debug.Log("On Write, UVs:" + ToString(net.GetUv()));
            this.normals = CPVectorArrayData.compressVectorArray(net.GetNormals(), GetVectorCompressionMode());
            this.tangents = new byte[0];
            this.edges = CPShortArrayData.CompressShortsArray(net.GetEdges());
            this.edgesIndex = CPShortArrayData.CompressShortsArray(net.GetEdgesIndex());
            this.edgeHints = CPShortArrayData.CompressShortsArray(net.GetEdgeHints());
            //this.edgeRots = CPFloatArrayData.compressFloatsArray(net.GetEdgesRots(), FLOAT_PRECISION);
            //this.edgeThickness = CPFloatArrayData.compressFloatsArray(net.GetEdgesThickness(), FLOAT_PRECISION);
            this.edgeWeights = CPFloatArrayData.compressFloatsArray(net.GetEdgeWeights(), FLOAT_PRECISION);
            this.geometries = new CPGeometryAsset[net.GetGeometriesCount()];
            for (int i = 0; i < geometries.Length; i++)
            {
                geometries[i] = new CPGeometryAsset {
                    polygons = CPShortArrayData.CompressShortsArray(net.GetGeometries()[i].GetPolygons()),
                    polygonsIndex = CPShortArrayData.CompressShortsArray(net.GetGeometries()[i].GetPolygonsIndex()),
                    polygonsSchema = CPShortArrayData.CompressShortsArray(net.GetGeometries()[i].GetPolygonsSchemas())
                };
            }
        }
        /*
        private string ToString(Vector3[] data) {
            string str = "{";
            for (int i=0;i<data.Length;i++) {
                str += "(" +data[i].x+" ," + data[i].y + "," + data[i].z + ")";
            }
            return str+"}";
        }*/

        public byte[] ShortBytes(int shortV) {
            return new byte[] { (byte)((shortV & 0xff00)>>8),(byte)(shortV & 0xff)};
        }

        public byte[] IntBytes(int intV)
        {
            return new byte[] {
                 (byte)((intV & 0xff000000) >> 24), (byte)((intV & 0xff0000) >> 16),
                (byte)((intV & 0xff00) >> 8), (byte)(intV & 0xff) };
        }

        public void ReadPolygonsNet(CurvedPolygonsNet net) {

            //Debug.Log("this.precision "+this.compressionMask);
            net.SetVertices(CPVertexArrayData.GetCompressedVertexArray(vertices, GetVertexCompressionMode()));
            
            net.SetUv(CPUVArrayData.getCompressedUVArray(uvs, GetUVCompressionMode()));
            
            net.SetNormals(CPVectorArrayData.getCompressedVectorArray(normals, GetVectorCompressionMode()));
            //net.setTangents(CPVectorArrayData.getCompressedVectorArray(vertices, getVectorCompressionMode()));
            short[] edges = CPShortArrayData.GetCompressedShortsArray(this.edges);
            short[] edgesIndex = CPShortArrayData.GetCompressedShortsArray(this.edgesIndex);
            int edgesSize = edgesIndex.Length - 1;
            net.SetNumberOfVertices(numberOfVertices);
            net.SetEdges(edgesIndex.Length - 1, edges, edgesIndex,
                    CPShortArrayData.GetCompressedShortsArray(edgeHints), 
                    CPFloatArrayData.getCompressedFloatsArray(edgeWeights, FLOAT_PRECISION));
            CPNGeometry[] geometries = new CPNGeometry[this.geometries.Length];
            for (int i = 0; i < geometries.Length; i++) {
                geometries[i] = new CPNGeometry();
                short[] polygonsIndex = CPShortArrayData.GetCompressedShortsArray(this.geometries[i].polygonsIndex);
                short[] polygons = CPShortArrayData.GetCompressedShortsArray(this.geometries[i].polygons);
                short[] polygonsSchemas = null;
                if (this.geometries[i].polygonsSchema != null && this.geometries[i].polygonsSchema.Length != 0)
                    polygonsSchemas = CPShortArrayData.GetCompressedShortsArray(this.geometries[i].polygonsSchema);
                else {
                    polygonsSchemas = new short[polygonsIndex.Length-1];
                }
                int polygonsCount = polygonsIndex.Length-1;
                geometries[i].Setup((short)polygonsCount,polygonsIndex,polygons, polygonsSchemas);
            }
            net.SetGeometries(geometries.Length, geometries);
        }
        

        private CPVertexCompressionMode GetVertexCompressionMode() {
            int vCompressionModeIndex = compressionMask & 0xf;
            switch (vCompressionModeIndex)
            {
                case 0: return CPVertexCompressionMode.MILLI;
                case 1: return CPVertexCompressionMode.MILLI;
                case 2: return CPVertexCompressionMode.DECIMILLI;
                case 3: return CPVertexCompressionMode.CENTI;
            }
            return CPVertexCompressionMode.DECI;
        }

        private CPVectorCompressionMode GetVectorCompressionMode()
        {
            int vCompressionModeIndex = (compressionMask & 0xf0) >> 4;
            switch (vCompressionModeIndex)
            {
                case 0: return CPVectorCompressionMode.MEDIUM_UNIT_PRECISION;
                case 1: return CPVectorCompressionMode.MEDIUM_UNIT_PRECISION;
                case 2: return CPVectorCompressionMode.HIGH_UNIT_PRECISION; 
            }
            return CPVectorCompressionMode.LOW_UNIT_PRECISION;
        }

        private CPUVCompressionMode GetUVCompressionMode()
        {
            int vCompressionModeIndex = (compressionMask & 0xf00) >> 8;
            switch (vCompressionModeIndex)
            {
                case 0: return CPUVCompressionMode.ONE_ON_10;
                case 1: return CPUVCompressionMode.ONE_ON_10;
                case 2: return CPUVCompressionMode.ONE_ON_100;
                case 3: return CPUVCompressionMode.ONE_ON_1000;
                case 4: return CPUVCompressionMode.ONE_ON_10000;
                case 5: return CPUVCompressionMode.ONE_ON_128;
                case 6: return CPUVCompressionMode.ONE_ON_256;
                case 7: return CPUVCompressionMode.ONE_ON_512;
                case 8: return CPUVCompressionMode.ONE_ON_1024;
                case 9: return CPUVCompressionMode.ONE_ON_2048;
                case 10: return CPUVCompressionMode.ONE_ON_4096;
                case 11: return CPUVCompressionMode.ONE_ON_8192;
                default: return CPUVCompressionMode.ONE_ON_16384;
            }
        }

        public CurvedPolyAsset Clone() {
            CurvedPolyAsset curvedPolyAsset = ScriptableObject.CreateInstance<CurvedPolyAsset>();
            //curvedPolyAsset.tessellationProcess = new CPNTessellationProcess(new SFDefaultInterpolationManager());
            curvedPolyAsset.compressionMask = this.compressionMask;

            curvedPolyAsset.vertices = this.vertices;
            curvedPolyAsset.numberOfVertices = this.numberOfVertices;
            curvedPolyAsset.uvs = this.uvs;
            curvedPolyAsset.normals = this.normals;
            curvedPolyAsset.tangents = this.tangents;
            curvedPolyAsset.edges = this.edges;
            curvedPolyAsset.edgesIndex = this.edgesIndex;
            curvedPolyAsset.edgeHints = this.edgeHints;
            curvedPolyAsset.edgeWeights = this.edgeWeights;
            curvedPolyAsset.geometries = this.geometries;
            curvedPolyAsset.ReadPolygonsNet(curvedPolyAsset.GetCPN());

            return curvedPolyAsset;
        }

        public int BytesSize() {
            int geomBytesSize = 0;

            if (geometries == null || vertices==null)
                return -1;

            for (int i=0;i<geometries.Length;i++) {
                geomBytesSize += 4+geometries[i].polygons.Length
                    + geometries[i].polygonsIndex.Length
                    + geometries[i].polygonsSchema.Length ;
            }

            return 4 + vertices.Length + uvs.Length + normals.Length + tangents.Length + edges.Length + edgesIndex.Length + edgeHints.Length +
                edgeWeights.Length + geomBytesSize;
        }

        public void ClearMeshes(LoDs lods) {
            meshItemDB.ClearMeshes(lods);
        }
    }
}
