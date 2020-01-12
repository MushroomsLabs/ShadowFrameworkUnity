using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.CurvedPoly;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes; 

namespace MLab.ShadowFramework.Tests
{
    class Demo006_CreateColliderMeshTypeA : CPRuntimeDemo, CPAssetGrabber, InterpolationSchemaMap
    {
        private CurvedPolyAsset asset;

        public string GetName() {
            return "Create Collider Mesh Type A";
        } 

        public void SetAsset(CurvedPolyAsset asset)
        {
            this.asset = asset;
        }

        public int GetMappedInterpolatorId(int id)
        {
            return SFGouraudSchemaBuilder.GOURAUD_SCHEMA_ID;
        }

        public IGuideModel GetGuideModel() {
            return null;
        }

        public void Test(ITestAssert testAssert)
        {
            testAssert.CallTest(GetName());
        }

        public void BuildModel(GameObject gameObject)
        { 
            CurvedPolygonsNet cpnet = GetPolylinesCPNet(asset.GetCPN());
             
            short[] loqs = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            CPNTessellationProcess tessellationProcess = ProcessesKeeper.GetTessellationProcess();

            TessellationOutput output = tessellationProcess.InitProcess(cpnet, loqs, this);

            //Debug.Log("cpnet.GetGeometriesCount() " + cpnet.GetGeometriesCount());

            tessellationProcess.BuildProfile();

            int[] builtTrianglesCount = output.GetBuiltTrianglesSize();
            int builtVerticesCount = output.GetBuiltVerticesSize();

            Vector2[] uvs_ = new Vector2[builtVerticesCount];
            Vector3[] vertices_ = new Vector3[builtVerticesCount];
            Vector3[] normals_ = new Vector3[builtVerticesCount];
            int[][] indices_ = new int[builtTrianglesCount.Length][];
            //Debug.Log("indices_ " + indices_.Length);
            for (int i = 0; i < builtTrianglesCount.Length; i++) {
                indices_[i] = new int[builtTrianglesCount[i] * 3];
                //Debug.Log("indices_[" + i + "] " + indices_[i].Length);
            }

            OutputMesh mesh = null;
            
            mesh = new OutputMesh(vertices_, uvs_, normals_, indices_);
            tessellationProcess.WriteMesh(mesh);
             
            MeshAssigner.AssignMesh(gameObject, vertices_, normals_, uvs_, indices_);
        }

        private CurvedPolygonsNet GetPolylinesCPNet(CurvedPolygonsNet otherNet)
        {

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> uvs = new List<Vector3>();

            int numberOfVertices = otherNet.GetNumberOfVertices();
            for (int i = 0; i < otherNet.GetNumberOfVertices(); i++)
            {
                vertices.Add(otherNet.GetVertices()[i]);
                normals.Add(otherNet.GetNormals()[i]);
                uvs.Add(otherNet.GetUv()[i]);
            }

            int numberOfEdges = otherNet.GetEdgesCount();
            short[] edges = otherNet.GetEdges();
             

            List<short> ps = new List<short>();
            List<short> psIndex = new List<short>();
            psIndex.Add(0);

            for (int i = 0; i < numberOfEdges; i++)
            {
                int ePos = otherNet.GetEdgePosition(i);
                Vector3 A = otherNet.GetVertices()[edges[ePos]];
                Vector3 AB = otherNet.GetVertices()[edges[ePos + 1]];
                Vector3 BA = otherNet.GetVertices()[edges[ePos + 2]];
                Vector3 B = otherNet.GetVertices()[edges[ePos + 3]];

                Vector3 Auv = otherNet.GetUv()[edges[ePos]];
                Vector3 ABuv = otherNet.GetUv()[edges[ePos + 1]];
                Vector3 BAuv = otherNet.GetUv()[edges[ePos + 2]];
                Vector3 Buv = otherNet.GetUv()[edges[ePos + 3]];

                Vector3 An = otherNet.GetNormals()[edges[ePos]];
                Vector3 Bn = otherNet.GetNormals()[edges[ePos + 3]];
                Vector3 ABn = otherNet.GetNormals()[numberOfVertices + i];

                vertices.Add(0.42f * A + 0.42f * AB + 0.144f * BA + 0.016f * B);
                vertices.Add(0.125f * B + 0.375f * BA + 0.375f * AB + 0.125f * A);
                vertices.Add(0.42f * B + 0.42f * BA + 0.144f * AB + 0.016f * A);

                uvs.Add(0.42f * Auv + 0.42f * ABuv + 0.144f * BAuv + 0.016f * Buv);
                uvs.Add(0.125f * Buv + 0.375f * BAuv + 0.375f * ABuv + 0.125f * Auv);
                uvs.Add(0.42f * Buv + 0.42f * BAuv + 0.144f * ABuv + 0.016f * Auv);

                normals.Add(0.56f * An + 0.376f * ABn + 0.062f * Bn);
                normals.Add(0.25f * An + 0.5f * ABn + 0.25f * Bn);
                normals.Add(0.56f * Bn + 0.376f * ABn + 0.062f * An);

                int id = numberOfVertices + 3 * i;
                ps.Add(edges[ePos]);
                ps.Add((short)id);
                ps.Add((short)(id + 1));
                ps.Add((short)(id + 2));
                ps.Add(edges[ePos + 3]);

                psIndex.Add((short)((i + 1) * 5));
            }


            CurvedPolygonsNet cPNet = new CurvedPolygonsNet();
            cPNet.SetNumberOfVertices(vertices.Count);
            cPNet.SetVertices(vertices.ToArray());
            cPNet.SetNormals(normals.ToArray());
            cPNet.SetUv(uvs.ToArray());
            cPNet.setPolylines(numberOfEdges, ps.ToArray(), psIndex.ToArray());
            cPNet.SetGeometries(otherNet.GetGeometriesCount(), otherNet.GetGeometries());

            return cPNet;
        }
    }
}
