using UnityEngine; 
using MLab.CurvedPoly;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;

namespace MLab.ShadowFramework.Tests
{
    class Demo003_CreateMeshFromAsset : CPRuntimeDemo, CPAssetGrabber
    {
        private Vector2[] uvs_;
        private Vector3[] vertices_;
        private Vector3[] normals_;
        private int[][] indices_;

        private CurvedPolyAsset asset;

        public string GetName()
        {
            return "Create Mesh From Curved Poly Asset";
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

            CurvedPolygonsNet cpnet = asset.GetCPN();

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
    }
}
