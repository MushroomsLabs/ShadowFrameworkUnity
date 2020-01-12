using UnityEngine;
using UnityEditor;
using MLab.CurvedPoly;

namespace MLab.ShadowFramework.Tests
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class CPRuntimeDemoBehaviour : MonoBehaviour
    {
        private CPRuntimeDemo[] demos;
        public CurvedPolyAsset asset;
        public int selected;

        public CPRuntimeDemoBehaviour() {
            CPRuntimeDemo[] demos = {
                new Demo001_CurvedPolyTriangleAtRuntime(),
                new Demo002_CurvedPolyTriangleWithPolylines(),
                new Demo003_CreateMeshFromAsset(),
                new Demo004_ConvertEdgesToPolylines(),
                new Demo005_UpdateMesh(),
                new Demo006_CreateColliderMeshTypeA(),
                new Demo007_CurvedPolyTriangleWithTangents(),
                new Demo008_CurvedPolyTriangleWithoutUvs(),
                new Demo009_CurvedPolyTriangleWithoutNormals(),
                new Demo010_CurvedPolyTriangleWithProperty3()
            };
            this.demos = demos;
        }

        public void Execute()
        {
            CPRuntimeDemo demo = demos[selected];
            CPAssetGrabber assetGrabber = demo as CPAssetGrabber;
            if (asset != null || assetGrabber==null) {
                if(assetGrabber!=null)
                    assetGrabber.SetAsset(asset);
                demo.BuildModel(gameObject);
            }
        }

        public void ExecuteAllTest()
        {
            if (asset != null)
            {
                /*Change this DebugLogTestAssert with a ITestAssert 
                 * of your own implementation if you need to 
                 * use a different tests tool.*/
                ITestAssert logTestAssert = new DebugLogTestAssert();
                for (int i=0;i<demos.Length;i++) {
                    if(demos[i] is CPAssetGrabber)
                        (demos[i] as CPAssetGrabber).SetAsset(asset);
                    demos[i].Test(logTestAssert);
                }
            }
        }

        public string[] GetNames(){
            string[] names = new string[demos.Length];
            for (int i=0;i<names.Length;i++) {
                names[i] = "("+FormatNumber(i+1)+")"+demos[i].GetName();
            }
            return names;
        }

        private string FormatNumber(int number)
        {
            if (number < 10)
                return "00" + number;
            else if (number < 100)
                return "0" + number;
            else return "" + number;
        }

        public bool NeedsAsset() {
            return demos[selected] is CPAssetGrabber;
        }
    }
     
    [CustomEditor(typeof(CPRuntimeDemoBehaviour))]
    public class CurvedPolyEditor : Editor
    {
        private bool useAsset;

        public override void OnInspectorGUI()
        {
            CPRuntimeDemoBehaviour demoB = (CPRuntimeDemoBehaviour)target;

            if (Event.current.type == EventType.Layout)
                this.useAsset = demoB.NeedsAsset();

            demoB.selected = EditorGUILayout.Popup(demoB.selected, 
                   demoB.GetNames());

            if (useAsset)
            {
                GUILayout.Label("Select an asset for this Demo:");
                demoB.asset = (CurvedPolyAsset) EditorGUILayout.ObjectField(demoB.asset, 
                    typeof(CurvedPolyAsset),true);
            }

            if (demoB.asset == null)
                GUI.enabled = false;

            if (GUILayout.Button("Build Demo Mesh")) {
                demoB.Execute();
            }

            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 2; 
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            if (GUILayout.Button("Execute All Test"))
            {
                demoB.ExecuteAllTest();
            }

            GUI.enabled = true;
        }
    } 
}
