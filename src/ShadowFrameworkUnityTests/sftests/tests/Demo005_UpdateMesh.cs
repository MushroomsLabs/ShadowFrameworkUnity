using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.CurvedPoly;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;

namespace MLab.ShadowFramework.Tests
{
    class Demo005_UpdateMesh : CPRuntimeDemo, CPAssetGrabber
    {
        private Vector2[] uvs_;
        private Vector3[] vertices_;
        private Vector3[] normals_;
        private int[][] indices_;
        private OutputMesh mesh2;
        private OutputMesh outMesh;

        private CurvedPolyAsset asset;

        public string GetName() {
            return "Update Mesh";
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
            MeshAssigner.AssignMesh(gameObject, mesh2.GetVertices(), mesh2.GetNormals(),
                mesh2.GetUVs(), outMesh.GetTriangles());
        }

        public void Execute() {

            CurvedPolygonsNet cpnet = asset.GetCPN();

            CurvedPolyVariants cpnVariants = new CurvedPolyVariants();
            cpnVariants.SetCPN(cpnet);

            short[] loqs = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            CPNTessellationProcess tessellationProcess = ProcessesKeeper.GetTessellationProcess();

            TessellationOutput output = tessellationProcess.InitProcess(cpnet, loqs);

            //Debug.Log("cpnet.GetGeometriesCount() " + cpnet.GetGeometriesCount());

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

            //here
            int id = cpnVariants.GetFreeTessellationRecordId();
            cpnVariants.SetRecord(id, new OutputMesh(vertices_, uvs_, normals_, indices_), output);

            //this.outMesh = cpnVariants.GetMeshOutput(id).GetNewCloneVariant();

            CPNSubset subsSet = new CPNSubset();

            TessellationOutput output2 = tessellationProcess.InitProcess(cpnet, loqs, subsSet);

            this.mesh2 = new OutputMesh(outMesh.GetVertices(), outMesh.GetUVs(),
                outMesh.GetNormals(), outMesh.GetTriangles());
            tessellationProcess.WriteMesh(mesh2);
        }
    }
}
