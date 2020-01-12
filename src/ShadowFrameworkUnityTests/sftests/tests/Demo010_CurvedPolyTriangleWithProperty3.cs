using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.CurvedPoly;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;

namespace MLab.ShadowFramework.Tests
{
    class Demo010_CurvedPolyTriangleWithProperty3 : CPRuntimeDemo
    {
        private OutputMesh mesh;

        public string GetName() {
            return "CurvedPoly Triangle Without Property3";
        } 

        public void Test(ITestAssert testAssert) {
            testAssert.CallTest(GetName());
            Execute();
            testAssert.AssertEquals(mesh.GetVertices().Length, 10, "Mesh Vertices");
            testAssert.AssertEquals(mesh.GetNormals().Length, 10, "Mesh Normals");
            testAssert.AssertEquals(mesh.GetUVs().Length, 10, "Mesh UVs");
            testAssert.AssertEquals(mesh.GetTangents().Length, 10, "Mesh Tangents");
            testAssert.AssertEquals(mesh.GetTriangles()[0].Length, 27, "Mesh Indices");
        }

        public void BuildModel(GameObject gameObject)
        {
            Execute();
            MeshAssigner.AssignMesh(gameObject, mesh);
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

            Vector3[] uv2s = {
                new Vector3(2,0,0),
                new Vector3(3,0,0),
                new Vector3(2,1,0),
                new Vector3(2+DemoUtils.ONE_THIRD,0,0),
                new Vector3(2+2*DemoUtils.ONE_THIRD,0,0),
                new Vector3(2+2*DemoUtils.ONE_THIRD,DemoUtils.ONE_THIRD,0),
                new Vector3(2+DemoUtils.ONE_THIRD,2*DemoUtils.ONE_THIRD,0),
                new Vector3(2,2*DemoUtils.ONE_THIRD,0),
                new Vector3(2,DemoUtils.ONE_THIRD,0),
            };
            Vector3[][] property3 = new Vector3[1][];
            property3[0] = uv2s;
            cpnet.SetProperty3(property3);

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
            
            this.mesh = new OutputMesh();
            this.mesh.SetupStructure(true, true, true, 1);
            this.mesh.Build(builtVerticesCount, builtTrianglesCount);
            tessellationProcess.WriteMesh(mesh);

            //Debug.Log("Tangents " + DemoUtils.Vector3sToString(mesh.GetTangents()));
        }
    }
}
