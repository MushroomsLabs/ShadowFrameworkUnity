using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.CurvedPoly;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;

namespace MLab.ShadowFramework.Tests
{
    
    class Demo004_ConvertEdgesToPolylines : CPRuntimeDemo, CPAssetGrabber
    {
        private Vector2[] uvs_;
        private Vector3[] vertices_;
        private Vector3[] normals_;
        private int[][] indices_;

        private CurvedPolyAsset asset;

        public string GetName()
        {
            return "Convert Edges To Polylines";
        } 

        public void SetAsset(CurvedPolyAsset asset)
        {
            this.asset = asset;
        }

        public void Test(ITestAssert testAssert)
        {
            testAssert.CallTest(GetName());
            Execute();
            testAssert.AssertEquals(indices_.Length, asset.GetCPN().GetGeometriesCount(), "Count Geometries");
            
        }

        public void BuildModel(GameObject gameObject)
        { 
            Execute();
            MeshAssigner.AssignMesh(gameObject, vertices_, normals_, uvs_, indices_);
        }

        public void Execute() {

            CurvedPolygonsNet cpnet = GetPolylinesCPNet(asset.GetCPN());

            short[] loqs = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            CPNTessellationProcess tessellationProcess = ProcessesKeeper.GetTessellationProcess();

            TessellationOutput output = tessellationProcess.InitProcess(cpnet, loqs);
             
            tessellationProcess.BuildProfile();

            int[] builtTrianglesCount = output.GetBuiltTrianglesSize();
            int builtVerticesCount = output.GetBuiltVerticesSize();

            uvs_ = new Vector2[builtVerticesCount];
            vertices_ = new Vector3[builtVerticesCount];
            normals_ = new Vector3[builtVerticesCount];
            indices_ = new int[builtTrianglesCount.Length][];
            for (int i = 0; i < builtTrianglesCount.Length; i++)
            {
                indices_[i] = new int[builtTrianglesCount[i] * 3];
            }

            OutputMesh mesh = null;
             
            mesh = new OutputMesh(vertices_, uvs_, normals_, indices_);
            tessellationProcess.WriteMesh(mesh);

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
