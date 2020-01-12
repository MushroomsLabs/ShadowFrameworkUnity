﻿using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.CurvedPoly;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;

namespace MLab.ShadowFramework.Tests
{
    class Demo001_CurvedPolyTriangleAtRuntime : CPRuntimeDemo
    {
        private Vector2[] uvs_;
        private Vector3[] vertices_;
        private Vector3[] normals_;
        private int[][] indices_;

        public string GetName() {
            return "CurvedPoly Triangle At Runtime";
        } 

        public void Test(ITestAssert testAssert) {
            testAssert.CallTest(GetName());
            Execute();
            testAssert.AssertEquals(vertices_.Length, 10, "Mesh Vertices");
            testAssert.AssertEquals(normals_.Length, 10, "Mesh Normals");
            testAssert.AssertEquals(indices_[0].Length, 27, "Mesh Indices");
        }

        public void BuildModel(GameObject gameObject)
        {
            Execute();
            MeshAssigner.AssignMesh(gameObject, vertices_, normals_, uvs_, indices_);
        }

        private void Execute() {

            CurvedPolygonsNet cpnet = new CurvedPolygonsNet();

            cpnet.SetNumberOfVertices(3);

            Vector3[] vertices = {
                new Vector3(0,0,0),
                new Vector3(1,0,0),
                new Vector3(0,1,0),
                new Vector3(DemoUtils.ONE_THIRD,0,0),
                new Vector3(2*DemoUtils.ONE_THIRD,0,0),
                new Vector3(2*DemoUtils.ONE_THIRD,DemoUtils.ONE_THIRD,0),
                new Vector3(DemoUtils.ONE_THIRD,2*DemoUtils.ONE_THIRD,0),
                new Vector3(0,2*DemoUtils.ONE_THIRD,0),
                new Vector3(0,DemoUtils.ONE_THIRD,0),
            };
            cpnet.SetVertices(vertices);
            Vector3[] uvs = {
                new Vector3(0,0,0),
                new Vector3(1,0,0),
                new Vector3(0,1,0),
                new Vector3(DemoUtils.ONE_THIRD,0,0),
                new Vector3(2*DemoUtils.ONE_THIRD,0,0),
                new Vector3(2*DemoUtils.ONE_THIRD,DemoUtils.ONE_THIRD,0),
                new Vector3(DemoUtils.ONE_THIRD,2*DemoUtils.ONE_THIRD,0),
                new Vector3(0,2*DemoUtils.ONE_THIRD,0),
                new Vector3(0,DemoUtils.ONE_THIRD,0),
            };
            cpnet.SetUv(uvs);

            Vector3[] normals = {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward
            };
            cpnet.SetNormals(normals);

            short[] edges = { 0, 3, 4, 1, 1, 5, 6, 2, 2, 7, 8, 0 };
            short[] edgesIndex = { 0, 4, 8, 12 };
            short[] edgesHints = { 3, 3, 3, 3, 3, 3 };
            float[] weights = { 1, 1, 1, 1, 1, 1 };

            cpnet.SetEdges(3, edges, edgesIndex, edgesHints, weights);

            short[] loqs = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            CPNGeometry[] geometries = { new CPNGeometry() };
            short[] polygons = { 1, 2, 3 };
            short[] polygonsIndex = { 0, 3 };
            short[] polygonsSchemas = { SFEdgeSurfaceSchemaBuilder.EDGE_SURFACE_SCHEMA_ID };
            geometries[0].Setup(1, polygonsIndex, polygons, polygonsSchemas);

            cpnet.SetGeometries(1, geometries);

            CPNTessellationProcess tessellationProcess = ProcessesKeeper.GetTessellationProcess();

            TessellationOutput output = tessellationProcess.InitProcess(cpnet, loqs);

            tessellationProcess.BuildProfile();

            int[] builtTrianglesCount = output.GetBuiltTrianglesSize();
            int builtVerticesCount = output.GetBuiltVerticesSize();

            this.uvs_ = new Vector2[builtVerticesCount];
            this.vertices_ = new Vector3[builtVerticesCount];
            this.normals_ = new Vector3[builtVerticesCount];
            this.indices_ = new int[builtTrianglesCount.Length][];
            for (int i = 0; i < builtTrianglesCount.Length; i++)
            {
                this.indices_[i] = new int[builtTrianglesCount[i] * 3];
            }

            OutputMesh mesh = new OutputMesh(vertices_, uvs_, normals_, indices_);
            tessellationProcess.WriteMesh(mesh); 
        }
    }
}
